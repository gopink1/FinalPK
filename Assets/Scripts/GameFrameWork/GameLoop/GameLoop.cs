using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLoop : MonoBehaviour
{

    SceneStateControler m_SceneStateController = new SceneStateControler(); 

    private void Awake()
    {
        GameObject.DontDestroyOnLoad(this.gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        GameInit();
    }


    // Update is called once per frame
    void Update()
    {
        UpdateGameLogic();
    }
    /// <summary>
    /// 游戏初始化
    /// </summary>
    private void GameInit()
    {
        m_SceneStateController.SetState(new StartState(m_SceneStateController));

        //把状态拥有者的SetState设置为事件可以让其他的对象进行调用
        GameEventManager.MainInstance.AddEventListening<string>(EventHash.ChangeSceneState, ChangeSceneState);
    }
    //游戏更新
    private void UpdateGameLogic()
    {

        m_SceneStateController.StateUpdate();
    }

    private void ChangeSceneState(string stateName)
    {
        //根据传入的场景名字改变场景
        switch (stateName)
        {
            case "StartState":
                m_SceneStateController.SetState(new StartState(m_SceneStateController));
                break;
            case "MainMenuState":
                m_SceneStateController.SetState(new MainMenuState(m_SceneStateController));
                break;
            case "BattleState":
                m_SceneStateController.SetState(new LoadingState(m_SceneStateController,"BattleScene"));
                break;
            default:
                Debug.LogError("错误的状态名字");
                break;
        }
    }

    private void OnDestroy()
    {
        GameEventManager.MainInstance.RemoveEvent<string>(EventHash.ChangeSceneState, ChangeSceneState);
    }
}
