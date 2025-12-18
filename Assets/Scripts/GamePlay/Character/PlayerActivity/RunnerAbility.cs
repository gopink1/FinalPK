using UnityEngine;

public class RunnerAbility : BaseAbility
{
    [Header("Runner Settings")]
    // 动作平滑过渡速度
    public float dampTime = 0.1f;

    private Vector3 lastPosition;
    private bool isCrouching = false;
    private int _lastActionState = -1;
    protected override void Awake()
    {
        base.Awake();
        lastPosition = transform.position;
    }

    private void Update()
    {
        // --- 1. 自动计算移动速度 ---
        // 无论是由 LocalControl 预测移动，还是 RemoteControl 插值移动，
        // 我们只需要比较这一帧和上一帧的位置差，就能算出动画速度。
        // 这样解耦最干净，不需要 Control 脚本告诉我们速度。

        float dist = Vector3.Distance(transform.position, lastPosition);
        float speed = dist / Time.deltaTime;

        // 假设 Animator 有个 "Speed" 参数 (0~1 或 0~MaxSpeed)
        //if (anim != null)
        //{
        //    anim.SetFloat("Speed", speed, dampTime, Time.deltaTime);
        //}

        lastPosition = transform.position;
    }
    // 【核心】负责维持状态：确保动画机一直在播放正确的跑步姿势
    // 由 ApplyVisuals 每帧调用
    public void UpdateActionVisuals(int actionState)
    {
        if (_lastActionState != actionState)
        {
            _lastActionState = actionState;
            if (anim != null)
            {
                switch (actionState)
                {
                    case 1:
                        anim.Play("Pose01");
                        Debug.Log("[RunnerAbility]" + "切换到动作Pose01");
                        break;
                    case 2:
                        
                        anim.Play("Pose02");
                        Debug.Log("[RunnerAbility]" + "切换到动作Pose02");
                        break;
                    case ActionCodes.Skill_E:
                        break;
                    default:
                        break;
                }
            }
        }
    }

    // 【辅助】负责瞬间反馈：比如切换时的音效
    // 由 HandleServerFrame -> UpdateFromServerInput 调用
    public override void ExecuteAction(int actionCode)
    {
        // 假设我们定义 ActionCode 10 是切换动作的按键
        if (actionCode == 10)
        {
            //// 播放变身音效 (一次性)
            //AudioSource.PlayClipAtPoint(switchSound, transform.position);
            //// 播放一阵烟雾 (一次性)
            //smokeParticle.Play();
        }
    }
}