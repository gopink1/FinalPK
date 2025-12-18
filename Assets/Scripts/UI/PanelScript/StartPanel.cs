using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartPanel : PanelBase
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            GameEventManager.MainInstance.CallEvent(EventHash.ChangeSceneState, "MainMenuState");
        }
    }
}
