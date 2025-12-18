// 挂在 Runner Prefab 上
using UnityEngine;

public class RunnerInput : MonoBehaviour, IInputStrategy
{
    public void GatherInput(InputData data)
    {
        // 采集位移
        data.AxisX = Input.GetAxisRaw("Horizontal"); // -1 到 1

        // 基础操作
        if (Input.GetKey(KeyCode.E)) data.Action = ActionCodes.Skill_E;
        else if (Input.GetKey(KeyCode.W)) data.Action = ActionCodes.Skill_W;
        else if (Input.GetKey(KeyCode.Q)) data.Action = ActionCodes.Skill_Q;

        else data.Action = ActionCodes.None;
    }
}
