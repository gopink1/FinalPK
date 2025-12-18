using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 摄像机管理器
/// </summary>
public class CameraSystem : IGameSystem
{
    private Camera _mainCam;
    private Transform _targetAnchor;
    public CameraSystem(GameBase gameBase) : base(gameBase)
    {
    }

    public override void Init()
    {
        // 获取场景里的主相机
        _mainCam = Camera.main;
        if (_mainCam == null)
        {
            Debug.LogError("BattleScene 缺少 MainCamera!");
        }
    }
    /// <summary>
    /// 绑定到某个玩家的锚点 (由 BattleManager 初始化完成后调用)
    /// </summary>
    public void AttachToPlayer(Transform anchor)
    {
        _targetAnchor = anchor;

        // 简单粗暴的方式：直接作为子物体
        // 优点：自动跟随插值后的平滑移动，无需自己写 Follow 代码
        if (_mainCam != null && _targetAnchor != null)
        {
            _mainCam.transform.SetParent(_targetAnchor);
            _mainCam.transform.localPosition = Vector3.zero;
            _mainCam.transform.localRotation = Quaternion.identity;
        }
    }
    public override void Release()
    {
        // 1. 保护性检查：确保摄像机还存在
        if (_mainCam != null)
        {
            // 【关键步骤】解除父子关系
            // null 代表放回场景的最外层（根节点）
            _mainCam.transform.SetParent(null);

            // 2. (可选) 重置一下位置和旋转，防止它停留在奇怪的角度
            // 比如把它移高一点，或者归零
            _mainCam.transform.position = new Vector3(0, 10, 0);
            _mainCam.transform.rotation = Quaternion.Euler(90, 0, 0); // 俯视视角，适合结算画面
        }

        // 3. 清空引用
        _mainCam = null;
        _targetAnchor = null;
    }

    public override void Update()
    {
    }
}
