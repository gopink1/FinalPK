using GamePlayer;
using System.Collections.Generic;
using UnityEngine;
public class ClientFrameSystem : IGameSystem
{
    public LocalPlayerControl m_LocalPlayer;

    // 存储其他玩家控制器的字典 (Key: PlayerID)
    private Dictionary<int, RemotePlayerControl> remotePlayers = new Dictionary<int, RemotePlayerControl>();

    // --- 帧同步核心变量 ---
    public int MyClientId { get; private set; } = -1;
    public int ServerFrame { get; private set; } = 0;

    // 逻辑帧间隔 (跟服务端保持一致 66ms)
    private const float LogicFrameInterval = 0.066f;
    private float timerAccumulator = 0f;

    // 【新增】换边倒计时相关
    private bool _isRoundEnding = false;
    private float _roundEndTimer = 0f;
    private float _roundEndDelay = 2.0f; // 子弹打完后等待2秒再换
    // 用这个变量判断是否处于“等待换边中”
    // 如果 _targetSwapFrame != -1，说明正在倒计时，此时应该锁定输入
    public bool IsInTransition => _targetSwapFrame != -1;
    // 辅助方法：暴露 RemotePlayers 给 BattleInitSystem 用
    public Dictionary<int, RemotePlayerControl> GetRemotePlayers() { return remotePlayers; }

    // --- 预测相关 ---
    private InputData _currentInput; // 当前帧的输入缓存
    private int _clientPredictFrame = 0; // 客户端当前的预测帧号

    // 预测历史队列 (用于回滚校验)
    private Queue<FramePredictionState> _predictionStates = new Queue<FramePredictionState>();

    // 本地玩家在服务端的权威状态 (用于作为校验的基准)
    private PlayerState _lastAuthoritativeState;

    // 输入延迟帧数 (用于平滑网络抖动)
    public int InputDelayFrames = 2;
    // 【新增】记录上一帧权威的墙壁状态
    private WallSnapshot _lastAuthoritativeWallState;

    // 【新增】换边目标帧号 (-1 代表无待定换边)
    private int _targetSwapFrame = -1;
    // 换边等待的帧数 (例如 1.5秒 / 0.066ms ≈ 22帧，保险起见设 30帧)
    private const int SwapDelayFrames = 30;
    // 标记游戏是否结束，防止重复触发
    public bool IsGameOver { get; private set; } = false;
    public bool IsRunning { get; private set; } = false;
    public ClientFrameSystem(GameBase gameBase) : base(gameBase)
    {
        
    }
    /// <summary>
    /// 初始化游戏 (由 BattleManager 调用)
    /// </summary>
    public void InitGame(int myId, Vector3 startPos, Quaternion startRot, bool resetFrameCounter = true, int initialHP = 100)
    {
        MyClientId = myId;
        // 【关键修改】只有游戏刚开始才重置帧号，换边时不重置
        // 1. 处理帧号逻辑
        if (resetFrameCounter)
        {
            // 游戏冷启动
            ServerFrame = 0;
            _clientPredictFrame = 0;
            _lastAuthoritativeWallState = new WallSnapshot(new Dictionary<int, int>());
        }
        else
        {
            // 【核心修复】换边热启动
            // 必须保持预测领先！不能等于 ServerFrame，必须加上 InputDelayFrames
            // 否则你发出的 Input 会被视为“过去帧”而失效，导致角色卡住
            _clientPredictFrame = ServerFrame + InputDelayFrames;

            // 注意：不要重置 _lastAuthoritativeWallState
        }
        timerAccumulator = 0f;

        // 清理历史
        _predictionStates.Clear();
        remotePlayers.Clear();

        // 初始化基准状态
        _lastAuthoritativeState = PlayerState.CreateSpawn(startPos, startRot);
        // 2. 【核心修复】应用传入的 HP 到权威基准
        // 这样 Correction 逻辑就会以这个 HP 为准，而不是默认的 100
        _lastAuthoritativeState = _lastAuthoritativeState.WithHP(initialHP);
        // 【关键】换边时不应该重置墙壁快照，应该保留当前的
        if (resetFrameCounter)
        {
            _lastAuthoritativeWallState = new WallSnapshot(new Dictionary<int, int>());
        }
        IsRunning = true;
        Debug.Log($"[ClientFrameManager] 启动成功，ID: {MyClientId}");
    }
    /// <summary>
    /// 注册远程玩家 (由 BattleManager 调用)
    /// </summary>
    /// <param name="id">玩家的 SocketID</param>
    /// <param name="control">远程玩家控制器脚本引用</param>
    public void RegisterRemotePlayer(int id, RemotePlayerControl control)
    {
        if (remotePlayers == null)
        {
            remotePlayers = new Dictionary<int, RemotePlayerControl>();
        }

        if (!remotePlayers.ContainsKey(id))
        {
            remotePlayers.Add(id, control);
            Debug.Log($"[ClientFrameSystem] 注册远程玩家: {id}");
        }
        else
        {
            // 如果已存在（可能是重连或逻辑错误），更新引用
            remotePlayers[id] = control;
        }
    }
    public override void Init()
    {
        
    }

