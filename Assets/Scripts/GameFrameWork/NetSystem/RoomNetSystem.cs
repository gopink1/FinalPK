using MyTools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomNetSystem : SingletonNonMono<RoomNetSystem>
{
    // 【关键】在这里存储你的 ID
    public int MyPlayerId { get; private set; } // Socket ID
    public int MySeatId { get; private set; }   // 【新增】我的座位号 (0或1)

    public int RandomSeed;
    // 存储一个简单的映射，方便战斗中查找
    // Key: SocketID, Value: SeatID
    public Dictionary<int, int> IdToSeatMap = new Dictionary<int, int>();
    public int FirstHunterId;
    public bool isHunter;
    // 【新增】当前回合的 Hunter ID
    public int CurrentHunterId;
    // 标记：服务端是否已经开始推帧了？
    public bool IsFighting { get; private set; } = false;
    // 【新增】用于缓存第一帧的数据
    public AllInputData PendingStartFrame { get; private set; }

    // 发送：告诉服务端“我场景加载好了”
    public void SendBattleReady()
    {
        BattleReadyMsg msg = new BattleReadyMsg();
        // 如果协议里需要ID就填，不需要（服务端根据Socket知道是谁）就不填
        msg.battleReadyData = new BattleReady();
        msg.battleReadyData.PlayerId = 0;
        NetAsyncMgr.Instance.Send(msg);
        Debug.Log("发送场景加载完成通知...");
    }

    //第一帧
    public void OnReceiveFrameInput(AllInputMsg msg)
    {
        // 1. 检查是否是第一帧（或者只要收到帧，就说明开始了）
        if (!IsFighting)
        {
            IsFighting = true;
            Debug.Log($"收到第 {msg.data.GlobalFrame} 帧，服务器已启动，客户端结束Loading状态！");
            PendingStartFrame = msg.data;
            return;
        }

        // 2. 将收到的帧数据，塞给客户端的帧同步管理器
        // 注意：即使是第一帧，也不能丢弃，必须传进去执行
        //ClientFrameManager.Instance.AddServerFrame(msg.data);
        // 2. 正常游戏中的后续帧，直接转发
        // 注意：这里加个判空，防止切换场景间隙报错
        if (GameBase.MainInstance.clientFrameSystem != null && GameBase.MainInstance.clientFrameSystem.IsRunning)
        {
            Debug.Log($"收到第 {msg.data.GlobalFrame} 帧，开启逻辑帧处理");
            GameBase.MainInstance.clientFrameSystem.HandleServerFrame(msg.data);
        }

    }

    public void ResetLocalState()
    {
        IsFighting = false;
        // ... 其他重置 ...
    }

    // UI点击“创建房间”
    public void RequestCreateRoom(string name)
    {
        JoinRoomMsg msg = new JoinRoomMsg();
        JoinRoom joinRoom = new JoinRoom();
        joinRoom.RoomId = 0; // 0 代表创建
        joinRoom.PlayerName = name;
        msg.joinRoomData = joinRoom;
        NetAsyncMgr.Instance.Send(msg);
    }

    // UI点击“加入房间”
    public void RequestJoinRoom(int roomId, string name)
    {
        JoinRoomMsg msg = new JoinRoomMsg();
        JoinRoom joinRoom = new JoinRoom();
        joinRoom.RoomId = roomId; // 0 代表创建
        joinRoom.PlayerName = name;
        msg.joinRoomData = joinRoom;
        NetAsyncMgr.Instance.Send(msg);
    }

    // UI点击“准备”
    public void RequestReady(bool isReady)
    {
        PlayerReadyMsg msg = new PlayerReadyMsg();
        msg.playerReadyData = new PlayerReady();
        msg.playerReadyData.IsReady = isReady;
        NetAsyncMgr.Instance.Send(msg);
    }

    // --- 接收响应 ---
    public void OnRoomStateReceived(BaseMsg baseMsg)
    {
        RoomStateMsg msg = baseMsg as RoomStateMsg;
        if (msg.roomStateData.Result != 0)
        {
            Debug.LogError("加入/创建房间失败");
            return;
        }
        MyPlayerId = msg.roomStateData.SelfId;

        // 更新映射表
        IdToSeatMap.Clear();
        foreach (var p in msg.roomStateData.Players)
        {
            IdToSeatMap[p.PlayerId] = p.SeatId;

            if (p.PlayerId == MyPlayerId)
            {
                MySeatId = p.SeatId; // 记录我自己的座位
            }
        }
        // 成功！更新数据
        MyPlayerId = msg.roomStateData.SelfId;
        int roomId = msg.roomStateData.RoomId;

        //判断是否成功
        if(msg.roomStateData.Result == 0)
        {
            //成功则刷新ui
            (UIManager.MainInstance.OpenPanel(UIConst.FloatingTextPanel) as FloatingTextPanel).FloatingText("Complete!");
            (UIManager.MainInstance.GetPanel(UIConst.HomePanel) as HomePanel).SetHomeNum(roomId);
        }
        else
        {
            //失败则显示失败
            (UIManager.MainInstance.OpenPanel(UIConst.FloatingTextPanel) as FloatingTextPanel).FloatingText("Fail !!!");
        }


        Debug.Log($"进入房间成功，房间号：{roomId}");

        // 判断selfid，如果为0说明为房主1说明为加入游戏的客户都安
        // 这里调用 UI 管理器去刷新界面
        foreach (var player in msg.roomStateData.Players)
        {
            if (player.PlayerId == MyPlayerId)
            {
                // A. 如果这个 ID 等于 SelfId，说明是【我自己】
                // 刷新【左侧/己方】面板
                (UIManager.MainInstance.GetPanel(UIConst.HomePanel) as HomePanel).SetPlayerName(true, player.PlayerName);
                (UIManager.MainInstance.GetPanel(UIConst.HomePanel) as HomePanel).SetPlayer1Ready(player.IsReady);
            }
            else
            {
                // B. 如果这个 ID 不等于 SelfId，说明是【对手】
                // 刷新【右侧/敌方】面板
                (UIManager.MainInstance.GetPanel(UIConst.HomePanel) as HomePanel).SetPlayerName(false, player.PlayerName);
                (UIManager.MainInstance.GetPanel(UIConst.HomePanel) as HomePanel).SetPlayer2Ready(player.IsReady);
            }
        }
    }
    // ---开接收到服务端开始游戏指令---
    public void OnGameStartReceived(BaseMsg baseMsg)
    {
        GameStartMsg msg = baseMsg as GameStartMsg;
        Debug.Log("游戏开始！随机种子：" + msg.gameStartData.RandomSeed);
        RandomSeed = msg.gameStartData.RandomSeed;
        FirstHunterId = msg.gameStartData.FirstHunterId;
        // 初始化当前 Hunter
        CurrentHunterId = FirstHunterId;
        // 更新 isHunter 标记 (这个标记决定了 UI 显示)
        UpdateRoleFlag();
        // 切换到战斗场景
        GameEventManager.MainInstance.CallEvent(EventHash.ChangeSceneState, "BattleState");

    }
    // 【新增】切换身份的辅助方法
    public void SwapRoleLogic()
    {
        // 简单逻辑：如果是双人对战，把 CurrentHunterId 换成对方的 ID
        foreach (var id in IdToSeatMap.Keys)
        {
            if (id != CurrentHunterId)
            {
                CurrentHunterId = id;
                break;
            }
        }
        UpdateRoleFlag();
    }

    private void UpdateRoleFlag()
    {
        isHunter = (MyPlayerId == CurrentHunterId);
        Debug.Log($"[RoomSystem] 身份切换完成。当前 HunterID: {CurrentHunterId}, 我是 Hunter 吗? {isHunter}");
    }
    public void ClearPendingFrame() { PendingStartFrame = null; }
    public void Realse()
    {
        // 1. 重置战斗状态
        IsFighting = false;

        // 2. 清理第一帧缓存
        PendingStartFrame = null;

        // 3. 清空玩家映射表
        if (IdToSeatMap != null)
        {
            IdToSeatMap.Clear();
        }

        // 4. 重置关键 Gameplay ID 数据
        // 注意：MyPlayerId 通常建议保留，除非你断开了 Socket 连接需要重连
        // 如果断线重连逻辑在 NetAsyncMgr 处理，这里可以不重置 MyPlayerId
        // 但 MySeatId、HunterID 等必须重置
        MySeatId = 0;
        CurrentHunterId = 0;
        FirstHunterId = 0;
        RandomSeed = 0;

        // 5. 重置身份标记
        isHunter = false;

        Debug.Log("[RoomNetSystem] 数据已清理，系统 Release 完成");
    }
}
