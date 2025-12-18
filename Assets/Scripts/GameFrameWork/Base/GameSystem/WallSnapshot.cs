using System.Collections.Generic;

// 墙壁系统的状态快照
public struct WallSnapshot
{
    // Key: WallID, Value: BrokenFrame (在第几帧碎的)
    public Dictionary<int, int> WallStates;

    public WallSnapshot(Dictionary<int, int> source)
    {
        // 深拷贝
        WallStates = new Dictionary<int, int>(source);
    }
}