using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuPanel : PanelBase
{
    private Transform StartBtn;
    private Transform OptionBtn;
    private Transform QuitBtn;
    private void Awake()
    {
        StartBtn = transform.Find("Options/Start");
        StartBtn.GetComponent<Button>().onClick.AddListener(OnStartBtnClick);
        OptionBtn = transform.Find("Options/Option");
        OptionBtn.GetComponent<Button>().onClick.AddListener(OnOptionBtnClick);
        QuitBtn = transform.Find("Options/Quit");
        QuitBtn.GetComponent<Button>().onClick.AddListener(OnQuitBtnClick);
    }

    private void OnStartBtnClick()
    {
        //NetAsyncMgr.Instance.Connect("59.110.9.106", 8080);
        NetAsyncMgr.Instance.Connect("127.0.0.1", 8080);
        //加载一个动态ui一直连接，当连接成功执行下一步


        UIManager.MainInstance.OpenPanel(UIConst.EnterMenuPanel);
        UIManager.MainInstance.ClosePanel(UIConst.MainMenuPanel);
    }

    private void OnOptionBtnClick()
    {
       
    }

    private void OnQuitBtnClick()
    {
        Application.Quit();
    }
}
