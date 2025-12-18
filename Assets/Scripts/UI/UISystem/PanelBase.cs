using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelBase : MonoBehaviour 
{
    //所有的界面的基础，是所有界面预制件的父类脚本
    //统一他们的行为

    protected bool isClose;//界面是否关闭

    protected string panelName;//界面名称
    public virtual void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }

    public virtual void OpenPanel(string name)
    {

        panelName = name;
        isClose = false;
        SetActive(true);
    }
    public virtual void ClosePanel()
    {
        if (isClose) { return; }
        isClose = true;
        gameObject.SetActive(false);
        Destroy(gameObject);
        if (UIManager.MainInstance.panelDict.ContainsKey(panelName))
        {
            //如果包含则删除
            UIManager.MainInstance.panelDict.Remove(panelName);
        }
    }
}