    public override void Release()
    {
        
    }

    public override void Update()
    {
        if (!IsRunning || m_LocalPlayer == null) return;

        // 1. 收集本地输入 (委托给 LocalPlayerControl 内部的 Strategy)
        CollectLocalInput();

        // 2. 渲染平滑 (插值)
        m_LocalPlayer.UpdateMovement();
        foreach (var p in remotePlayers.Values)
        {
            p.UpdateMovement();
        }

        // 3. 驱动预测逻辑帧
        timerAccumulator += Time.deltaTime;
        while (timerAccumulator >= LogicFrameInterval)
        {
            PredictNextFrame();
            timerAccumulator -= LogicFrameInterval;
        }
    }

    #region 收集输入
    private void CollectLocalInput()
    {
        // 懒加载
        if (_currentInput == null) _currentInput = new InputData { Id = MyClientId };

        // 1. 如果处于过渡期（等待换边），强制输入归零
        if (IsInTransition)
        {
            _currentInput.AxisX = _currentInput.AxisX;
            _currentInput.AxisY = _currentInput.AxisY; // 如果有俯仰角，也可以设为当前角度保持不动，或者归零平视
            _currentInput.Action = 0; // 禁止开枪、蹲下
            // 注意：不要 return，因为我们需要发送这个“空操作”给服务端
            // 这样服务端和对手才知道“我停下来了”
        }
        else
        {
            // 2. 正常游戏，采集真实输入
            m_LocalPlayer.CollectInput(_currentInput);
        }
    }
    #endregion

    #region 预测
    /// <summary>
    /// 核心预测逻辑
    /// </summary>
    private void PredictNextFrame()
    {
        // 如果输入未准备好，跳过
        if (_currentInput == null) return;
        // 保险逻辑：如果预测帧落后于服务器（发生严重卡顿或重置失误），强制追赶
        if (_clientPredictFrame <= ServerFrame)
        {
            Debug.LogWarning($"[预测修正] 预测帧 {_clientPredictFrame} 落后于 服务端帧 {ServerFrame}，强制对齐。");
            _clientPredictFrame = ServerFrame + InputDelayFrames;
        }
        // 1. 初始化对齐
        // 如果还没开始预测，且服务端已经开始跑了，对齐到服务端未来几帧
        if (_clientPredictFrame == 0 && ServerFrame > 0)
        {
            _clientPredictFrame = ServerFrame + InputDelayFrames;
            // 首次同步时，强制将本地预测状态对齐到当前位置
            // 这里假设当前位置已经是正确的
        }

        // 还没收到过服务端帧，等待同步
        if (_clientPredictFrame == 0) return;

        // ==========================================
        // 【关键步骤】：重置所有人的逻辑状态
        // 确保本次预测是干净的
        // ==========================================
        m_LocalPlayer.ResetFrameLogic();
        foreach (var remote in remotePlayers.Values)
        {
            remote.ResetFrameLogic();
        }


        // 2. 标记帧号并发送
        _currentInput.GlobalFrame = _clientPredictFrame;
        SendInputToServer(_currentInput);

        // 3. 本地模拟移动 (调用 LocalPlayerControl 的预测方法)
        // PredictNextState 会更新 LocalPlayer 内部的 _clientPredictedState 并设置插值目标
        PlayerState predictedState = m_LocalPlayer.PredictNextState(_currentInput, LogicFrameInterval);
        // 2. 【关键】在记录快照前，确保墙壁状态被捕获
        WallSnapshot currentWallState = GameBase.MainInstance.wallSystem.GetSnapshot();
        // 4. 记录预测历史 (用于收到服务端确认后进行比对)
        _predictionStates.Enqueue(new FramePredictionState
        {
            FrameNumber = _clientPredictFrame,
            State = predictedState,
            WorldWallState = currentWallState // 存入历史

        });

        // 5. 帧号递增
        _clientPredictFrame++;
    }
    #endregion

