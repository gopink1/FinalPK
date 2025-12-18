using UnityEngine;
using UnityEngine.Rendering;

public class HunterMovement : MonoBehaviour, IMovementProcessor
{
    [Header("Weapon Stats")]
    public int MaxAmmo = 3;
    public float FireRate = 1.0f; // 射击间隔秒数

    [Header("Bone Settings")]
    [Tooltip("拖入角色的脊柱或胸部骨骼")]
    public Transform SpineBone;

    [Tooltip("脊柱旋转的偏移修正（如果模型原本是歪的，调整这里）")]
    public Vector3 SpineOffset = Vector3.zero;

    [Header("Weapon")]
    public int Damage = 30;
    public int HeadshotDamage = 100;
    public LayerMask HitLayers; // 记得包含 Runner 的 Layer

    [Header("射线检测偏移量（与摄像机平齐）")]
    public Vector3 OriginOffset = new Vector3(0,3f,0.6f);
    [Header("Config")]
    // 假设你的角色高 1.8米，眼睛大概在 1.6米
    public float EyeHeight = 3.5f;
    // 缓存当前的 Pitch 角度
    private float _currentPitch;
    public Transform CameraAnchor;
    // 【新增】公开方法，供 LaserSight 获取准确的抬枪角度
    public float GetCurrentPitch()
    {
        return _currentPitch;
    }
    public PlayerState CalculateNextState(PlayerState currentState, InputData input, float deltaTime, float speed, bool isCorrection)
    {
        // Hunter 逻辑：AxisX=Yaw(Y轴), AxisY=Pitch(X轴)
        // 构造完整的旋转 (包含上下看)
        Quaternion newRot = Quaternion.Euler(input.AxisY, input.AxisX, 0);

        // 2. 处理冷却时间
        float newCooldown = currentState.FireCooldown - deltaTime;
        if (newCooldown < 0) newCooldown = 0;
        int newAmmo = currentState.Ammo;


        // 3. 处理开火逻辑
        // 条件：按下开火键 + 弹药充足 + 冷却完毕
        if (input.Action == 1 && newAmmo > 0 && newCooldown <= 0)
        {
            newAmmo--;
            newCooldown = 1.0f; // FireRate

            //if (!isCorrection)
            //{
                Vector3 eyePos = currentState.Position + Vector3.up * EyeHeight;
                //Vector3 eyePos = currentState.Position + OriginOffset;
                Quaternion rot = Quaternion.Euler(input.AxisY, input.AxisX, 0);
                Vector3 aimDir = rot * Vector3.forward;
                // 【新增调试日志 - 极其重要】
                Debug.Log($"[Hunter逻辑] 执行射线检测... 发起者:{name} 是否为Correction:{isCorrection}");
                // 1. 获取路径上所有物体
                RaycastHit[] hits = Physics.RaycastAll(eyePos, aimDir, 100f, HitLayers);

                // 2. 按距离排序 (从近到远)
                System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

                if (!isCorrection) Debug.Log($"[Hunter] 开火穿透检测，命中数量: {hits.Length}");

                foreach (var hit in hits)
                {
                    // A. 检测是否是墙壁
                    var wall = hit.collider.GetComponent<DestructibleWall>();
                    if (wall != null)
                    {
                        // 【核心逻辑变更】
                        // 询问系统：在 input.GlobalFrame (例如120帧) 这一刻，这面墙碎了吗？
                        bool isBrokenNow = GameBase.MainInstance.wallSystem.IsWallBrokenAt(wall.WallId, input.GlobalFrame);
                        if (!isCorrection) Debug.Log($"[Hunter] 击穿墙壁: {wall.WallId}");
                        if (isBrokenNow)
                        {
                            // 如果这一帧墙已经是碎的，逻辑上子弹直接穿过去
                            // 不产生阻挡，不触发破坏逻辑（因为它已经碎了）
                            continue;
                        }
                        else
                        {
                            // 如果这一帧墙是好的，子弹击中了它
                            // 1. 把墙标记为“在这一帧破碎”
                            GameBase.MainInstance.wallSystem.OnHitWall(wall.WallId, input.GlobalFrame);

                            // 2. 只有第一次预测时才打印Log
                            if (!isCorrection) Debug.Log($"[Hunter] 击穿墙壁: {wall.WallId} (Frame {input.GlobalFrame})");

                            // 3. 逻辑上穿透：继续检测后面的物体
                            // (你的需求是激光炮穿墙，所以这里是 continue；如果子弹会被墙挡住，这里就是 break)
                            continue;
                        }
                    }

                    // B. 检测是否是身体部位
                    var hitbox = hit.collider.GetComponent<BodyPartHitbox>();
                    if (hitbox != null && hitbox.RootControl != null)
                    {
                        int finalDamage = 0;
                        switch (hitbox.PartType)
                        {
                            case BodyPartType.Head: finalDamage = HeadshotDamage; break;
                            case BodyPartType.Body: finalDamage = Damage; break;
                            case BodyPartType.Limb: finalDamage = (int)(Damage * 0.8f); break;
                        }

                        // 施加伤害
                        hitbox.RootControl.PendingDamage += finalDamage;

                        if (!isCorrection)
                        {
                            Debug.Log($"[Hunter] 穿墙命中 Runner! 伤害: {finalDamage}");
                        }

                        // 打中人后停止穿透 (或者如果你想要一穿二，就去掉 break)
                        break;
                    }
                //}
            }
        }

        // 返回计算后的状态
        return new PlayerState(
           currentState.Position,  // Pos
           newRot,                 // Rot
           newAmmo,                // Hunter Ammo (更新)
           newCooldown,            // Hunter FireCD (更新)
           currentState.HP,
           currentState.ActionState,   // Runner Stance (透传/忽略)
           currentState.FireCooldown  // Runner SkillCD (透传/忽略)
       );
    }
    public void ApplyVisuals(Transform root, Transform anchor, PlayerState state)
    {
        // =======================================================
        // 【核心修复】直接使用 eulerAngles，放弃向量投影法
        // =======================================================

        Vector3 euler = state.Rotation.eulerAngles;

        // 1. 解析 Yaw (Y轴)
        float yaw = euler.y;

        // 2. 解析 Pitch (X轴)
        // Unity 的 eulerAngles.x 范围是 0~360。
        // 比如抬头 10 度可能是 350，低头 10 度是 10。
        // 我们需要把它转回 -180 ~ 180 的格式，以便驱动骨骼。
        float pitch = euler.x;
        if (pitch > 180) pitch -= 360;

        // -------------------------------------------------------

        // 3. 应用 Yaw 到身体根节点
        // 这里直接设置，绝对不要再加 Lerp，因为 state.Rotation 已经是插值过的结果了
        root.rotation = Quaternion.Euler(0, yaw, 0);

        // 确保位置同步
        root.position = state.Position;

        // 4. 记录 Pitch 给 LateUpdate 用
        // 【关键】直接赋值，不要 LerpAngle！
        // 如果这里再平滑一次，就会跟 RemotePlayerControl 的 Slerp 产生“弹簧效应”导致抖动
        _currentPitch = pitch;

        // 5. 应用 Pitch 到摄像机锚点 (如果有)
        if (anchor != null)
        {
            anchor.localRotation = Quaternion.Euler(pitch, 0, 0);
        }

        // 可以在这里更新 UI (子弹数)
        // UIManager.UpdateAmmo(state.Ammo);

    }
    // 【核心】：在动画播放后，强行覆盖脊柱旋转
    private void LateUpdate()
    {
        if (SpineBone != null)
        {
            // 获取当前脊柱的旋转（包含动画）
            // 然后叠加我们的瞄准角度
            // 注意：不同模型的脊柱轴向可能不同，通常是 X 轴，如果歪了请改 Vector3.right

            // 方式 A：增量叠加 (保留呼吸动画)
            //SpineBone.localRotation *= Quaternion.Euler(_currentPitch + SpineOffset.x, SpineOffset.y, SpineOffset.z);

            // 方式 B (如果 A 乱飘)：绝对覆盖 (更稳，但呼吸会消失)
            SpineBone.localRotation = Quaternion.Euler(_currentPitch, 0, 0);
        }
    }
}