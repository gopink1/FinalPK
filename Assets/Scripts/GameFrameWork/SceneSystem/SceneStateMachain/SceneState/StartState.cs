using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartState : ISceneState
{
    public StartState(SceneStateControler controler) : base(controler)
    {
        this.StateName = "StartState";
        this.SceneName = "StartScene";
    }

    public override void StateBegain() 
    {
        SceneManager.LoadScene("StartScene", LoadSceneMode.Additive);
        UIManager.MainInstance.OpenPanel(UIConst.StartPanel);
    }
    public override void StateUpdate()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            m_Controler.SetState(new MainMenuState(this.m_Controler));
        }
    }

    public override void StateEnd()
    {
        UIManager.MainInstance.ClosePanel(UIConst.StartPanel);
    }
}