    #region 发送消息
    private void SendInputToServer(InputData input)
    {
        // 构造消息
        InputMsg msg = new InputMsg();
        msg.playerID = MyClientId;
        // 注意：InputMsg 需要深拷贝数据，防止引用在下一帧被修改
        // 这里假设 NetAsyncMgr 会序列化发送，或者 InputData 是结构体/做了处理
        // 为了安全起见，新建一个 Data 对象发送
        msg.data = new InputData
        {
            Id = input.Id,
            GlobalFrame = input.GlobalFrame,
            AxisX = input.AxisX,
            AxisY = input.AxisY,
            Action = input.Action
        };
        // 【排查日志】
        if (input.AxisX != 0 || input.AxisY != 0)
        {
            Debug.Log($"[Local Send] Frame:{input.GlobalFrame} AxisX:{input.AxisX} AxisY:{input.AxisY}");
        }
        NetAsyncMgr.Instance.Send(msg);
    }
    #endregion

    /// <summary>
    /// 处理服务端下发的帧数据 (由 RoomNetSystem 调用)
    /// </summary>
    public void HandleServerFrame(AllInputData serverData)
    {
        if (!IsRunning || IsGameOver) return; // 如果游戏结束了，就不处理新帧了

        Debug.Log("开始处理服务端帧同步帧号为" + serverData.GlobalFrame);
        foreach (var kvp in serverData.AllClientInput)
        {
            if (kvp.Key != MyClientId) // 看看对手的数据
            {
                var input = kvp.Value;
                if (input.AxisX != 0 || input.AxisY != 0)
                {
                    Debug.Log($"[Hunter Recv Remote] ID:{kvp.Key} Frame:{serverData.GlobalFrame} X:{input.AxisX} Action:{input.Action}" );
                }
            }
        }
        // 更新权威帧号
        ServerFrame = serverData.GlobalFrame;
        //  【数据回滚】
        // 这一步非常快，只是把内存里的整数赋值回去。
        // Unity 场景里什么都没变，不会闪烁。
        GameBase.MainInstance.wallSystem.RestoreSnapshot(_lastAuthoritativeWallState);
        // =======================================================
        // 物理位置回滚 (Lag Compensation)
        // 1. 记录当前 Runner 在预测未来的位置
        Vector3 visualPos = m_LocalPlayer.transform.position;
        Quaternion visualRot = m_LocalPlayer.transform.rotation;

        // 2. 将 Runner 强行瞬移回“这一帧”的起点 (权威位置)
        // 这样 Hunter 的射线才能打中 Runner 的 Collider
        m_LocalPlayer.transform.position = _lastAuthoritativeState.Position;
        m_LocalPlayer.transform.rotation = _lastAuthoritativeState.Rotation;

        // 3. 【至关重要】强制刷新物理引擎
        // 移动 Transform 后，必须立刻同步给物理引擎，否则 Raycast 依然会检测旧位置
        Physics.SyncTransforms();
        // =======================================================
        Debug.Log($"[LagComp] 瞬移回滚。当前物理位置: {m_LocalPlayer.transform.position} | 目标权威位置: {_lastAuthoritativeState.Position}");

        // 4. 处理远程玩家 (Hunter 开火)
        // 此时 Runner 的 Collider 正好挡在 Hunter 的枪口线上，PendingDamage 会被正确加上
        ProcessRemotePlayers(serverData);

        // 5. 处理本地玩家回滚 (Runner 结算 PendingDamage -> 扣血)
        ProcessLocalPlayerCorrection(serverData);

        // 6. 恢复 Runner 的位置 (或者不恢复也行，因为 UpdateMovement 会在 Update 中覆盖它)
        // 为了安全起见，防止同一帧后续逻辑出错，建议恢复
        m_LocalPlayer.transform.position = visualPos;
        m_LocalPlayer.transform.rotation = visualRot;
        // 再次刷新，把Collider带回未来
        Physics.SyncTransforms();
        // =========================================================
        // 【核心修复】必须在这里告诉墙壁系统：“数据变了，请刷新画面”
        // =========================================================

        // 计算应该显示哪一帧的状态
        // 如果本地有预测（_predictionStates不为空），就显示本地预测到的最新时间（比如185帧）
        // 这样即使墙是在181帧碎的，因为 181 <= 185，墙也会显示为碎
        int displayFrame = ServerFrame;
        if (_predictionStates.Count > 0)
        {
            // 获取队列中最后一帧的帧号
            displayFrame = _predictionStates.ToArray()[_predictionStates.Count - 1].FrameNumber;
        }

        // 调试日志：检查这一步是否执行，以及帧号对不对
        // Debug.Log($"[ClientFrameSystem] 刷新墙壁表现。显示帧: {displayFrame}, Server帧: {ServerFrame}");

        // 强制刷新所有墙壁的 Renderer
        GameBase.MainInstance.wallSystem.RefreshVisuals(displayFrame);


        // 2. 【核心修改】在此处检测换边逻辑
        CheckRoundLogicInFrame();
        // 3. 【新增】检测游戏结束 (放在最后！)
        CheckGameOverLogic();
    }
    #region 检查游戏是否结束
    private void CheckGameOverLogic()
    {
        if (IsGameOver) return;

        // 1. 收集所有人的权威 HP (Local + Remote)
        // 这里的逻辑是：只要场上任何一个人的血量归零，游戏就结束

        int loserId = -1; // 输家的 ID

        // 检查自己 (Local)
        if (_lastAuthoritativeState.HP <= 0)
        {
            loserId = MyClientId;
        }
        else
        {
            // 检查别人 (Remote)
            foreach (var kvp in remotePlayers)
            {
                if (kvp.Value.GetAuthoritativeHP() <= 0)
                {
                    loserId = kvp.Key;
                    break;
                }
            }
        }

        // 2. 如果找到了输家，触发结算
        if (loserId != -1)
        {
            Debug.Log($"[Game Over] 玩家 {loserId} 血量归零，游戏结束！");
            ExecuteGameOver(loserId);
        }
    }

