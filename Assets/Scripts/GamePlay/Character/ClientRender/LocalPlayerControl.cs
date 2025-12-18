using UnityEngine;

public class LocalPlayerControl : CharacterControl
{
    private IInputStrategy _strategy;

    // --- 双缓冲状态 (包含位置和旋转) ---
    private PlayerState _fromState;
    private PlayerState _toState;

    // 当前客户端预测的游标
    private PlayerState _clientPredictedState;
    public PlayerState GetPredictedState() => _clientPredictedState;
    private float _interpolationTimer;
    private float _logicFrameIntervalSec = 0.066f; // 66ms
    // 缓存 Ability 引用
    private BaseAbility _ability;

    // 【新增】UI状态缓存，防止每帧重复刷新
    private int _lastUI_Ammo = -1;
    private int _lastUI_HP = -1;
    public void Initialize(Vector3 pos, Quaternion rot, Transform cameraAnchor, int initialHP = -1)
    {
        base.InitializeCommon(); // <--- 调用父类的注入逻辑
        CameraAnchor = cameraAnchor;
        // 初始化状态
        Debug.Log("HPupdate:" + initialHP);

        PlayerState startState = PlayerState.CreateSpawn(pos, rot);
        Debug.Log("HPupdate:" + startState.HP);
        // 2. 【关键】如果传入了继承血量，立刻覆盖！
        if (initialHP != -1)
        {
            startState = startState.WithHP(initialHP);
            Debug.Log("HPupdate:" + startState.HP);
        }
        Debug.Log("HPupdate:" + startState.HP);

        _fromState = startState;
        _toState = startState;
        _clientPredictedState = startState;

        // 应用初始位置
        transform.position = pos;
        transform.rotation = rot;

        _strategy = GetComponent<IInputStrategy>();
        // 自动获取挂在同一个物体上的 Ability 脚本
        _ability = GetComponent<BaseAbility>();

        // 初始化时重置UI缓存
        _lastUI_Ammo = -1;
        _lastUI_HP = -1;
        Debug.Log("新的Hp为" + startState.HP);
        UpdateUI(startState);
        Debug.Log($"[LocalPlayer] 初始化完成。位置:{pos}, HP:{startState.HP}");
    }

    public void CollectInput(InputData data)
    {
        _strategy?.GatherInput(data);
    }

    /// <summary>
    /// 执行预测，并更新插值目标
    /// </summary>
    public PlayerState PredictNextState(InputData input, float dt)
    {
        // 1. 计算下一帧状态 (位置+旋转)
        PlayerState nextState = PredictState(_clientPredictedState, input, dt,false);


        // 2. 更新双缓冲目标 (用于 Update 中的平滑渲染)
        Debug.Log("强制校正状态" + "从状态"+ _fromState.HP);
        _fromState = _clientPredictedState; // 上一帧的预测结果作为起点
        _toState = nextState;               // 这一帧的预测结果作为终点
        Debug.Log("强制校正状态" + "到状态"+ _toState.HP);
        _interpolationTimer = 0f;           // 重置计时器

        // 3. 更新游标
        _clientPredictedState = nextState;

        // 4. 返回状态供 Manager 记录历史
        return nextState;
    }


    /// <summary>
    /// 强制回滚校正
    /// </summary>
    public void TeleportTo(PlayerState correctState)
    {
        transform.position = correctState.Position;
        transform.rotation = correctState.Rotation;

        _fromState = correctState;
        _toState = correctState;
        _clientPredictedState = correctState;
        _interpolationTimer = 0f;
    }

    /// <summary>
    /// 渲染帧平滑移动
    /// </summary>
    public override void UpdateMovement()
    {
        _interpolationTimer += Time.deltaTime;
        float t = Mathf.Clamp01(_interpolationTimer / _logicFrameIntervalSec);
        // ... 计算 t 和 插值 ...
        Vector3 smoothPos = Vector3.Lerp(_fromState.Position, _toState.Position, t);
        Quaternion smoothRot = Quaternion.Slerp(_fromState.Rotation, _toState.Rotation, t);
        //只需要插值物体位置
        PlayerState smoothState = _toState.WithPhysics(smoothPos, smoothRot,_toState.HP);
        // 3. 【新增】更新 UI 逻辑
        UpdateUI(smoothState);

        // 【关键修改】：委托给策略去应用视觉 (解决 Hunter 身体转Y头转X的问题)
        if (_movementProcessor != null)
        {
            _movementProcessor.ApplyVisuals(transform, CameraAnchor, smoothState);
        }
        else
        {
            // 保底逻辑
            transform.position = smoothPos;
            transform.rotation = smoothRot;
        }
    }
    // 专门处理 UI 更新的方法
    private void UpdateUI(PlayerState state)
    {
        // 1. HP逻辑：LocalPlayer 永远对应 UI 左边的 "ChangeLocalHp"
        // 只要血量变了，就更新左边
        if (state.HP != _lastUI_HP)
        {
            _lastUI_HP = state.HP;
            // 获取UI面板并更新左侧血条
            var panel = UIManager.MainInstance.GetPanel(UIConst.PlayingPanel) as PlayingPanel;
            if (panel != null)
            {
                panel.ChangeLocalHp(_lastUI_HP);
            }
        }

        // 2. 子弹逻辑：只有当我是 Hunter 时，中间的子弹数才归我管
        if (MyRole == RoleType.Hunter)
        {
            if (state.Ammo != _lastUI_Ammo)
            {
                _lastUI_Ammo = state.Ammo;
                var panel = UIManager.MainInstance.GetPanel(UIConst.PlayingPanel) as PlayingPanel;
                if (panel != null)
                {
                    panel.SetBulletsNum(_lastUI_Ammo);
                }
            }
        }
    }

}