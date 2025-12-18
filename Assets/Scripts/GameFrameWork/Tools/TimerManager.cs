using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using MyTools;
using System;

public class TimerManager : MyTools.Singleton<TimerManager>
{
    //管理Timer的类
    //初始空的Timer集合
    [SerializeField] private int initMaxTimerCount;

    private Queue<GameTimer> noWorkingTimers = new Queue<GameTimer>();
    private List<GameTimer> isWorkingTimers = new List<GameTimer>();

    private void Start()
    {
        InitGamerTimerManager();
        //初始化
    }

    //初始化游戏时间管理器
    private void InitGamerTimerManager()
    {
        //初始化一定数量的空计时器
        for (int i = 0; i < initMaxTimerCount; i++)
        {
            GameTimer timer = new GameTimer();
            noWorkingTimers.Enqueue(timer);
        }
    }

    private void Update()
    {
        UpdateGameTimers();
    }

    public void TryEnableOneGameTimer(float time, Action action)
    {
        if (noWorkingTimers.Count == 0)
        {
            var newtimer = new GameTimer();
            noWorkingTimers.Enqueue(newtimer);
            var timer = noWorkingTimers.Dequeue();
            timer.StartTimer(time, action);
            isWorkingTimers.Add(timer);
        }
        else
        {
            var timer = noWorkingTimers.Dequeue();
            timer.StartTimer(time, action);
            isWorkingTimers.Add(timer);
        }

    }
    private void UpdateGameTimers()
    {
        if (isWorkingTimers.Count == 0) return;
        for (int i = 0; i < isWorkingTimers.Count; i++)
        {
            if (isWorkingTimers[i].GetTimerState == TimerState.Working)
            {
                isWorkingTimers[i].UpdateTimer();
            }
            else
            {
                //加队列 反初始化 删除
                noWorkingTimers.Enqueue(isWorkingTimers[i]);
                isWorkingTimers[i].ResetTimer();
                isWorkingTimers.Remove(isWorkingTimers[i]);
            }

        }
    }
}