    private void ExecuteGameOver(int loserId)
    {
        IsGameOver = true;
        IsRunning = false; // 停止预测循环

        // 停止物理模拟 (可选，防止尸体乱飞)
        Time.timeScale = 0;

        // 判断是我赢了还是输了
        bool isWin = (loserId != MyClientId);

        // 通知 UI 层展示结算面板
        // 你可以使用事件系统，或者直接调用 UIManager
        // 这里假设发一个事件
        GameBase.MainInstance.GameOver(isWin);
    }
    #endregion


    #region 换边
    private void CheckRoundLogicInFrame()
    {
        // 如果已经定好了换边时间，就只检查时间到了没
        if (_targetSwapFrame != -1)
        {
            // 计算还要等多少秒 (用于 UI 显示)
            int framesLeft = _targetSwapFrame - ServerFrame;
            float secondsLeft = framesLeft * LogicFrameInterval; // 0.066f
            // 可选：每帧通知 UI 更新倒计时
            // UIManager.UpdateCountDown(secondsLeft);
            if (ServerFrame >= _targetSwapFrame)
            {
                ExecuteSwap(); // 执行换边
                _targetSwapFrame = -1; // 重置 
            }
            return;
        }

        // --- 还没定换边时间，检查是否满足条件 ---

        int currentHunterId = RoomNetSystem.MainInstance.CurrentHunterId;
        int hunterAmmo = -1;

        // 获取权威的子弹数量
        if (MyClientId == currentHunterId)
        {
            // 我是 Hunter，查我的权威状态
            hunterAmmo = _lastAuthoritativeState.Ammo;
        }
        else
        {
            // 别人是 Hunter，查 Remote 的状态
            if (remotePlayers.TryGetValue(currentHunterId, out var remoteCtrl))
            {
                hunterAmmo = remoteCtrl.GetAuthoritativeAmmo(); // 需要在 RemoteControl 加个 Getter
            }
        }

        // 触发条件：子弹归零
        if (hunterAmmo == 0)
        {
            Debug.Log($"[Frame {ServerFrame}] 检测到 Hunter 子弹耗尽，计划在第 {ServerFrame + SwapDelayFrames} 帧换边");
            _targetSwapFrame = ServerFrame + SwapDelayFrames;
            (UIManager.MainInstance.OpenPanel(UIConst.WaitingPanel) as WaitingPanel).Setup(3f, "Change Round......");
        }
    }
    private void ExecuteSwap()
    {
        Debug.Log($"[Frame {ServerFrame}] >>> 执行换边逻辑 <<<");

        // 1. 收集权威 HP (必须是权威状态，不能是预测状态)
        Dictionary<int, int> hpMap = new Dictionary<int, int>();

        // 存自己的权威 HP
        hpMap.Add(MyClientId, _lastAuthoritativeState.HP);

        // 存其他人的权威 HP
        foreach (var kvp in remotePlayers)
        {
            hpMap.Add(kvp.Key, kvp.Value.GetAuthoritativeHP()); // 需要 Getter
        }

        // 2. 清空预测队列 & 重置预测帧
        // 换边后，预测历史失效，必须重新对齐
        _predictionStates.Clear();
        _clientPredictFrame = 0; // 下次 PredictNextFrame 会自动重新对齐

        // 3. 调用 BattleInitSystem 重建角色
        GameBase.MainInstance.battleInitSystem.SwitchRound(hpMap);

        // 4. 强制刷新墙壁 (确保视觉正确)
        GameBase.MainInstance.wallSystem.RefreshVisuals(ServerFrame);
    }
    #endregion

