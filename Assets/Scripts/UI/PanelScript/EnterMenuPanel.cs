using UnityEngine;
using UnityEngine.UI;

public class EnterMenuPanel : PanelBase
{
    private Transform CreateHomeBtn;
    private Transform JoinHomeBtn;
    private Transform BackBtn;
    private void Awake()
    {
        CreateHomeBtn = transform.Find("Options/Create");
        CreateHomeBtn.GetComponent<Button>().onClick.AddListener(OnCreateHomeBtnClick);
        JoinHomeBtn = transform.Find("Options/Enter");
        JoinHomeBtn.GetComponent<Button>().onClick.AddListener(OnJoinHomeBtnClick);
        BackBtn = transform.Find("Options/Back");
        BackBtn.GetComponent<Button>().onClick.AddListener(OnBackBtnClick);
    }

    private void OnCreateHomeBtnClick()
    {
        UIManager.MainInstance.OpenPanel(UIConst.CreateHomePanel);
        UIManager.MainInstance.ClosePanel(UIConst.EnterMenuPanel);
    }

    private void OnJoinHomeBtnClick()
    {
        UIManager.MainInstance.OpenPanel(UIConst.JoinHomePanel);
        UIManager.MainInstance.ClosePanel(UIConst.EnterMenuPanel);
    }

    private void OnBackBtnClick()
    {
        UIManager.MainInstance.OpenPanel(UIConst.MainMenuPanel);
        UIManager.MainInstance.ClosePanel(UIConst.EnterMenuPanel);
    }
}
