using System.Collections.Generic;
using UnityEngine;

public class RunnerMovement : MonoBehaviour, IMovementProcessor
{
    // 获取 CharacterControl 以便读取 PendingDamage
    private CharacterControl _control;
    // 配置：共用 CD 时间
    private const float TotalSharedCD = 3.0f;

    // --- 核心设计：动作配置表 ---
    // Key: Input里的 ActionCode (比如 10, 11)
    // Value: 对应的 StanceIndex (比如 1, 2)
    private Dictionary<int, int> _skillConfig;
    private void Awake()
    {
        // 尝试找一下，找不到也不要报错，因为可能是动态挂载的
        _control = GetComponent<CharacterControl>();
        if (_control == null) _control = GetComponentInParent<CharacterControl>();

        // 【易扩展】：想加新技能，就在这里配一行，不用改下面逻辑
        _skillConfig = new Dictionary<int, int>()
        {
            { ActionCodes.Skill_Q,   1 }, // Q键 -> 姿态1
            { ActionCodes.Skill_W,  2 }, // W键 -> 姿态2
            { ActionCodes.Skill_E, 3 }  // E键 -> 姿态3
        };
    }

    // 【新增】提供一个公开方法，让外部注入 Control
    public void SetupControl(CharacterControl ctrl)
    {
        _control = ctrl;
        // Debug.Log($"[RunnerMovement] 成功注入控制器: {_control.name}");
    }
    public PlayerState CalculateNextState(PlayerState currentState, InputData input, float deltaTime, float speed, bool isCorrection)
    {
        //技能计算
        // 1. 继承旧状态
        int nextStance = currentState.ActionState;
        float nextCD = currentState.SkillCooldown;

        // 2. CD 计时 (帧同步核心：必须每帧计算)
        if (nextCD > 0) nextCD -= deltaTime;
        if (nextCD < 0) nextCD = 0;

        // 3. 处理技能输入
        // 核心判断：输入了某个技能键 且 CD 转好了
        if (nextCD <= 0 && _skillConfig.TryGetValue(input.Action, out int targetStance))
        {
            // 只有当“想切的状态”和“当前状态”不一样时才触发（可选）
            if (nextStance != targetStance)
            {
                nextStance = targetStance; // 切换姿态
                nextCD = TotalSharedCD;    // 重置共用 CD

                if (!isCorrection)
                    Debug.Log($"[Runner] 技能释放! Action:{input.Action} -> Stance:{nextStance}");
            }
        }

        Vector3 newPos = currentState.Position;
        Quaternion newRot = currentState.Rotation;

        // Runner 逻辑：本地坐标系移动
        Vector3 localDir = new Vector3(input.AxisX, 0, 0); // 只取左右

        if (Mathf.Abs(input.AxisX) > 0.01f)
        {
            // 螃蟹步：向自己的侧面移动
            Vector3 worldMoveDir = currentState.Rotation * localDir;
            newPos += worldMoveDir.normalized * speed * deltaTime;

            // Runner 保持朝向不变 (Strafing)
            // 如果你想让Runner转身，就在这里改 newRot
        }

        // --- 伤害结算逻辑 ---
        int currentHP = currentState.HP;
        //if (isCorrection)
        //{
        //    Debug.LogError($"[Runner自查] 我是: {_control.name}, ID: {_control.GetInstanceID()}, 当前PendingDamage: {_control.PendingDamage}");
        //}
        if (_control != null && _control.PendingDamage > 0)
        {
            // 打印日志方便调试
            Debug.Log($"[Runner] 结算伤害: {_control.PendingDamage}。 当前HP: {currentHP} -> {currentHP - _control.PendingDamage} (Correction模式: {isCorrection})");

            currentHP -= _control.PendingDamage;
            if (currentHP < 0) currentHP = 0;

            // 注意：这里不需要把 PendingDamage 归零
            // 因为下一帧 PredictNextFrame 开始时会自动调用 ResetFrameLogic() 归零
        }

        // 返回新状态 (HP 可能减少了)
        return new PlayerState(newPos, 
            newRot, 
            currentState.Ammo, 
            currentState.FireCooldown, 
            currentHP, 
            nextStance, 
            nextCD);
    }

    public void ApplyVisuals(Transform root, Transform anchor, PlayerState state)
    {
        // Runner 很简单：身体直接应用全部旋转和位置
        root.position = state.Position;
        root.rotation = state.Rotation;

        // Runner 的锚点通常是固定的，不需要动
        if (anchor != null)
        {
            anchor.localRotation = Quaternion.identity;
        }

        // --- 表现层更新 ---
        // 获取身上的 Ability 脚本来播放动画
        var ability = root.GetComponent<RunnerAbility>();
        if (ability != null)
        {
            // 将纯数据 (PoseIndex) 传给表现层
            ability.UpdateActionVisuals(state.ActionState);
        }
    }
}