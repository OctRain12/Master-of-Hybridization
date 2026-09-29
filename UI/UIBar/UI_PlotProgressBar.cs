using UnityEngine;
using UnityEngine.UI;

public class UI_PlotProgressBar : MonoBehaviour
{
    [Header("UI 组件引用")]
    [SerializeField] private Image bgFlowering;     // 开花期底色
    [SerializeField] private Image fillForeground;  // 前景进度条
    [SerializeField] private RectTransform flowerMarker; // 阈值刻度/图标

    [Header("配置参数")]
    [SerializeField] private bool hideWhenEmpty = true;
    // [SerializeField] private float defaultFloweringThreshold = 0.6f;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        EnsureCanvasGroup();
        if (hideWhenEmpty) canvasGroup.alpha = 0f; // 默认隐藏
    }

    /// <summary>
    /// 初始化进度条结构（每次播种时调用）
    /// </summary>
    public void Init(float floweringThreshold)
    {
        if (hideWhenEmpty) canvasGroup.alpha = 1f;

        // 设置开花底色的起始点 (Fill Amount)
        // bgFlowering.fillAmount = 1 - floweringThreshold;
        
        // 由于是 Vertical 填充，需要调整 bgFlowering 的填充原点为 Top(1) 或反转逻辑
        // 更简单的做法：调整刻度图标的 Y 轴锚点
        if (flowerMarker != null)
        {
            flowerMarker.anchorMin = new Vector2(0.5f, floweringThreshold);
            flowerMarker.anchorMax = new Vector2(0.5f, floweringThreshold);
            flowerMarker.anchoredPosition = Vector2.zero; // 精准贴合阈值比例
            // Debug.Log($"修改成功{floweringThreshold}");
        }

        UpdateProgress(0f);
    }

    /// <summary>
    /// 刷新当前进度
    /// </summary>
    public void UpdateProgress(float progress)
    {
        fillForeground.fillAmount = progress;
        
        // 满进度或空地块时隐藏进度条
        if (progress >= 1f || progress <= 0f)
        {
            if (hideWhenEmpty) canvasGroup.alpha = 0f;
        }
        else
        {
            if (hideWhenEmpty) canvasGroup.alpha = 1f;
        }
    }
    /// <summary>
    /// 保证canvasGroup被加载
    /// </summary>
    private void EnsureCanvasGroup()
    {
        if (canvasGroup != null) return;
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }
    public void Hide()
    {
        EnsureCanvasGroup();
        if (hideWhenEmpty) canvasGroup.alpha = 0f;
    }
}