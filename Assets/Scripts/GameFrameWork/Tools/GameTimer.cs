using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
public enum TimerState
{
    NoWorking,
    Working,
    Done
}

public class GameTimer
{
    private float startTime;
    private TimerState nowState;
    private Action task;
    private bool isStopTimer;


    public GameTimer()
    {
        ResetTimer();
    }

    public void StartTimer(float time, Action action)
    {
        startTime = time;
        task = action;
        isStopTimer = false;
        nowState = TimerState.Working;
    }


    public void UpdateTimer()
    {
        if (isStopTimer) return;
        startTime -= Time.deltaTime;
        if (startTime <= 0)
        {
            task?.Invoke();
            isStopTimer=true;
            nowState=TimerState.Done;
        }
    }

    public TimerState GetTimerState => nowState;
    public void ResetTimer()
    {
        startTime = 0;
        isStopTimer = true;
        task = null;
        nowState = TimerState.NoWorking;
    }
}
