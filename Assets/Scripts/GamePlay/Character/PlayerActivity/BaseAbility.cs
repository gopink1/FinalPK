using UnityEngine;

/// <summary>
/// 表现层基类
/// 负责处理：动画、音效、特效、UI更新（仅限本机）
/// </summary>
public abstract class BaseAbility : MonoBehaviour
{
    [Header("Base Components")]
    public Animator anim;
    public AudioSource audioSource;

    protected virtual void Awake()
    {
        if (anim == null) anim = GetComponentInChildren<Animator>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    /// <summary>
    /// 执行动作指令 (由 Local/Remote Control 调用)
    /// </summary>
    /// <param name="actionCode">来自 InputData 的 Action 字段</param>
    public abstract void ExecuteAction(int actionCode);

    /// <summary>
    /// 更新移动动画 (通用逻辑)
    /// </summary>
    /// <param name="speed">移动速度</param>
    public virtual void UpdateMoveAnim(float speed)
    {
        if (anim != null)
        {
            // 假设 Animator 里有个 "Speed" 参数
            anim.SetFloat("Speed", speed);
        }
    }
}