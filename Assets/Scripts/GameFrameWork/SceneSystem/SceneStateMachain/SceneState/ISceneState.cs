using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 场景状态父类统一状态行为
/// </summary>
public class ISceneState
{
    /// <summary>
    /// 状态名
    /// </summary>
    protected string m_StateName = "ISceneState";

    /// <summary>
    /// 场景名
    /// </summary>
    protected string m_SceneName = "";
    public string StateName
    {
        get { return m_StateName; }
        set { m_StateName = value; }
    }
    public string SceneName
    {
        get
        {
            return m_SceneName;
        }
        set 
        {
            m_SceneName = value; 
        }
    }
    /// <summary>
    /// 状态拥有者
    /// </summary>
    protected SceneStateControler m_Controler;
    /// <summary>
    /// 构造函数初始化状态拥有者
    /// </summary>
    /// <param name="controler">状态拥有者</param>
    public ISceneState(SceneStateControler controler)
    {
        m_Controler = controler;
    }

    /// <summary>
    /// 状态开始
    /// </summary>
    public virtual void StateBegain() { }
    /// <summary>
    /// 状态更新
    /// </summary>
    public virtual void StateUpdate() { }
    /// <summary>
    /// 状态结束
    /// </summary>
    public virtual void StateEnd() { }

    protected virtual void LoadCurScene()
    {
        //根据当前的场景名字
        //开启加载场景的协程
    }
    
}
