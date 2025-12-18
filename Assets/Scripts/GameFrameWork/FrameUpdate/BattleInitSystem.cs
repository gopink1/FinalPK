using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 处理战斗场景初始化
/// 于游戏第一帧进行处理
/// </summary>
public class BattleInitSystem : IGameSystem
{
    public GameObject HunterPrefab;
    public GameObject RunnerPrefab;

    public Vector3[] BornPos = new Vector3[2];
    public Quaternion[] BornRot = new Quaternion[2];

    public BattleInitSystem(GameBase gameBase) : base(gameBase)
    {
        HunterPrefab =  Resources.Load<GameObject>("Character/Hunter");
        RunnerPrefab = Resources.Load<GameObject>("Character/Runner");
        BornPos[0] = new Vector3(3, 0.2f, 15);
        BornPos[1] = new Vector3(30, 0.2f, 17);
        BornRot[0] = Quaternion.Euler(0, 90, 0);
        BornRot[1] = Quaternion.Euler(0, -90, 0);

    }

    public override void Init()
    {
        InitBattle();
    }
    public override void Update()
    {

    }
    /// <summary>
    /// 初始化战斗场景
    /// 由 BattleState 在 StateBegin 中调用
    /// </summary>
    public void InitBattle()
    {
        // ==============================
        // 1. 数据准备阶段
        // ==============================
        int mySocketId = RoomNetSystem.MainInstance.MyPlayerId;
        int firstHunterSocketId = RoomNetSystem.MainInstance.FirstHunterId;
        int seed = RoomNetSystem.MainInstance.RandomSeed;

        Random.InitState(seed);

        var idToSeatMap = RoomNetSystem.MainInstance.IdToSeatMap;
        if (idToSeatMap == null || idToSeatMap.Count == 0)
        {
            Debug.LogWarning("IdToSeatMap 为空，正在尝试紧急修复...");
            idToSeatMap = new Dictionary<int, int>();
            idToSeatMap[mySocketId] = 0;
            int guessEnemyId = (mySocketId == 1) ? 2 : 1;
            idToSeatMap[guessEnemyId] = 1;
        }

        // ==============================
        // 2. 身份与阵营计算
        // ==============================
        if (!idToSeatMap.TryGetValue(mySocketId, out int mySeatId)) return;

        int enemySocketId = -1;
        int enemySeatId = -1;

        foreach (var kvp in idToSeatMap)
        {
            if (kvp.Key != mySocketId)
            {
                enemySocketId = kvp.Key;
                enemySeatId = kvp.Value;
                break;
            }
        }

        if (!idToSeatMap.TryGetValue(firstHunterSocketId, out int firstHunterSeatId)) firstHunterSeatId = 0;

        bool amIHunter = (mySeatId == firstHunterSeatId);
        RoleType myRole = amIHunter ? RoleType.Hunter : RoleType.Runner;
        RoleType enemyRole = amIHunter ? RoleType.Runner : RoleType.Hunter;

        Debug.Log($"[BattleInit] MyRole:{myRole} | EnemyRole:{enemyRole}");

        // ==============================
        // 3. 【关键修改】先启动引擎 (InitGame)
        // ==============================
        // 先获取出生点数据

        int mySpawnIndex = (myRole == RoleType.Hunter) ? 0 : 1;
        Vector3 mySp = BornPos[mySpawnIndex];
        Quaternion mySr = BornRot[mySpawnIndex];

        if (GameBase.MainInstance.clientFrameSystem != null)
        {
            // 先初始化，这会清理 remotePlayers 字典
            // 并设置初始状态 _lastAuthoritativeState
            GameBase.MainInstance.clientFrameSystem.InitGame(mySocketId, mySp, mySr);
        }

        // ==============================
        // 4. 后生成角色 (CreatePlayer)
        // ==============================

        // 生成我自己 (Local) -> 内部会赋值 m_LocalPlayer
        CreatePlayer(mySocketId, mySeatId, myRole, true,100);

        // 生成敌人 (Remote) -> 内部会调用 RegisterRemotePlayer 填充字典
        if (enemySocketId != -1)
        {
            CreatePlayer(enemySocketId, enemySeatId, enemyRole, false,100);
        }
    }
    
    // 【新增】换边逻辑入口
    // 参数 currentHps: Key是PlayerID, Value是剩余血量
    public void SwitchRound(Dictionary<int, int> currentHps)
    {
        Debug.Log(">>> 开始执行换边逻辑 <<<");

        // 1. 清理场景中旧的角色
        // 我们可以通过 ClientFrameSystem 的引用来销毁
        var frameSys = GameBase.MainInstance.clientFrameSystem;
        if (frameSys.m_LocalPlayer != null)
            GameObject.Destroy(frameSys.m_LocalPlayer.gameObject);

        foreach (var remote in frameSys.GetRemotePlayers().Values) // 需要给 ClientFrameSystem 加个 getter
        {
            if (remote != null) GameObject.Destroy(remote.gameObject);
        }
        // 【建议】强制执行物理刷新，确保旧 Collider 立刻从物理世界移除
        Physics.SyncTransforms();
        // 2. 切换 RoomNetSystem 里的身份标记
        RoomNetSystem.MainInstance.SwapRoleLogic();

        // 3. 切换 UI 面板 (Hunter变Runner，UI也要切)
        if (RoomNetSystem.MainInstance.isHunter)
        {
            UIManager.MainInstance.ClosePanel(UIConst.RunnerPanel);
            UIManager.MainInstance.OpenPanel(UIConst.HunterPanel);
        }
        else
        {
            UIManager.MainInstance.ClosePanel(UIConst.HunterPanel);
            UIManager.MainInstance.OpenPanel(UIConst.RunnerPanel);
        }


        // 4. 重新生成角色 (复用 InitBattle 的部分逻辑，但需要传入 HP)
        ReCreatePlayers(currentHps);
    }

