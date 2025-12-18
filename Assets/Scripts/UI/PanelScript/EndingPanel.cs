using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndingPanel : PanelBase
{
    public TextMeshProUGUI Msg;
    private Transform backToMenu;
    private void Awake()
    {
        Msg = transform.Find("Msg").GetComponent<TextMeshProUGUI>();
        backToMenu = transform.Find("BackToMenu");
        backToMenu.GetComponent<Button>().onClick.AddListener(OnBackClick);
    }

    private void OnBackClick()
    {
        //点击返回主菜单
        //断开连接
        //返回主菜单
        NetAsyncMgr.Instance.Close();
        RoomNetSystem.MainInstance.Realse();
        GameBase.MainInstance.ReStart();
    }

    public void SetEndingMsg(bool isWin)
    {
        if (isWin)
        {
            Msg.text = "You Are Win !";
        }
        else
        {
            Msg.text = "You Are Die !";
        }
    }
}
