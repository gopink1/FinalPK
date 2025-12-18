// 挂在 Hunter Prefab 上
using UnityEngine;

public class HunterInput : MonoBehaviour, IInputStrategy
{
    [Header("FPS Settings")]
    public float MouseSensitivity = 2.0f; // 鼠标灵敏度
    public float MaxVerticalAngle = 80f;  // 仰俯角限制

    // 本地累积的旋转角度
    private float _currentYaw = 0f;   // 左右旋转 (Y轴)
    private float _currentPitch = 0f; // 上下瞄准 (X轴) - 如果你需要同步枪口抬起角度

    private HunterAbility _ability;
    private float _localFireTimer = 0; // 本地防抖动，防止按住一直播特效
    private void Awake()
    {
        _ability = GetComponent<HunterAbility>();
    }
    private void Start()
    {
        // 【关键】FPS模式必须锁定鼠标
        // 注意：在进入战斗场景初始化时调用最好，或者在这里做个保底
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // 【核心修复】初始化时，将累积变量对齐到当前物体的实际角度
        Vector3 currentEuler = transform.eulerAngles;
        _currentYaw = currentEuler.y;
        _currentPitch = 0f; // 假设刚出生是平视

        // 防止万一角度超过 180 度导致的计算跳变
        if (_currentYaw > 180) _currentYaw -= 360;
    }

    public void GatherInput(InputData data)
    {
        // 1. 获取鼠标移动量 (Delta)
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // 2. 累积计算绝对角度
        // 左右旋转 (Yaw)
        _currentYaw += mouseX * MouseSensitivity;

        // 上下瞄准 (Pitch) - 即使CharacterControl暂时只用了Y轴，建议也先算好存起来
        _currentPitch -= mouseY * MouseSensitivity;
        _currentPitch = Mathf.Clamp(_currentPitch, -MaxVerticalAngle, MaxVerticalAngle);


        // 3. 填充数据
        // AxisX: 传递身体/枪口的水平朝向 (0~360)
        data.AxisX = _currentYaw;

        // AxisY: 传递枪口的垂直朝向 (用于服务端判定是否打中头还是脚，或者打中地板)
        data.AxisY = _currentPitch;

        // 动作
        if (Input.GetMouseButton(0))
        {
            data.Action = 1;

            // 【关键】：本地表现先行！
            // 为了防止每帧都播特效，加个简单的本地计时器检查
            // 这个计时器不需要和服务端完全同步，只是为了视觉效果不鬼畜
            if (Time.time > _localFireTimer)
            {
                _ability.PlayFireEffects();
                _localFireTimer = Time.time + 1f; // 这里的 0.5f 最好读取 HunterMovement.FireRate
            }
        }
        else
        {
            data.Action = 0;
        }
    }

    // 如果在编辑器里想释放鼠标按 ESC
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}