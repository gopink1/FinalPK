using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateHomePanel : PanelBase
{
    private Transform CreateHomeBtn;
    private Transform BackHomeBtn;
    private TMP_InputField inputField;
    private void Awake()
    {
        CreateHomeBtn = transform.Find("Panel/Create");
        CreateHomeBtn.GetComponent<Button>().onClick.AddListener(OnCreateHomeClick);
        BackHomeBtn = transform.Find("Panel/Back");
        BackHomeBtn.GetComponent<Button>().onClick.AddListener(OnBackClick);
        inputField = transform.Find("Panel/SetName/InputField").GetComponent<TMP_InputField>();
    }

    private void OnBackClick()
    {
        UIManager.MainInstance.OpenPanel(UIConst.EnterMenuPanel);
        UIManager.MainInstance.ClosePanel(UIConst.CreateHomePanel);
    }

    private void OnCreateHomeClick()
    {
        //¼ì²âÂß¼­
        //·¢ËÍJoinRommMsg
        if (NetAsyncMgr.Instance.IsConnected)
        {
            RoomNetSystem.MainInstance.RequestCreateRoom(inputField.text);
            (UIManager.MainInstance.OpenPanel(UIConst.FloatingTextPanel) as FloatingTextPanel).FloatingText("Complete!");
        }
        else
        {

        }
        UIManager.MainInstance.OpenPanel(UIConst.HomePanel);
        (UIManager.MainInstance.GetPanel(UIConst.HomePanel) as HomePanel).SetPlayerName(true,inputField.text);
        UIManager.MainInstance.ClosePanel(UIConst.CreateHomePanel);
    }
}
