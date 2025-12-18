using System.Collections.Generic;
using UnityEngine;

public class WallSystem : IGameSystem
{
    private Dictionary<int, DestructibleWall> _wallMap = new Dictionary<int, DestructibleWall>();
    private GameObject _wallPrefab;
    private Transform _wallRoot;

    // 记录碎墙ID的HashSet (用于回滚)
    private HashSet<int> _brokenWallIds = new HashSet<int>();

    // ==========================================
    // 【自定义配置区域】
    // ==========================================

    // 1. 墙壁积木的物理尺寸 (根据你的描述)
    // 假设墙壁积木在它的"右边"是宽，"上边"是高
    private float _unitWidth = 0.6f;
    private float _unitHeight = 0.4f;

    // 2. 生成多少个积木 (几乘几)
    private int _cols = 12; // 横向生成多少个 (宽度)
    private int _rows = 12;  // 纵向生成多少个 (高度)

    // 3. 墙壁的起始点 (世界坐标)
    // 比如在 X=15 的位置开始生成
    private Vector3 _startPosWall1 = new Vector3(28f, 0.4f, 5f);

    private Vector3 _startPosWall2 = new Vector3(28f, 0.4f, 15f);
    private Vector3 _startPosWall3 = new Vector3(28f, 0.4f, 25f);
    private Vector3 _startPosWall4 = new Vector3(28f, 0.4f, 35f);
    int idCounter = 0;
    public WallSystem(GameBase gameBase) : base(gameBase)
    {
        _wallPrefab = Resources.Load<GameObject>("SceneModel/WallCube");
    }

    public override void Init()
    {
        // 只在整个系统初始化时清空一次
        _wallMap.Clear();
        _brokenWallIds.Clear();
        idCounter = 0;

        _wallRoot = new GameObject("WallRoot").transform;

        // 依次生成，ID 会持续累加，不会被重置
        GenerateBrickWall(_startPosWall1);
        GenerateBrickWall(_startPosWall2);
        GenerateBrickWall(_startPosWall3);
        GenerateBrickWall(_startPosWall4);

        Debug.Log($"[WallSystem] 所有墙壁初始化完成，总方块数: {idCounter}");
    }

    /// <summary>
    /// 生成一面由小方块组成的砖墙
    /// </summary>
    private void GenerateBrickWall(Vector3 startPos)
    {

        // =======================================================
        // 【核心数学逻辑】
        // 既然你要墙面正面朝向 -X (Vector3.left)
        // 那么墙壁的"平面"就是 Y-Z 平面。
        // =======================================================
        Vector3 spos = startPos;

        // A. 确定墙壁积木的朝向
        // 你的积木局部Z轴是正面。我们想让它面向世界 -X。
        // Quaternion.LookRotation(前方方向, 上方方向)
        Quaternion wallRotation = Quaternion.LookRotation(Vector3.left, Vector3.up);

        // B. 确定墙壁的延伸方向
        // 高度方向：肯定是世界 Y 轴 (Vector3.up)
        Vector3 heightDir = Vector3.up;

        // 宽度方向：既然面向 X，那么宽度就在 Z 轴上延伸 (Vector3.forward)
        Vector3 widthDir = Vector3.forward;

        // 开始双层循环生成 (像砌砖一样)
        for (int r = 0; r < _rows; r++) // 每一层 (Y轴)
        {
            for (int c = 0; c < _cols; c++) // 每一列 (Z轴)
            {
                // 计算当前积木的世界坐标
                // 公式：起点 + (列号 * 宽度方向 * 积木宽) + (行号 * 高度方向 * 积木高)
                Vector3 spawnPos = spos
                                   + (widthDir * c * _unitWidth)
                                   + (heightDir * r * _unitHeight);

                // 生成物体
                GameObject wallObj = GameObject.Instantiate(_wallPrefab, spawnPos, wallRotation, _wallRoot);

                // 设置名字和层级 (方便调试)
                wallObj.name = $"Wall_R{r}_C{c}_{idCounter}";
                // 确保它有 DestructibleWall 组件
                DestructibleWall wallScript = wallObj.GetComponent<DestructibleWall>();
                if (wallScript == null) wallScript = wallObj.AddComponent<DestructibleWall>();

                // 初始化 ID
                wallScript.Init(idCounter);
                _wallMap.Add(idCounter, wallScript);

                idCounter++;
            }
        }

        Debug.Log($"[WallSystem] 生成了 {_rows}层 x {_cols}列 的砖墙，共 {idCounter} 块。");
        Debug.Log($"[WallSystem] 墙壁朝向: -X, 延伸方向: Z(宽) & Y(高)");
    }

