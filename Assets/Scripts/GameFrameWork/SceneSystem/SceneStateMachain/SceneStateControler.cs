using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneStateControler
{
    private ISceneState m_State = null;
    private bool m_Running = false;
    public bool IsRunning
    {
        get { return m_Running; }
    }

    public SceneStateControler() { }

    /// <summary>
    /// 设置状态机状态(不需要进行Loading的场景切换)
    /// </summary>
    /// <param name="state">需要更改的状态</param>
    /// <param name="stateName">需要更改状态名</param>
    public void SetState(ISceneState state)
    {
        ISceneState lastScene = null;
        //不同场景的大小是不同的，如果需要加载到比较大的场景就会使用到LoadingScene。
        //对sceneName进行判断，需要Loading
        m_Running = false;

        if(m_State != null)
        {
            m_State.StateEnd();
            lastScene = m_State;
        }
        //切换状态
        m_State = state;
        //状态开始
        state.StateBegain();
        m_Running = true;

        if (lastScene != null)
        {
            //当上一个场景为加载场景就不需要卸载场景
            if (lastScene.SceneName == "") return;
            SceneManager.UnloadSceneAsync(lastScene.SceneName);
            Debug.Log("卸载场景" + lastScene.SceneName);
            Debug.Log("当前激活场景" + SceneManager.GetActiveScene());
        }
    }

    public void StateUpdate()
    {
        if (m_Running)//状态机持续运行
        {
            //Debug.Log("22222");
            m_State.StateUpdate();
        }
    }
}
