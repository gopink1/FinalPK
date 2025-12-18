using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HomePanel : PanelBase
{
    private Transform HomeNum;
    private TextMeshProUGUI HomeNumText;

    private Button BackBtn;

    private Transform P1ReadyBtn;
    private Transform P2ReadyBtn;

    private Transform P1ReadyImg;
    private Transform P1ReadyImgYes;
    private Transform P1ReadyImgNo;
    private Transform P2ReadyImg;
    private Transform P2ReadyImgYes;
    private Transform P2ReadyImgNo;

    private Button start;

    private TextMeshProUGUI p1Name;
    private TextMeshProUGUI p2Name;

    private bool p1IsReady = false;
    private bool p2IsReady = false;

    private void Awake()
    {
        HomeNum = transform.Find("HomeNum");
        HomeNumText = transform.Find("HomeNum/Num").GetComponent<TextMeshProUGUI>();
        BackBtn = transform.Find("HomeNum/Back").GetComponent<Button>();
        BackBtn.onClick.AddListener(OnBackClick);
        P1ReadyBtn = transform.Find("You/Button");
        P1ReadyBtn.GetComponent<Button>().onClick.AddListener(OnP1ReadyBtnClick);
        P2ReadyBtn = transform.Find("Other/Button");
        P2ReadyBtn.GetComponent<Button>().onClick.AddListener(OnP2ReadyBtnClick);
        P1ReadyImg = transform.Find("You/Ready");
        P1ReadyImgYes = transform.Find("You/Ready/Yes");
        P1ReadyImgNo = transform.Find("You/Ready/No");
        P2ReadyImg = transform.Find("Other/Ready");
        P2ReadyImgYes = transform.Find("Other/Ready/Yes");
        P2ReadyImgNo = transform.Find("Other/Ready/No");


        p1Name = transform.Find("You/Image/Text").GetComponent<TextMeshProUGUI>();
        p2Name = transform.Find("Other/Image/Text").GetComponent<TextMeshProUGUI>();
    }

    private void OnBackClick()
    {
        //返回MainMenu菜单栏
        //发送离开房间的消息
        //或者直接断开连接
        NetAsyncMgr.Instance.Close();
        UIManager.MainInstance.OpenPanel(UIConst.MainMenuPanel);
        UIManager.MainInstance.ClosePanel(UIConst.HomePanel);
    }

    public void SetHomeNum(int num)
    {
        HomeNumText.text = num.ToString();
    }
    public void SetHomeNum(string num)
    {
        HomeNumText.text = num;
    }
    public void SetPlayerName(bool isP1,string name)
    {
        if (isP1)
        {
            p1Name.text = name;
        }
        else
        {
            p2Name.text = name;
        }
    }
    public void SetPlayer1Ready(bool isReady)
    {

        P1ReadyImgYes.gameObject.SetActive(isReady);
        P1ReadyImgNo.gameObject.SetActive(!isReady);

    }
    public void SetPlayer2Ready(bool isReady)
    {
        P2ReadyImgYes.gameObject.SetActive(isReady);
        P2ReadyImgNo.gameObject.SetActive(!isReady);
    }

    private void OnP1ReadyBtnClick()
    {
        //与服务端交互逻辑

        //UI交互
        if(!p1IsReady)
        {
            p1IsReady=!p1IsReady;
            P1ReadyImgYes.gameObject.SetActive(true);
            P1ReadyImgNo.gameObject.SetActive(false);
        }
        else
        {
            p1IsReady=!p1IsReady;
            P1ReadyImgYes.gameObject.SetActive(false);
            P1ReadyImgNo.gameObject.SetActive(true);
        }
        //发送准备好了的消息给服务端
        RoomNetSystem.MainInstance.RequestReady(p1IsReady);
    }

    private void OnP2ReadyBtnClick()
    {
        //与服务端交互逻辑

        //UI交互
        if(!p2IsReady)
        {
            p2IsReady=!p2IsReady;
            P2ReadyImgYes.gameObject.SetActive(true);
            P2ReadyImgNo.gameObject.SetActive(false);
        }
        else
        {
            p2IsReady=!p2IsReady;
            P2ReadyImgYes.gameObject.SetActive(false);
            P2ReadyImgNo.gameObject.SetActive(true);
        }

    }
}
