using DG.Tweening;
using TMPro;
using UnityEngine;

public class FloatingTextPanel : PanelBase
{
    [Header("浮动动画参数")]
    [SerializeField] private float moveDuration = 2f;    // 移动持续时间
    [SerializeField] private float fadeDuration = 0.5f;  // 淡入淡出持续时间
    [SerializeField] private Vector3 offset = new Vector3(0, 800, 0); // UI浮动偏移量(像素)

    private TextMeshProUGUI textComponent;
    private CanvasGroup canvasGroup;
    private RectTransform textRectTransform;
    private Vector2 startAnchoredPosition;

    private void Awake()
    {
        // 查找子对象并获取组件
        Transform textTransform = transform.Find("Text");
        if (textTransform != null)
        {
            textComponent = textTransform.GetComponent<TextMeshProUGUI>();
            canvasGroup = textTransform.GetComponent<CanvasGroup>();
            textRectTransform = textTransform.GetComponent<RectTransform>();

            // 如果没有CanvasGroup组件，则添加一个
            if (canvasGroup == null)
                canvasGroup = textTransform.gameObject.AddComponent<CanvasGroup>();

            // 记录初始位置（使用anchoredPosition用于UI元素）
            if (textRectTransform != null)
                startAnchoredPosition = textRectTransform.anchoredPosition;
        }
        else
        {
            Debug.LogError("未找到Text子对象，请检查UI结构");
        }
    }

    public void FloatingText(string content, Vector2? customStartPos = null)
    {
        if (textComponent == null || canvasGroup == null || textRectTransform == null) return;

        // 设置文本内容
        textComponent.text = content;

        // 重置位置和透明度
        if (customStartPos.HasValue)
        {
            textRectTransform.anchoredPosition = customStartPos.Value;
            startAnchoredPosition = customStartPos.Value;
        }
        else
        {
            textRectTransform.anchoredPosition = startAnchoredPosition;
        }
        canvasGroup.alpha = 0f;

        // 创建序列动画
        Sequence sequence = DOTween.Sequence();

        // 淡入
        sequence.Append(canvasGroup.DOFade(1f, fadeDuration).SetUpdate(true));

        // 使用anchoredPosition进行UI移动
        sequence.Join(textRectTransform.DOAnchorPos(startAnchoredPosition + (Vector2)offset, moveDuration)
            .SetEase(Ease.OutQuad).SetUpdate(true));

        // 淡出
        sequence.Append(canvasGroup.DOFade(0f, fadeDuration).SetUpdate(true));

        // 动画完成后回调
        sequence.OnComplete(() => {
            // 可添加回收逻辑
        });
    }

}
