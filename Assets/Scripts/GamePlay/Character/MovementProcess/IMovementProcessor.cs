using UnityEngine;

public interface IMovementProcessor
{
    /// <summary>
    /// 纯计算：输入 + 旧状态 = 新状态
    /// </summary>
    // 增加 isCorrection 参数
    PlayerState CalculateNextState(PlayerState currentState, InputData input, float deltaTime, float speed, bool isCorrection);

    /// <summary>
    /// 视觉表现：将插值后的状态应用到物体上
    /// </summary>
    /// <param name="root">角色根物体</param>
    /// <param name="anchor">摄像机锚点 (可选)</param>
    /// <param name="state">当前的平滑状态</param>
    void ApplyVisuals(Transform root, Transform anchor, PlayerState state);
}