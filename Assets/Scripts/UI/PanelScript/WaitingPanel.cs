using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class WaitingPanel : PanelBase
{
    private TextMeshProUGUI TipText;
    private TextMeshProUGUI TimerText; // 可选，显示数字

    private Action _onCompleteCallback;
    private float _timer;
    private bool _isRunning;

    private void Awake()
    {
        TipText = transform.Find("Text").GetComponent<TextMeshProUGUI>();
        TimerText = transform.Find("Num").GetComponent<TextMeshProUGUI>();
    }

    /// <summary>
    /// 初始化等待
    /// </summary>
    /// <param name="duration">锁定几秒</param>
    /// <param name="message">提示文字</param>
    /// <param name="onComplete">倒计时结束后的回调</param>
    public void Setup(float duration, string message, Action onComplete = null)
    {
        _timer = duration;
        _onCompleteCallback = onComplete;
        _isRunning = true;

        if (TipText != null) TipText.text = message;
    }

    private void Update()
    {
        if (!_isRunning) return;

        _timer -= Time.deltaTime;

        if (TimerText != null)
            TimerText.text = Mathf.CeilToInt(_timer).ToString();

        if (_timer <= 0)
        {
            _isRunning = false;
            FinishWait();
        }
    }

    private void FinishWait()
    {

        _onCompleteCallback?.Invoke();

        // 自动关闭
        UIManager.MainInstance.ClosePanel(UIConst.WaitingPanel);
    }
}