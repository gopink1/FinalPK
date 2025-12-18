using UnityEngine;

/// <summary>
/// 玩家状态快照
/// 用于记录某一帧角色的物理信息（位置、旋转等）
/// </summary>
[System.Serializable]
public struct PlayerState
{
    public Vector3 Position;    // 位置
    public Quaternion Rotation; // 旋转

    // 【新增】弹药量
    public int Ammo;

    // 【新增】开火冷却计时器 (用于控制射速)
    public float FireCooldown;

    // 【新增】生命值
    public int HP;
    // 当前处于哪种动作姿态？(0:默认, 1:红色姿态, 2:蓝色姿态...)
    public int ActionState;

    // 共用技能冷却时间 (Float)
    public float SkillCooldown;
    // =========================================================
    // 优化 1：使用默认参数
    // 这样 new PlayerState(pos, rot) 就合法了，后面两个会自动填默认值
    // =========================================================
    // 修改构造函数
    public PlayerState(Vector3 pos, Quaternion rot, int ammo, float fireCd, int hp, int actState, float skillCd)
    {
        Position = pos;
        Rotation = rot;
        Ammo = ammo;
        FireCooldown = fireCd;
        HP = hp;
        ActionState = actState;
        SkillCooldown = skillCd;
    }

    // =========================================================
    // 优化 2：静态工厂方法 (推荐用于 Init/Spawn)
    // 语义更清晰，专门用于创建一个“满状态”的角色
    // =========================================================
    public static PlayerState CreateSpawn(Vector3 pos, Quaternion rot)
    {
        // 在这里统一管理初始数值
        // 如果以后要加 HP = 100，只需要改这一行
        // 默认为 动作0，CD 0
        return new PlayerState(pos, rot, 3, 0f, 100, 0, 0f);
    }
    // 辅助方法：只更新HP
    public PlayerState WithHP(int newHP)
    {
        return new PlayerState(this.Position, this.Rotation, this.Ammo, this.FireCooldown, newHP, this.ActionState, this.SkillCooldown);
    }
    // =========================================================
    // 优化 3：Wither 模式 (部分修改)
    // =========================================================

    // 只改物理，保留逻辑 (Runner用)
    // 之前的 WithPhysics 也要带上 HP
    public PlayerState WithPhysics(Vector3 newPos, Quaternion newRot, int hp)
    {
        return new PlayerState(newPos, newRot, this.Ammo, this.FireCooldown, this.HP, this.ActionState, this.SkillCooldown);
    }


    // 方便调试用的 ToString
    public override string ToString()
    {
        return $"Pos:{Position}, Rot:{Rotation.eulerAngles}";
    }
    public static bool IsClose(PlayerState a, PlayerState b)
    {
        return Vector3.Distance(a.Position, b.Position) < 0.05f &&
               Quaternion.Angle(a.Rotation, b.Rotation) < 2f &&
               a.Ammo == b.Ammo &&
               a.HP == b.HP; // 【新增】校验HP
    }
}