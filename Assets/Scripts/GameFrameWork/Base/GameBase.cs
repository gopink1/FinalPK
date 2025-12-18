using MyTools;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 当游戏开始时候开始调用
/// </summary>
public class GameBase : SingletonNonMono<GameBase>
{
    //游戏系统

    public BattleInitSystem battleInitSystem;

    public ClientFrameSystem clientFrameSystem;

    public CameraSystem cameraSystem;
    public WallSystem wallSystem; // 新增

    /// <summary>
    /// 初始化所有子系统
    /// </summary>
    public void Init()
    {
        cameraSystem = new CameraSystem(this);
        cameraSystem.Init();
        wallSystem = new WallSystem(this);
        wallSystem.Init();
        //初始化生成玩家
        battleInitSystem = new BattleInitSystem(this);
        //初始化帧同步系统
        clientFrameSystem = new ClientFrameSystem(this);
        battleInitSystem.Init();


        UIManager.MainInstance.OpenPanel(UIConst.PlayingPanel);
        if (RoomNetSystem.MainInstance.isHunter)
        {
            UIManager.MainInstance.OpenPanel(UIConst.HunterPanel);
        }
        else
        {
            UIManager.MainInstance.OpenPanel(UIConst.RunnerPanel);
        }
    }

    public void UpdateSystem()
    {
        battleInitSystem.Update();
        clientFrameSystem.Update();
    }

    public void GameOver(bool isWin)
    {
        FloatingTextPanel panel = UIManager.MainInstance.OpenPanel(UIConst.FloatingTextPanel) as FloatingTextPanel;
        panel.FloatingText("游戏结束！！！");
        Time.timeScale = 0.0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        //出现游戏结束的UI
        EndingPanel panel1 = UIManager.MainInstance.OpenPanel(UIConst.EndingPanel) as EndingPanel;
        panel1.SetEndingMsg(isWin);
    }
    public void ReStart()
    {
        //重新开始
        //重新加载Battle场景
        Time.timeScale = 1.0f;
        GameEventManager.MainInstance.CallEvent<string>(EventHash.ChangeSceneState, "StartState");
    }
    public void Exit()
    {

        Application.Quit();
    }
    public void Release()
    {
        wallSystem.Release();
        clientFrameSystem.Release();
        battleInitSystem.Release();
        cameraSystem.Release();
        UIManager.MainInstance.CloseAllPanel();
    }
}