    #region 回滚：通过客户端输入回滚
    /// <summary>
    /// 本地回滚校正逻辑
    /// </summary>
    private void ProcessLocalPlayerCorrection(AllInputData serverData)
    {
        // 获取服务端记录的“我上一帧的输入”
        // 注意：如果丢包，服务端可能补发了空操作，必须基于那个空操作来校正
        InputData myServerInput = null;
        if (!serverData.AllClientInput.TryGetValue(MyClientId, out myServerInput))
        {
            // 如果服务端没发我的数据（极少见），构造一个空输入防止报错
            myServerInput = new InputData
            {
                Id = MyClientId,
                GlobalFrame = ServerFrame,
                AxisX = 0,
                AxisY = 0,
                Action = 0,
                Speed = 0
            };
        }

        //  重新执行这一帧的逻辑 (模拟服务端发生的事情)
        PlayerState newAuthState = m_LocalPlayer.PredictState(
                   _lastAuthoritativeState,
                   myServerInput,
                   LogicFrameInterval,
                   true // <--- isCorrection = true
               );
        // =========================================================
        // 【核心修复】伤害已经结算进 newAuthState 了，必须立刻清空！
        // 否则下一帧 ProcessLocalPlayerCorrection 时会再扣一次
        // =========================================================
        m_LocalPlayer.PendingDamage = 0;
        // =========================================================
        // C. 现在 WallSystem 里的状态就是“这一帧最新的权威状态”
        WallSnapshot newAuthWallState = GameBase.MainInstance.wallSystem.GetSnapshot();
        // 更新权威基准
        _lastAuthoritativeState = newAuthState;
        _lastAuthoritativeWallState = newAuthWallState; // 更新墙壁基准



        // 2. 从预测队列中清理掉“已经确认过”的旧帧
        while (_predictionStates.Count > 0 && _predictionStates.Peek().FrameNumber < ServerFrame)
        {
            _predictionStates.Dequeue();
        }

        // 3. 校验：如果队列里还有这一帧的记录，拿出来对比
        if (_predictionStates.Count > 0 && _predictionStates.Peek().FrameNumber == ServerFrame)
        {
            var history = _predictionStates.Dequeue();

            // 比较 历史预测值 vs 现在的权威计算值
            if (!PlayerState.IsClose(history.State, newAuthState))
            {
                Debug.LogWarning($"[预测回滚] 帧 {ServerFrame} 出现偏差。预测:{history.State} 权威:{newAuthState}");

                // 1. 恢复玩家位置
                m_LocalPlayer.TeleportTo(newAuthState);

                // 2. 【新增】恢复墙壁到权威状态
                // 此时 newAuthWallState 是碎的。
                // 这句话执行后，不仅权威数据是对的，游戏世界的墙也被强制设为碎的。
                // 接下来客户端预测 Frame 121, 122 时，都会基于这个碎墙进行预测。
                GameBase.MainInstance.wallSystem.RestoreSnapshot(newAuthWallState);

                // 3. 清空预测队列，重置预测帧
                _predictionStates.Clear();
                _clientPredictFrame = 0;
                // 下一帧 Update 会从 ServerFrame 开始，基于当前的 Auth 墙壁状态重新预测未来
            }
        }
    }

    /// <summary>
    /// 远程玩家处理逻辑
    /// </summary>
    private void ProcessRemotePlayers(AllInputData serverData)
    {
        foreach (var kvp in serverData.AllClientInput)
        {
            int pid = kvp.Key;

            // 跳过自己
            if (pid == MyClientId) continue;

            // 找到对应的远程控制器
            if (remotePlayers.TryGetValue(pid, out RemotePlayerControl remoteCtrl))
            {
                // 将输入交给远程控制器，它内部会更新权威位置并设置插值目标
                remoteCtrl.UpdateFromServerInput(kvp.Value);
            }
        }
    }
    #endregion

}
