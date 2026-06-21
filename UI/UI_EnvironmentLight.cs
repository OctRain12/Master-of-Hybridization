using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_EnvironmentLight : MonoBehaviour
{
    [Header("全屏调色滤镜引用")]
    public Image screenFilterImage;

    [Header("全天色彩渐变配置")]
    [Tooltip("配置 0点(午夜)-12点(正午)-24点(午夜) 的滤镜颜色")]
    public Gradient dayNightGradient;

    void Start()
    {
        if (screenFilterImage == null)
            screenFilterImage = GetComponent<Image>();
    }
    void Update()
    {
        if (TimeManager.Instance == null || screenFilterImage == null) return;

        // 1. 获取当前时间在全天 24小时 里的百分比 (0.0 ~ 1.0)
        float timePercent = TimeManager.Instance.GetTimePercent();

        // 2. 采样渐变色带，并赋给全屏滤镜
        Color targetColor = dayNightGradient.Evaluate(timePercent);
        screenFilterImage.color = targetColor;
    }
}
