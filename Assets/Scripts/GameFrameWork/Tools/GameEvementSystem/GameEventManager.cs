using MyTools;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventHash
{
    //public static readonly string ChangeCharacterVerticalVelocity = "ChangeCharacterVerticalVelocity";
    //设置场景状态
    public static readonly string ChangeSceneState = "ChangeSceneState";
    public static readonly string WaitUI = "WaitUI";
    
}
public class GameEventManager : SingletonNonMono<GameEventManager>
{
    private interface IEventHelp { }//接口同意

    private class EventHelp : IEventHelp
    {
        private event Action action;

        //构造函数
        public EventHelp(Action action)
        {
            this.action = action;
        }

        public void AddCall(Action action)
        {
            this.action += action;
        }

        public void InvokeCall()
        {
            this.action?.Invoke();
        }
        public void RemoveCall(Action action)
        {
            this.action -= action;
        }

    }

    private class EventHelp<T> : IEventHelp
    {
        private event Action<T> action;

        //构造函数
        public EventHelp(Action<T> action)
        {
            this.action = action;
        }

        public void AddCall(Action<T> action)
        {
            this.action += action;
        }

        public void InvokeCall(T value)
        {
            this.action?.Invoke(value);
        }
        public void RemoveCall(Action<T> action)
        {
            this.action -= action;
        }

    }
    private class EventHelp<T1, T2> : IEventHelp
    {
        private event Action<T1, T2> action;

        //构造函数
        public EventHelp(Action<T1, T2> action)
        {
            this.action = action;
        }

        public void AddCall(Action<T1, T2> action)
        {
            this.action += action;
        }

        public void InvokeCall(T1 value1, T2 value2)
        {
            this.action?.Invoke(value1, value2);
        }
        public void RemoveCall(Action<T1, T2> action)
        {
            this.action -= action;
        }

    }
    private class EventHelp<T1, T2, T3> : IEventHelp
    {
        private event System.Action<T1, T2, T3> action;

        //构造函数
        public EventHelp(Action<T1, T2, T3> action)
        {
            this.action = action;
        }

        public void AddCall(Action<T1, T2, T3> action)
        {
            this.action += action;
        }

        public void InvokeCall(T1 value1, T2 value2, T3 Value3)
        {
            this.action?.Invoke(value1, value2, Value3);
        }
        public void RemoveCall(Action<T1, T2, T3> action)
        {
            this.action -= action;
        }

    }
    private class EventHelp<T1, T2, T3, T4> : IEventHelp
    {
        private event System.Action<T1, T2, T3, T4> action;

        //构造函数
        public EventHelp(Action<T1, T2, T3, T4> action)
        {
            this.action = action;
        }

        public void AddCall(Action<T1, T2, T3, T4> action)
        {
            this.action += action;
        }

        public void InvokeCall(T1 value1, T2 value2, T3 Value3, T4 Value4)
        {
            this.action?.Invoke(value1, value2, Value3, Value4);
        }
        public void RemoveCall(Action<T1, T2, T3, T4> action)
        {
            this.action -= action;
        }

    }
    private class EventHelp<T1, T2, T3, T4, T5> : IEventHelp
    {
        private event System.Action<T1, T2, T3, T4, T5> action;

        //构造函数
        public EventHelp(Action<T1, T2, T3, T4, T5> action)
        {
            this.action = action;
        }

        public void AddCall(Action<T1, T2, T3, T4, T5> action)
        {
            this.action += action;
        }

        public void InvokeCall(T1 value1, T2 value2, T3 Value3, T4 Value4, T5 Value5)
        {
            this.action?.Invoke(value1, value2, Value3, Value4, Value5);
        }
        public void RemoveCall(Action<T1, T2, T3, T4, T5> action)
        {
            this.action -= action;
        }

    }

    private Dictionary<string, IEventHelp> dic = new Dictionary<string, IEventHelp>();


    /// <summary>
    /// 添加时间监听
    /// </summary>
    /// <param name="action"></param>
    public void AddEventListening(string eventName, Action action)
    {
        if (dic.TryGetValue(eventName, out IEventHelp e))
        {
            //如果找到事件监听不需要再添加，只需要对其进行取出
            (e as EventHelp)?.AddCall(action);
        }
        else
        {
            dic.Add(eventName, new EventHelp(action));
            //Debug.Log("添加了" + eventName +"的事件");
        }
    }
    public void AddEventListening<T>(string eventName, Action<T> action)
    {
        if (dic.TryGetValue(eventName, out IEventHelp e))
        {
            //如果找到事件监听不需要再添加，只需要对其进行取出
            (e as EventHelp<T>)?.AddCall(action);

        }
        else
        {
            dic.Add(eventName, new EventHelp<T>(action));
            Debug.Log("添加了" + eventName +"的事件");
        }
    }
    public void AddEventListening<T1, T2>(string eventName, Action<T1, T2> action)
    {
        if (dic.TryGetValue(eventName, out IEventHelp e))
        {
            (e as EventHelp<T1, T2>)?.AddCall(action);
        }
        else
        {
            dic.Add(eventName, new EventHelp<T1, T2>(action));
        }
    }
    public void AddEventListening<T1, T2, T3>(string eventName, System.Action<T1, T2, T3> action)
    {
        if (dic.TryGetValue(eventName, out IEventHelp e))
        {
            (e as EventHelp<T1, T2, T3>)?.AddCall(action);
        }
        else
        {
            dic.Add(eventName, new EventHelp<T1, T2, T3>(action));
        }
    }
    public void AddEventListening<T1, T2, T3, T4>(string eventName, System.Action<T1, T2, T3, T4> action)
    {
        if (dic.TryGetValue(eventName, out IEventHelp e))
        {
            (e as EventHelp<T1, T2, T3, T4>)?.AddCall(action);
        }
        else
        {
            dic.Add(eventName, new EventHelp<T1, T2, T3, T4>(action));
        }
    }

