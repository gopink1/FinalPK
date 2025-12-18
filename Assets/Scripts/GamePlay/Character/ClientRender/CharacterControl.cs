using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
public enum RoleType
{
    Hunter,
    Runner
}
public abstract class CharacterControl : MonoBehaviour
{
    public float speed = 5f;
    [Header("Camera Setup")]
    public Transform CameraAnchor; // 摄像机挂点
    public RoleType MyRole;
    // 策略接口引用
    protected IMovementProcessor _movementProcessor;
    // 本帧待结算的伤害
    // 不需要同步给服务端，这是一个中间计算变量
    [System.NonSerialized]
    public int PendingDamage = 0;
    protected virtual void Awake()
    {
        // 自动获取挂在同一个 Prefab 上的策略脚本
        _movementProcessor = GetComponent<IMovementProcessor>();

        if (_movementProcessor == null)
            Debug.LogError($"{name} 缺少 IMovementProcessor 组件！");
    }
    // 这是一个通用的初始化入口，我们在 BattleInitSystem 里调用过它
    public virtual void InitializeCommon()
    {
        // 1. 获取策略
        _movementProcessor = GetComponent<IMovementProcessor>();
        if (_movementProcessor == null)
            Debug.LogError($"{name} 缺少 IMovementProcessor 组件！");

        // 2. 【关键】如果是 RunnerMovement，主动把我自己传给它
        if (_movementProcessor is RunnerMovement runnerMove)
        {
            runnerMove.SetupControl(this);
        }
    }
    // 每次预测开始前必须重置
    public void ResetFrameLogic()
    {
        PendingDamage = 0;
    }

    // 以前这里是具体的 if-else 计算，现在变成委托调用
    public virtual PlayerState PredictState(PlayerState currentState, InputData input, float deltaTime, bool isCorrection = false)
    {
        if (_movementProcessor == null) return currentState;
        // 将标志位传给具体策略
        return _movementProcessor.CalculateNextState(currentState, input, deltaTime, speed, isCorrection);
    }
    public abstract void UpdateMovement();
}
