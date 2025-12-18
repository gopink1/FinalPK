using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FramePredictionState
{
    public int FrameNumber;
    public PlayerState State; // 记录当时预测的位置和旋转
    public WallSnapshot WorldWallState; // 新增
}
