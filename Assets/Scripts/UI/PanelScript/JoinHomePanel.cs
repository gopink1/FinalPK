using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JoinHomePanel : PanelBase
{
    private Transform JoinHomeBtn;
    private Transform BackBtn;
    private TMP_InputField nameField;
    private TMP_InputField roomNumField;
    private void Awake()
    {
        JoinHomeBtn = transform.Find("Panel/Join");
        JoinHomeBtn.GetComponent<Button>().onClick.AddListener(OnJoinHomeBtnClick);
        BackBtn = transform.Find("Panel/Back");
        BackBtn.GetComponent<Button>().onClick.AddListener(OnBackBtnClick);
        nameField = transform.Find("Panel/SetName/InputField").GetComponent<TMP_InputField>();
        roomNumField = transform.Find("Panel/HomeNum/InputField").GetComponent<TMP_InputField>();
    }

    private void OnBackBtnClick()
    {
        UIManager.MainInstance.OpenPanel(UIConst.EnterMenuPanel);
        UIManager.MainInstance.ClosePanel(UIConst.CreateHomePanel);
    }

    private void OnJoinHomeBtnClick()
    {
        //¼ì²âÂß¼­
        RoomNetSystem.MainInstance.RequestJoinRoom(int.Parse(roomNumField.text), nameField.text);
        //µÈ´ý·µ»ØÂß¼­

        //
        //
        HomePanel homePanel =  UIManager.MainInstance.OpenPanel(UIConst.HomePanel) as HomePanel;
        homePanel.SetPlayerName(false,nameField.text);
        homePanel.SetHomeNum(roomNumField.text);
        UIManager.MainInstance.ClosePanel(UIConst.JoinHomePanel);
    }
}
