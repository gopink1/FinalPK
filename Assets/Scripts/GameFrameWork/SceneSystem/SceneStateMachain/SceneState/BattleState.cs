using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleState : ISceneState
{


    public BattleState(SceneStateControler controler) : base(controler)
    {
        this.StateName = "BattleState";
        this.SceneName = "BattleScene";
    }
    public override void StateBegain()
    {
        //GameObject.FindWithTag("MainCamera").gameObject.SetActive(false);
        Debug.Log(SceneManager.GetSceneByName(SceneName).name);
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(SceneName));
        Debug.Log("开启战斗的逻辑");
        //开启
        GameBase.MainInstance.Init();


        // 3. 【核心修改】检查是否有缓存的第一帧数据
        if (RoomNetSystem.MainInstance.PendingStartFrame != null)
        {
            Debug.Log("[BattleState] 检测到缓存的首帧数据，正在注入...");

            // 手动把第一帧塞给帧系统
            GameBase.MainInstance.clientFrameSystem.HandleServerFrame(RoomNetSystem.MainInstance.PendingStartFrame);

            // 清空缓存，避免重复处理
            RoomNetSystem.MainInstance.ClearPendingFrame();
        }

    }
    public override void StateUpdate()
    {
        GameBase.MainInstance.UpdateSystem();
    }

    public override void StateEnd()
    {
        GameBase.MainInstance.Release();
    }
}
