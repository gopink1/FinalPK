using UnityEngine;

public enum BodyPartType
{
    Body,
    Head,
    Limb
}

public class BodyPartHitbox : MonoBehaviour
{
    [Header("Setup")]
    [Tooltip("指向根节点的控制器")]
    public CharacterControl RootControl;

    [Tooltip("部位类型")]
    public BodyPartType PartType;

    [Tooltip("伤害倍率 (例如头是100，身子是30，这里可以填倍数或者直接填基础扣血量)")]
    // 既然你的需求是具体的数值，我们可以直接定义基础伤害，或者在 HunterMovement 里判断
    // 这里我们只做标记
    public float DamageMultiplier = 1.0f;

    // 自动查找根节点（防止手动拖拽麻烦）
    private void Start()
    {
        if (RootControl == null)
        {
            RootControl = GetComponentInParent<CharacterControl>();
        }
    }
}
