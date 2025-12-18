using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class LaserSight : MonoBehaviour
{
    [Header("Setup")]
    [Tooltip("枪口位置 (空物体)")]
    public Transform FirePoint;

    [Tooltip("激光最大距离")]
    public float MaxDistance = 100f;

    [Tooltip("激光颜色")]
    public Color LaserColor = Color.red;

    [Header("Layer Mask")]
    [Tooltip("激光能打到的层 (比如 Wall, Player)")]
    public LayerMask HitLayers;

    private HunterMovement _hunterMovement;
    private LineRenderer _line;

    private void Awake()
    {
        _line = GetComponent<LineRenderer>();
        InitLineRenderer();
        _hunterMovement = GetComponent<HunterMovement>();
    }

    private void InitLineRenderer()
    {
        _line.startWidth = 0.05f;
        _line.endWidth = 0.05f;
        _line.material = new Material(Shader.Find("Sprites/Default"));
        _line.startColor = LaserColor;
        _line.endColor = new Color(LaserColor.r, LaserColor.g, LaserColor.b, 0.2f);
        _line.positionCount = 2;
    }

    private void LateUpdate()
    {
        if (FirePoint == null) return;

        // 1. 激光的视觉起点：永远是枪口
        Vector3 visualStartPos = FirePoint.position;

        // 2. 激光的视觉终点：默认为 "从眼睛看出去的视线末端"
        // (先给一个默认值，防止没脚本或没打中时的空指针)
        Vector3 visualEndPos = visualStartPos + transform.forward * MaxDistance;

        if (_hunterMovement != null)
        {
            // --- A. 计算逻辑射线 (从眼睛/摄像机出发) ---
            Vector3 eyePos = transform.position + Vector3.up * _hunterMovement.EyeHeight;
            //Vector3 eyePos = transform.position + _hunterMovement.OriginOffset;
            float pitch = _hunterMovement.GetCurrentPitch();
            float yaw = transform.eulerAngles.y;
            Quaternion aimRot = Quaternion.Euler(pitch, yaw, 0);
            Vector3 aimDir = aimRot * Vector3.forward;

            // 【关键修正 1】：默认终点应该是 "眼睛看到的最远点"，而不是枪口的前方
            // 这样即使没打中物体，激光也会指向准星瞄准的方向（汇聚在无限远）
            visualEndPos = eyePos + (aimDir * MaxDistance);

            // --- B. 穿透检测 ---
            RaycastHit[] hits = Physics.RaycastAll(eyePos, aimDir, MaxDistance, HitLayers);

            // 排序：从近到远
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            // --- C. 寻找真正的阻挡点 ---
            foreach (var hit in hits)
            {
                // 如果是墙壁 -> 忽略（穿透）
                if (hit.collider.GetComponent<DestructibleWall>() != null)
                {
                    continue;
                }

                // 如果是 Runner 或 地板/不可破坏环境 -> 阻挡
                // 【关键修正 2】：一旦找到阻挡物，更新终点并退出循环
                visualEndPos = hit.point;
                break;
            }
        }
        else
        {
            // 保底逻辑：如果没有 Movement 脚本，就简单地向前画
            visualEndPos = visualStartPos + transform.forward * MaxDistance;
        }

        // --- D. 绘制 ---
        // 起点：枪口
        // 终点：眼睛看到的落点 (形成了 枪口->落点 的汇聚效果)
        _line.SetPosition(0, visualStartPos);
        _line.SetPosition(1, visualEndPos);
    }
}