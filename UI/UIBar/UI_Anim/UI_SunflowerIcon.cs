using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UI_SunflowerIcon : MonoBehaviour
{
    [Header("引用")]
    public Image iconImage;
    public Sprite daySprite;
    public Sprite nightSprite;

    [Header("动画参数")]
    public float scaleDuration = 0.3f;
    public float rotateDuration = 0.5f;
    public float scaleUp = 1.3f;

    private Coroutine switchRoutine;

    private void Awake()
    {
        // 组件自动获取
        if (iconImage == null) iconImage = GetComponent<Image>();
    }

    private void OnEnable()
    {
        EventBus.OnPhaseChanged += HandlePhaseChanged;

        // 初始化：同步一次当前阶段
        if (TimeManager.Instance != null)
            UpdateIcon(TimeManager.Instance.IsDay);
    }

    private void OnDisable()
    {
        EventBus.OnPhaseChanged -= HandlePhaseChanged;
    }

    private void HandlePhaseChanged(DayPhase phase)
    {
        UpdateIcon(phase == DayPhase.Day);
    }

    private void UpdateIcon(bool isDay)
    {
        Sprite target = isDay ? daySprite : nightSprite;
        if (iconImage.sprite == target) return;

        if (switchRoutine != null) StopCoroutine(switchRoutine);
        switchRoutine = StartCoroutine(SwitchRoutine(target));
    }

    private IEnumerator SwitchRoutine(Sprite targetSprite)
    {
        // 1. 放大
        float elapsed = 0f;
        while (elapsed < scaleDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / scaleDuration;
            float scale = Mathf.Lerp(1f, scaleUp, t);
            transform.localScale = new Vector3(scale, scale, 1f);
            yield return null;
        }

        // 2. 旋转一圈 + 中点换图
        elapsed = 0f;
        float startAngle = transform.localEulerAngles.z;
        bool spriteSwapped = false;
        while (elapsed < rotateDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / rotateDuration;
            float angle = Mathf.Lerp(startAngle, startAngle + 360f, t);
            transform.localRotation = Quaternion.Euler(0, 0, angle);

            if (!spriteSwapped && t >= 0.5f)
            {
                iconImage.sprite = targetSprite;
                spriteSwapped = true;
            }

            yield return null;
        }

        // 3. 缩回
        elapsed = 0f;
        while (elapsed < scaleDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / scaleDuration;
            float scale = Mathf.Lerp(scaleUp, 1f, t);
            transform.localScale = new Vector3(scale, scale, 1f);
            yield return null;
        }

        // 收尾
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
        switchRoutine = null;
    }
}