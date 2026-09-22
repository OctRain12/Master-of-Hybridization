using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// 顶部 HUD 的金币显示。
/// 负责：
/// 1. 显示当前金币数
/// 2. 金币变化时刷新文本
/// 3. 金币不足时播放警告动画（变红 + 左右晃动）
/// </summary>
public class UI_GoldDisplay : MonoBehaviour
{
    [Header("引用")]
    public TextMeshProUGUI goldText;          // 显示金币数字文本

    [Header("颜色")]
    public Color normalColor = Color.white;   // 正常颜色
    public Color warningColor = Color.red;    // 警告颜色

    private RectTransform rectTransform;      // 自己的 RectTransform，用于晃动
    private Vector2 originalPosition;         // 记录初始位置，晃动完要还原
    private Coroutine warningRoutine;         // 当前正在跑的警告协程引用

    private void Awake()
    {
        // 缓存自己的 RectTransform，避免每次晃动都 GetComponent
        rectTransform = GetComponent<RectTransform>();

        // 记录初始 anchoredPosition，晃动结束后要回到这里
        originalPosition = rectTransform.anchoredPosition;
    }
        private void OnEnable()
    {
        // 面板启用时订阅钱包事件
        if (WalletManager.Instance != null)
        {
            // 先同步一次当前金币，避免显示旧数据
            goldText.text = WalletManager.Instance.GetGold().ToString();

            // 金币变化 → 刷新文本
            WalletManager.Instance.OnGoldChanged += UpdateGoldText;

            // 金币不足 → 播放警告动画
            WalletManager.Instance.OnInsufficientFunds += TriggerWarning;
        }
    }
        private void OnDisable()
    {
        // 面板禁用时取消订阅
        if (WalletManager.Instance != null)
        {
            WalletManager.Instance.OnGoldChanged -= UpdateGoldText;
            WalletManager.Instance.OnInsufficientFunds -= TriggerWarning;
        }
    }
    /// <summary>
    /// 金币变化时刷新文本。
    /// </summary>
    /// <param name="currentGold">当前金币</param>
    /// <param name="delta">本次变化量（正数为增加，负数为减少）</param>
    private void UpdateGoldText(int currentGold, int delta)
    {
        goldText.text = currentGold.ToString();
        // 此处可扩展：数字平滑滚动动画 (Number Lerp)
    }
    /// <summary>
    /// 触发警告动画。
    /// 如果上一次警告还没播完，先停掉，避免动画叠加。
    /// </summary>
    private void TriggerWarning()
    {
        // 防止重复触发导致多个协程同时跑
        if (warningRoutine != null) StopCoroutine(warningRoutine);

        warningRoutine = StartCoroutine(WarningAnimationRoutine());

        // SoundManager.Instance?.PlaySFX(SFXType.Error); // 音效改为事件触发
    }
    /// <summary>
    /// 警告动画：变红 + 左右晃动，然后还原。
    /// </summary>
    private IEnumerator WarningAnimationRoutine()
    {
        // 先变红
        goldText.color = warningColor;

        // 左右晃动，持续 0.3 秒
        float duration = 0.3f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            // 用 sin 函数产生左右摆动
            // 40f 控制频率，10f 控制幅度
            // (1f - elapsed / duration) 让幅度随时间衰减，看起来更自然
            float offsetX = Mathf.Sin(elapsed * 40f) * 10f * (1f - elapsed / duration);

            // 在初始位置基础上加偏移
            rectTransform.anchoredPosition = originalPosition + new Vector2(offsetX, 0);

            yield return null; // 等下一帧
        }

        // 动画结束，还原位置和颜色
        rectTransform.anchoredPosition = originalPosition;
        goldText.color = normalColor;
    }
}
