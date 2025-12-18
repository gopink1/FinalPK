using UnityEngine;

public class RemotePlayerControl : CharacterControl
{
    // 双缓冲
    private PlayerState _fromState;
    private PlayerState _toState;

    // --- 关键新增：当前逻辑状态缓存 ---
    // 用于记录上一帧插值计算出的完整结果（包含Pitch和Yaw）
    // 绝不能直接读 transform.rotation，因为 transform 可能被 ApplyVisuals 拆解了
    private PlayerState _currentInterpolatedState;

    // 权威基准 (上一帧确认的状态)
    private PlayerState _authoritativeState;
    public int GetLatestHP() => _authoritativeState.HP;

    // 【新增】获取权威数据的接口
    public int GetAuthoritativeAmmo()
    {
        // 注意：这里要返回 _authoritativeState，它是 server input 算出来的绝对正确的值
        // 不要返回 _toState 或 transform，因为那是插值用的
        return _authoritativeState.Ammo;
    }

    public int GetAuthoritativeHP()
    {
        return _authoritativeState.HP;
    }
    public PlayerState GetTargetState() => _toState;
    private float _interpolationTimer;
    private float _logicFrameIntervalSec = 0.066f;
    // 缓存 Ability 引用
    private BaseAbility _ability;

    // 【新增】UI状态缓存，防止每帧重复刷新
    private int _lastUI_Ammo = -1;
    private int _lastUI_HP = -1;
    public void InitializeAuthoritativeState(Vector3 pos, Quaternion rot, int initialHP = -1)
    {
        PlayerState startState = PlayerState.CreateSpawn(pos, rot);
        // 【新增】覆盖 HP
        // 【关键】覆盖继承血量
        if (initialHP != -1)
        {
            startState = startState.WithHP(initialHP);
        }

        _authoritativeState = startState;
        _fromState = startState;
        _toState = startState;

        _currentInterpolatedState = startState; // 初始化缓存

        transform.position = pos;
        transform.rotation = rot;
        // 自动获取挂在同一个物体上的 Ability 脚本
        _ability = GetComponent<BaseAbility>();
        // 初始化时重置UI缓存
        _lastUI_Ammo = -1;
        _lastUI_HP = -1;
    }

    /// <summary>
    /// 根据服务端发来的输入，推演下一帧
    /// </summary>
    public void UpdateFromServerInput(InputData serverInput)
    {
        // 1. 推演下一帧权威状态
        PlayerState nextState = PredictState(_authoritativeState, serverInput, _logicFrameIntervalSec, false);

        // =========================================================
        // 【核心修复】：结算“被动伤害”
        // 无论是 Runner 算 Hunter 的伤害，还是 Hunter 算 Runner 的伤害
        // 只要 PendingDamage 有值，就得扣
        // =========================================================
        int currentHP = _authoritativeState.HP;

        if (PendingDamage > 0)
        {
            currentHP -= PendingDamage;
            if (currentHP < 0) currentHP = 0;

            // 可以在这里播受击特效
            Debug.Log($"[RemotePlayer] ID:{serverInput.Id} 扣血:{PendingDamage} 剩余HP:{currentHP}");

            // 归零，防止重复扣血
            PendingDamage = 0;
        }

        // 将新的 HP 写入状态
        nextState = nextState.WithHP(currentHP);

        // 2. 设置插值目标
        // 【核心修复】：起点使用内存中记录的完整状态，而不是被拆解过的 transform
        _fromState = _currentInterpolatedState;

        _toState = nextState;
        _interpolationTimer = 0f;

        // 3. 更新权威基准
        _authoritativeState = nextState;


        if (_ability != null && serverInput.Action != 0)
        {
            _ability.ExecuteAction(serverInput.Action);
        }
    }

    public override void UpdateMovement()
    {
        _interpolationTimer += Time.deltaTime;
        float t = Mathf.Clamp01(_interpolationTimer / _logicFrameIntervalSec);

        // 1. 计算完整的数学插值状态
        Vector3 smoothPos = Vector3.Lerp(_fromState.Position, _toState.Position, t);
        Quaternion smoothRot = Quaternion.Slerp(_fromState.Rotation, _toState.Rotation, t);

        PlayerState smoothState = _toState.WithPhysics(smoothPos, smoothRot, _toState.HP);


        // 2. 【关键】：更新缓存，供下一帧 UpdateFromServerInput 使用
        _currentInterpolatedState = smoothState;
        // 3. 【新增】更新敌人信息 UI
        UpdateUI(smoothState);

        // 3. 视觉表现委托 (ApplyVisuals 会把 Rotation 拆成 Yaw给身体, Pitch给脊柱)
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
        // 1. HP逻辑：RemotePlayer 永远对应 UI 右边的 "ChangeRemoteHp"
        if (state.HP != _lastUI_HP)
        {
            _lastUI_HP = state.HP;
            // 获取UI面板并更新右侧血条
            var panel = UIManager.MainInstance.GetPanel(UIConst.PlayingPanel) as PlayingPanel;
            if (panel != null)
            {
                panel.ChangeRemoteHp(_lastUI_HP);
            }
        }

        // 2. 子弹逻辑：如果对手是 Hunter，说明中间的子弹数应该显示他的数据
        // (例如：我是 Runner，我需要看到 Hunter 还有几发子弹)
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