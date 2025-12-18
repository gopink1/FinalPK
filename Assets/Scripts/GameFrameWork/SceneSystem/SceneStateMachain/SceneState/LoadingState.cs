using MyTools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingState : ISceneState
{
    private string nextSceneName;

    private LoadingPanel LoadingPanel;
    private float timer = 0;
    private float virtualProgress;
    private bool isLoading = false;
    public LoadingState(SceneStateControler controler,string nextScene) : base(controler)
    {
        this.StateName = "LoadingScene";
        this.SceneName = "";
        nextSceneName = nextScene;
    }

    public override void StateBegain()
    {
        base.StateBegain();
        //打开加载面板
        LoadingPanel =  UIManager.MainInstance.OpenPanel(UIConst.LoadingPanel) as LoadingPanel;
        // 重置战斗状态标记
        RoomNetSystem.MainInstance.ResetLocalState();
        //加载场景的开始
        CoroutineHelper.MainInstance.StartCoroutine(LoadSceneAsync());

    }

    public override void StateUpdate()
    {
        base.StateUpdate();
        //更新UI界面
        if (!isLoading) return;
        //当正在异步处理场景时候
        //对进度进行虚假更新
        VirtualAddProgress(Time.deltaTime, 0.02f, 0.1f, 0.1f);
    }

    public override void StateEnd()
    {
        base.StateEnd();
        UIManager.MainInstance.ClosePanel(UIConst.LoadingPanel);
    }


    /// <summary>
    /// 虚假进度条更新
    /// </summary>
    /// <param name="deltaTime">时间增量Time.Deltatime</param>
    /// <param name="timeLimited">进度更新的时间间隔</param>
    /// <param name="addvirtualProgress">虚假进度条的更新的间隔</param>
    /// <param name="interpolition">进度条大于0.9时候的差值增量</param>
    private void VirtualAddProgress(float deltaTime, float timeLimited, float addvirtualProgress, float interpolition)
    {
        timer += deltaTime;
        if (timer> timeLimited)
        {
            timer -= timeLimited;
            //需要更新进度调用panelbase的refresh方法
            if (virtualProgress < 0.9f)
            {
                virtualProgress += addvirtualProgress;
                LoadingPanel.AddProgress(addvirtualProgress);
                //Debug.Log(virtualProgress);
            }
            else
            {
                virtualProgress = Mathf.Lerp(virtualProgress, 1f, interpolition);
                LoadingPanel.RefreshProgress(virtualProgress);
                // 【修正点】：当接近1时，直接强制设为1，不要设置 isLoading = false
                if (virtualProgress >= 0.99f)
                {
                    virtualProgress = 1.0f; // 强制设为1，满足协程退出条件
                    LoadingPanel.RefreshProgress(1.0f);
                    // LoadingPanel.SetProgressText("Waiting for players.."); 
                    // 这行文字建议放到协程跳出循环后再设置，逻辑更顺畅
                }
            }

        }
    }


    IEnumerator LoadSceneAsync()
    {
        isLoading = true;
        //1.开始异步加载
        AsyncOperation op = SceneManager.LoadSceneAsync(nextSceneName, LoadSceneMode.Additive);
        op.allowSceneActivation = false;//不允许加载完自动启动场景

        // 2. 跑进度条 (强制让玩家看一会加载动画，平滑体验)
        // 即使场景秒加载，进度条也会慢慢走完
        while (op.progress < 0.9f || virtualProgress < 1.0f)
        {
            // 如果真实加载完了(0.9)，但虚假进度还没完，就等虚假进度
            // 如果虚假进度完了，真实加载没完，就等真实加载

            // 为了防止死循环，这里其实主要依赖 StateUpdate 里的 VirtualAddProgress 来增加 virtualProgress
            yield return null;
        }
        //3.进度条跑满了，激活场景，但依旧被ui遮挡
        op.allowSceneActivation = true;
        yield return op;
        // 4. 【关键步骤】握手流程
        // 此时场景已加载完毕，告诉服务器“我好了”
        LoadingPanel.SetProgressText("Waiting for other players...");
        RoomNetSystem.MainInstance.SendBattleReady();

        // 5. 【死循环等待】直到收到服务器的第一帧
        // 这意味着所有人都加载完了，且服务器启动了帧同步
        while (!RoomNetSystem.MainInstance.IsFighting)
        {
            // 可以让 Loading 图标转圈圈，表示正在等待
            yield return null;
        }

        // 6. 收到开始信号，清理现场，进入战斗状态
        isLoading = false;

        // 切换到 BattleState，BattleState 的 Init 会初始化战斗逻辑
        m_Controler.SetState(new BattleState(m_Controler));
    }
}
