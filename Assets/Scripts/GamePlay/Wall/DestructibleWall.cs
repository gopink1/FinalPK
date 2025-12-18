using UnityEngine;

public class DestructibleWall : MonoBehaviour
{
    public int WallId { get; private set; }

    // 【核心修改】不再只存 bool，而是存“在哪一帧碎的”
    // -1 代表没碎
    public int BrokenFrame { get; set; } = -1;

    private Collider[] _colliders;
    private Renderer[] _renderers;
    // 注意：Collider 我们将保持常开，或者由系统统一管理，这里不再频繁开关

    private void Awake()
    {
        _colliders = GetComponentsInChildren<Collider>();
        _renderers = GetComponentsInChildren<Renderer>();
    }

    public void Init(int id)
    {
        WallId = id;
        BrokenFrame = -1; // 重置
        // 初始化时，确保物理和视觉都是好的
        SetVisualState(false);
    }
    /// <summary>
    /// 根据当前状态刷新 Unity 组件
    /// 这个方法只在每一帧的最后调用一次
    /// </summary>
    public void SetVisualState(bool isBroken)
    {
        // 如果 isBroken 是 true (碎了)，enabled 应该是 false (隐藏)
        // 如果 isBroken 是 false (好的)，enabled 应该是 true (显示)
        bool shouldEnable = !isBroken;

        // 优化：避免每帧重复赋值，只有状态变了才改
        // 假设 renderers 数组里第一个的状态代表了整体状态
        if (_renderers.Length > 0 && _renderers[0].enabled != shouldEnable)
        {
            foreach (var r in _renderers) r.enabled = shouldEnable;

            // 加个日志验证一下 Runner 是否真的执行到了这里
            Debug.Log($"[Wall {WallId}] 视觉状态更新: {shouldEnable} (IsBroken: {isBroken})");
        }
    }
    /// <summary>
    /// 标记墙壁在某一帧破碎
    /// </summary>
    public void MarkBroken(int frame)
    {
        // 如果已经碎了，且碎得比现在更早，就不改（保留最早的破碎时间）
        if (BrokenFrame != -1 && BrokenFrame <= frame) return;

        BrokenFrame = frame;
        // 注意：这里不再直接关 Renderer/Collider，而是等待统一刷新
    }

    /// <summary>
    /// 仅仅更新视觉表现 (给 Update 用)
    /// </summary>
    /// <param name="isBroken">当前是否应该显示为破碎</param>
    public void UpdateVisuals(bool isBroken)
    {
        // 只有状态改变时才操作，减少开销
        bool shouldEnable = !isBroken;
        if (_renderers.Length > 0 && _renderers[0].enabled != shouldEnable)
        {
            foreach (var r in _renderers) r.enabled = shouldEnable;
        }
        // Collider 建议一直保持开启，依靠 Raycast 逻辑去过滤
    }
}