    /// <summary>
    /// 核心逻辑：检查某面墙在特定帧是否是破碎的
    /// </summary>
    public bool IsWallBrokenAt(int wallId, int checkFrame)
    {
        if (_wallMap.TryGetValue(wallId, out var wall))
        {
            // 如果墙碎了(BrokenFrame != -1) 并且 碎的时间(BrokenFrame) <= 当前检查的时间(checkFrame)
            // 那么在那个时间点，它就是碎的
            return wall.BrokenFrame != -1 && wall.BrokenFrame <= checkFrame;
        }
        return false;
    }

    /// <summary>
    /// 逻辑执行：标记破碎
    /// </summary>
    public void OnHitWall(int wallId, int hitFrame)
    {
        if (_wallMap.TryGetValue(wallId, out var wall))
        {
            // 如果还没碎，或者新的破碎时间更早，就更新它
            // 注意：这里只改数据，不动 Renderer！
            if (wall.BrokenFrame == -1 || hitFrame < wall.BrokenFrame)
            {
                wall.BrokenFrame = hitFrame;
            }
        }
    }
    // ==========================================
    // 数据回滚 API (静默回滚)
    // ==========================================

    public WallSnapshot GetSnapshot()
    {
        // 只记录碎掉的墙，节省内存
        Dictionary<int, int> states = new Dictionary<int, int>();
        foreach (var kvp in _wallMap)
        {
            if (kvp.Value.BrokenFrame != -1)
            {
                states.Add(kvp.Key, kvp.Value.BrokenFrame);
            }
        }
        return new WallSnapshot(states);
    }

    public void RestoreSnapshot(WallSnapshot snapshot)
    {
        // 1. 遍历所有墙
        foreach (var kvp in _wallMap)
        {
            int id = kvp.Key;
            DestructibleWall wall = kvp.Value;

            // 2. 查找快照里这面墙的状态
            if (snapshot.WallStates.TryGetValue(id, out int savedFrame))
            {
                // 快照里它是碎的 -> 恢复成那个破碎时间
                wall.BrokenFrame = savedFrame;
            }
            else
            {
                // 快照里没它 -> 说明它是好的
                wall.BrokenFrame = -1;
            }
            // 【重点】这里完全没有调用 wall.SetVisualState()
            // 所以 Unity 的画面和物理完全不动，没有任何开销和闪烁！
        }
    }

    // ==========================================
    // 表现刷新 API
    // ==========================================

    /// <summary>
    /// 在一切计算结束后，统一刷新画面
    /// </summary>
    public void RefreshVisuals(int currentRenderFrame)
    {
        foreach (var wall in _wallMap.Values)
        {
            // 只有当墙的破碎时间 <= 当前渲染时间，才在画面上显示为碎
            bool shouldLookBroken = (wall.BrokenFrame != -1 && wall.BrokenFrame <= currentRenderFrame);

            // 这里才真正操作 Renderer
            wall.SetVisualState(shouldLookBroken);
        }
    }
    public override void Update() { }
    public override void Release()
    {
        if (_wallRoot != null) GameObject.Destroy(_wallRoot.gameObject);
        _wallMap.Clear();
    }
}