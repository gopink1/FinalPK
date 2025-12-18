using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class UIConst
{
    //UI常量用于对于,面板名字的记录
    public const string EnterMenuPanel = "EnterMenuPanel";
    public const string HomePanel = "HomePanel";
    public const string MainMenuPanel = "MainMenuPanel";
    public const string CreateHomePanel = "CreateHomePanel";
    public const string JoinHomePanel = "JoinHomePanel";
    public const string LoadingPanel = "LoadingPanel";
    public const string StartPanel = "StartPanel";

    public const string PlayingPanel = "PlayingPanel";
    public const string RunnerPanel = "RunnerPanel";
    public const string HunterPanel = "HunterPanel";
    public const string WaitingPanel = "WaitingPanel"; // 新增
    public const string EndingPanel = "EndingPanel"; // 新增
    public const string FloatingTextPanel = "FloatingTextPanel"; // 新增

}
public class UIManager
{
    private static UIManager instance;

    private static readonly object instanceLock = new object();

    public static UIManager MainInstance
    {
        get
        {
            if (instance == null)
            {
                lock (instanceLock)
                {
                    if (instance == null)
                    {
                        instance = new UIManager();
                        return instance;
                    }
                }
            }
            return instance;
        }
    }


    //路径配置
    private Dictionary<string, string> pathDict;
    //ui从层级
    UILayer layer;
    private UIManager()
    {
        InitDict();
    }
    private void InitDict()
    {
        //初始化路径
        pathDict = new Dictionary<string, string>() {
            { UIConst.EnterMenuPanel,"UI/UIPanel/EnterMenuPanel"},
            { UIConst.HomePanel,"UI/UIPanel/HomePanel"},
            { UIConst.MainMenuPanel,"UI/UIPanel/MainMenuPanel"},
            { UIConst.CreateHomePanel,"UI/UIPanel/CreateHomePanel"},
            { UIConst.JoinHomePanel,"UI/UIPanel/JoinHomePanel"},
            { UIConst.LoadingPanel,"UI/UIPanel/LoadingPanel"},
            { UIConst.StartPanel,"UI/UIPanel/StartPanel"},
            { UIConst.PlayingPanel,"UI/UIPanel/PlayingPanel"},
            { UIConst.RunnerPanel,"UI/UIPanel/RunnerPanel"},
            { UIConst.HunterPanel,"UI/UIPanel/HunterPanel"},
            { UIConst.WaitingPanel, "UI/UIPanel/WaitingPanel"},
            { UIConst.EndingPanel, "UI/UIPanel/EndingPanel"},
            { UIConst.FloatingTextPanel, "UI/UIPanel/FloatingTextPanel"}
    };
        prefabDict = new Dictionary<string, GameObject>();
        panelDict = new Dictionary<string, PanelBase>();
        layer = new UILayer();
    }


    //获取每个panel需要依附的画布
    //复杂需要多个但简单的只需要一个

    //根据UITYPE有多个Root
    private Transform canvas;

    public Transform UIRoot
    {
        get
        {
            if (canvas == null)
            {
                if (GameObject.Find("Canvas"))
                {
                    canvas = GameObject.Find("Canvas").transform;
                }
                else
                {
                    //新建一个canvas
                    canvas = new GameObject("Canvas").transform;

                }
            }
            return canvas;
        }
    }
    //打开和关闭panel的方法
    //两个字典
    //一个存储所有的预制体
    //一个存储现在正在打开的UI界面
    private Dictionary<string, GameObject> prefabDict;//所有的预制体

    public Dictionary<string, PanelBase> panelDict;//已开启

    public PanelBase OpenPanel(string name)
    {
        //打开面板
        PanelBase panel = null;

        //打开前需要对面板的是否开启信息进行确认如果已经开启那么不需要
        if (panelDict.TryGetValue(name, out panel))
        {
            //已经开启怎直接跳出函数
            Debug.Log(name + "需要打开的面板已经开启");
            return panel;
        }

        //再检查预制体字典前需要对路径进行判断，查看路径是否配置
        string path = "";
        if (!pathDict.TryGetValue(name, out path))
        {
            //没有配置对应panel的路径
            Debug.Log(name + "路径没有配置");
        }
        //先检查当前的预制体字典中是否含有该名称的panel
        if (!prefabDict.TryGetValue(name, out GameObject prefab))
        {
            //如果预制件没有则通过Resources进行缓存
            prefab = Resources.Load(path) as GameObject;//通过路径获取预制件
            prefabDict.Add(name, prefab);
        }
        //经过上面的检索已经为name的panel的面板加入到prefabdict和赋予PanelBase
        //只需要对其进行取出然后启用
        GameObject panelObj = GameObject.Instantiate<GameObject>(prefab, UIRoot);
        panel = panelObj.GetComponent<PanelBase>();
        panelDict.Add(name, panel);
        //Debug.Log("添加了"+ name);
        panel.OpenPanel(name);
        //设置开启页面层级
        layer.SetLayer(panelObj);

        return panel;

    }
    public bool ClosePanel(string name)
    {
        PanelBase panel = null;
        if (!panelDict.TryGetValue(name, out panel))
        {
            //页面未开启直接返回
            Debug.Log("关闭页面" +  name + "失败");
            return false; 
        }
        //走到这说明页面已经开启只需要把页面关闭
        //并且panel的值已经与自带中相对
        panel.ClosePanel();
        return true;
    }
    public bool CloseAllPanel()
    {
        if (panelDict == null || panelDict.Count == 0) return false;

        string[] panelNames = panelDict.Keys.ToArray();
        foreach (string name in panelNames)
        {
            if (panelDict.TryGetValue(name, out PanelBase panel))
            {
                panel.ClosePanel();
            }
        }
        return true;
    }

    public PanelBase GetPanel(string name)
    {
        PanelBase panel1 = null;
        foreach(var key in panelDict.Keys)
        {
            if(name == key)
            {
                panel1 = panelDict[name];
                return panel1;
            }
        }
        Debug.LogWarning("面板名字为"+name+"的面板并未开启");
        return panel1;
    }
}
