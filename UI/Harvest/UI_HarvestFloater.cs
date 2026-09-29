using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_HarvestFloater : MonoBehaviour
{
    [Header("UI 引用")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform rectTransform;

    [Header("动效参数")]
    [SerializeField] private float floatDuration = 0.8f;   // 整体漂浮时长
    [SerializeField] private float floatDistance = 50f;    // 向上漂浮的像素距离
    [SerializeField] private float overshootScale = 1.3f;  // 弹跳最大缩放倍数
    [SerializeField] private float popDuration = 0.15f;    // 弹出弹跳耗时

    public void Trigger(Sprite icon, int amount, Vector2 startPosition)
    {
        iconImage.sprite = icon;
        amountText.text = $"+{amount}";
        // canvasGroup.alpha = 0f;
        rectTransform.position = startPosition; // UI 坐标直接赋值
        gameObject.SetActive(true);
        
        StartCoroutine(FloatRoutine());
    }

    private IEnumerator FloatRoutine()
    {
        // yield return null;
        float elapsed = 0f;
        Vector2 startPos = rectTransform.anchoredPosition;
        Vector2 targetPos = startPos + new Vector2(0, floatDistance);

        while (elapsed < floatDuration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / floatDuration;

            // 1. 位置向上平滑移动 (Ease Out 效果)
            float moveT = 1f - Mathf.Pow(1f - normalizedTime, 3f);
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, moveT);

            // 2. 弹出与缩放 (前半段弹跳，后半段稳定)
            if (elapsed < popDuration)
            {
                float popT = elapsed / popDuration;
                // 简单的类似弹簧的抛物线放大再缩小
                float currentScale = Mathf.Lerp(0.5f, overshootScale, Mathf.Sin(popT * Mathf.PI));
                rectTransform.localScale = new Vector3(currentScale, currentScale, 1f);
            }
            else
            {
                rectTransform.localScale = Vector3.one;
            }

            // 3. 后半段 Alpha 渐隐
            if (normalizedTime > 0.5f)
            {
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, (normalizedTime - 0.5f) * 2f);
            }
            else
            {
                canvasGroup.alpha = 1f;
            }

            yield return null;
        }
        // 动效结束，放回对象池
        gameObject.SetActive(false);
        FloaterPoolManager.Instance.ReturnToPool(this);
    }
}
