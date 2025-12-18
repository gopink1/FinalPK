using UnityEngine;

public class HunterAbility : BaseAbility
{
    [Header("Hunter Visuals")]
    public Transform firePoint;        // 枪口位置
    public GameObject muzzleFlashVFX;  // 枪口火花预制体
    public LineRenderer bulletTrail;   // 子弹轨迹线 (LineRenderer)

    [Header("Hunter Settings")]
    public float shotEffectDuration = 0.05f;

    // 状态标记
    private bool isLocalPlayer;
    private float fireTimer = 0;

    public ParticleSystem MuzzleFlash;
    public AudioSource GunAudio;

    protected override void Awake()
    {
        base.Awake();
        // 如果挂载了 LocalPlayerControl，说明是本机玩家，需要处理UI
        isLocalPlayer = GetComponent<LocalPlayerControl>() != null;

        if (bulletTrail != null)
            bulletTrail.enabled = false;
    }

    private void Start()
    {

    }

    private void Update()
    {
        // 处理特效消失计时
        if (fireTimer > 0)
        {
            fireTimer -= Time.deltaTime;
            if (fireTimer <= 0 && bulletTrail != null)
            {
                bulletTrail.enabled = false;
            }
        }
    }
    // 通用表现接口：播放开火特效
    public void PlayFireEffects()
    {
        Debug.Log("开火！");
        if (GunAudio != null) GunAudio.Play();

        // 如果有后坐力震屏，写在这里
    }

    /// <summary>
    /// 帧同步系统的统一入口
    /// </summary>
    public override void ExecuteAction(int actionCode)
    {
        // 1 = 开火
        if (actionCode == 1)
        {
            PlayFireEffects();
        }
    }
}