    public void AddEventListening<T1, T2, T3, T4, T5>(string eventName, System.Action<T1, T2, T3, T4, T5> action)
    {
        if (dic.TryGetValue(eventName, out IEventHelp e))
        {
            (e as EventHelp<T1, T2, T3, T4, T5>)?.AddCall(action);
        }
        else
        {
            dic.Add(eventName, new EventHelp<T1, T2, T3, T4, T5>(action));
        }
    }


    public void CallEvent(string eventName)
    {
        if (dic.TryGetValue(eventName, out IEventHelp e))
        {
            (e as EventHelp)?.InvokeCall();
        }
        else
        {
            Debug.Log("没有名字为"+ "#"+eventName+ "#"+ "的事件");
        }
    }
    public void CallEvent<T>(string eventName, T value)
    {
        if (dic.TryGetValue(eventName, out IEventHelp e))
        {
            (e as EventHelp<T>)?.InvokeCall(value);
        }
        else
        {
            Debug.Log("没有名字为"+ "#"+eventName+ "#"+ "的事件");
        }
    }
    public void CallEvent<T1, T2>(string eventName, T1 value1, T2 value2)
    {
        if (dic.TryGetValue(eventName, out IEventHelp e))
        {
            (e as EventHelp<T1, T2>)?.InvokeCall(value1, value2);
        }
        else
        {
            Debug.Log("没有名字为"+ "#"+eventName+ "#"+ "的事件");
        }
    }

    public void CallEvent<T1, T2, T3>(string eventName, T1 value1, T2 value2, T3 Value3)
    {
        if (dic.TryGetValue(eventName, out IEventHelp e))
        {
            (e as EventHelp<T1, T2, T3>)?.InvokeCall(value1, value2, Value3);
        }
        else
        {
            Debug.Log("没有名字为"+ "#"+eventName+ "#"+ "的事件");
        }
    }

    public void CallEvent<T1, T2, T3, T4>(string eventName, T1 value1, T2 value2, T3 Value3, T4 Value4)
    {
        if (dic.TryGetValue(eventName, out IEventHelp e))
        {
            (e as EventHelp<T1, T2, T3, T4>)?.InvokeCall(value1, value2, Value3, Value4);
        }
        else
        {
            Debug.Log("没有名字为"+ "#"+eventName+ "#"+ "的事件");
        }
    }
    public void CallEvent<T1, T2, T3, T4, T5>(string eventName, T1 value1, T2 value2, T3 Value3, T4 Value4, T5 Value5)
    {
        if (dic.TryGetValue(eventName, out IEventHelp e))
        {
            (e as EventHelp<T1, T2, T3, T4, T5>)?.InvokeCall(value1, value2, Value3, Value4, Value5);
        }
        else
        {
            Debug.Log("没有名字为"+ "#"+eventName+ "#"+ "的事件");
        }
    }


    public void RemoveEvent(string eventName, Action action)
    {
        if (dic.TryGetValue(eventName, out IEventHelp e))
        {
            (e as EventHelp)?.RemoveCall(action);
        }
        else
        {
            Debug.Log("没有名字为"+ "#"+eventName+ "#"+ "的事件");
        }
    }
    public void RemoveEvent<T>(string eventName, Action<T> action)
    {
        if (dic.TryGetValue(eventName, out IEventHelp e))
        {
            (e as EventHelp<T>)?.RemoveCall(action);
        }
        else
        {
            Debug.Log("没有名字为"+ "#"+eventName+ "#"+ "的事件");
        }
    }
    public void RemoveEvent<T1, T2>(string eventName, Action<T1, T2> action)
    {
        if (dic.TryGetValue(eventName, out IEventHelp e))
        {
            (e as EventHelp<T1, T2>)?.RemoveCall(action);
        }
        else
        {
            Debug.Log("没有名字为"+ "#"+eventName+ "#"+ "的事件");
        }
    }
    public void RemoveEvent<T1, T2, T3>(string eventName, Action<T1, T2, T3> action)
    {
        if (dic.TryGetValue(eventName, out IEventHelp e))
        {
            (e as EventHelp<T1, T2, T3>)?.RemoveCall(action);
        }
        else
        {
            Debug.Log("没有名字为"+ "#"+eventName+ "#"+ "的事件");
        }
    }
    public void RemoveEvent<T1, T2, T3, T4>(string eventName, Action<T1, T2, T3, T4> action)
    {
        if (dic.TryGetValue(eventName, out IEventHelp e))
        {
            (e as EventHelp<T1, T2, T3, T4>)?.RemoveCall(action);
        }
        else
        {
            Debug.Log("没有名字为"+ "#"+eventName+ "#"+ "的事件");
        }
    }
    public void RemoveEvent<T1, T2, T3, T4, T5>(string eventName, System.Action<T1, T2, T3, T4, T5> action)
    {
        if (dic.TryGetValue(eventName, out IEventHelp e))
        {
            (e as EventHelp<T1, T2, T3, T4, T5>)?.RemoveCall(action);
        }
        else
        {
            Debug.Log("没有名字为"+ "#"+eventName+ "#"+ "的事件");
        }
    }

}