    private void ReCreatePlayers(Dictionary<int, int> inheritedHps)
    {
        // 重新获取身份和座位
        int mySocketId = RoomNetSystem.MainInstance.MyPlayerId;
        int currentHunterId = RoomNetSystem.MainInstance.CurrentHunterId; // 此时已经变了
        var idToSeatMap = RoomNetSystem.MainInstance.IdToSeatMap;

        // 重新计算我是什么角色
        RoleType myRole = (mySocketId == currentHunterId) ? RoleType.Hunter : RoleType.Runner;

        // 查找敌人 ID
        int enemySocketId = -1;
        foreach (var id in idToSeatMap.Keys)
        {
            if (id != mySocketId) enemySocketId = id;
        }

        // 获取要继承的 HP
        int myHp = inheritedHps.ContainsKey(mySocketId) ? inheritedHps[mySocketId] : 100;
        Debug.Log("我需要继承的HP" +  myHp);
        int enemyHp = (enemySocketId != -1 && inheritedHps.ContainsKey(enemySocketId)) ? inheritedHps[enemySocketId] : 100;
        // ===============================================================
        // 【核心修复】调用 InitGame 时，传入 false 不重置帧号
        // ===============================================================
        if (GameBase.MainInstance.clientFrameSystem != null)
        {
            int mySpawnIndex = (myRole == RoleType.Hunter) ? 0 : 1;
            Vector3 mySp = BornPos[mySpawnIndex];
            Quaternion mySr = BornRot[mySpawnIndex];

            // 参数4传入 false：代表这是换边，不是游戏冷启动
            // 这样会保留 ServerFrame 计数，同时保留墙壁的破坏数据
            GameBase.MainInstance.clientFrameSystem.InitGame(mySocketId, mySp, mySr, false,myHp);
        }
        // ===============================================================

        // --- 生成自己 ---
        // 注意：spawnIndex 取决于现在的 Role。
        // 如果我变成了 Runner，mySpawnIndex = 1。
        // 所以 BornPos 实际上是互换了（因为角色变了）。
        CreatePlayer(mySocketId, idToSeatMap[mySocketId], myRole, true, myHp);

        // --- 生成敌人 ---
        if (enemySocketId != -1)
        {
            RoleType enemyRole = (myRole == RoleType.Hunter) ? RoleType.Runner : RoleType.Hunter;
            CreatePlayer(enemySocketId, idToSeatMap[enemySocketId], enemyRole, false, enemyHp);
        }

        // 重要：不要调用 wallSystem.Init()，保留墙壁残骸！
    }
    /// <summary>
    /// 通用角色创建工厂
    /// </summary>
    private void CreatePlayer(int socketId, int seatId, RoleType role, bool isLocal, int hp)
    {
        // 1. 获取对应的预制体
        GameObject prefab = (role == RoleType.Hunter) ? HunterPrefab : RunnerPrefab;

        // 2. 获取出生点 (使用 SeatID)
        int spawnIndex = (role == RoleType.Hunter) ? 0 : 1;
        Vector3 sp = BornPos[spawnIndex];
        Quaternion sr = BornRot[spawnIndex];

        // 3. 实例化
        GameObject playerObj = GameObject.Instantiate(prefab, sp, sr);

        playerObj.name = $"Player_{seatId}_{role}_{(isLocal ? "Local" : "Remote")}";

        // 4. 获取摄像机锚点 (CameraAnchor)
        // 假设你在 Prefab 里做了一个子物体叫 "CameraAnchor"
        Transform anchor = playerObj.transform.Find("CameraAnchor");
        if (anchor == null)
        {
            Debug.LogError($"预制体 {prefab.name} 缺少名为 'CameraAnchor' 的子物体！");
            // 容错：如果没有锚点，就用根节点
            anchor = playerObj.transform;
        }

        // 4. 动态挂载组件 (传入 HP)
        if (isLocal)
        {
            var ctrl = playerObj.AddComponent<LocalPlayerControl>();
            ctrl.MyRole = role;
            // 传入 hp
            ctrl.Initialize(sp, sr, anchor, hp);
            GameBase.MainInstance.cameraSystem.AttachToPlayer(anchor);
            if (GameBase.MainInstance.clientFrameSystem != null)
            {
                GameBase.MainInstance.clientFrameSystem.m_LocalPlayer = ctrl;
            }
        }
        else
        {
            var ctrl = playerObj.AddComponent<RemotePlayerControl>();
            ctrl.MyRole = role;
            // 传入 hp
            ctrl.InitializeAuthoritativeState(sp, sr, hp);

            if (GameBase.MainInstance.clientFrameSystem != null)
            {
                GameBase.MainInstance.clientFrameSystem.RegisterRemotePlayer(socketId, ctrl);
            }
        }
        Physics.SyncTransforms();
    }
    public override void Release()
    {
        
    }


}