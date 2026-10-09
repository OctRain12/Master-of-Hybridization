using UnityEngine;
using UnityEngine.UI;

public class UI_AutoAudioBinder : MonoBehaviour
{
    private void Awake()
    {
        // 自动找出当前面板下所有的按钮（包含隐藏状态的）
        Button[] allButtons = GetComponentsInChildren<Button>(true);

        foreach (var btn in allButtons)
        {
            // 根据按钮名称智能分类（例如名字里包含 Close、Back、Cancel 的走取消音效）
            string btnName = btn.gameObject.name.ToLower();
            if (btnName.Contains("close") || btnName.Contains("cancel"))
            {
                btn.onClick.AddListener(() => SoundManager.Instance?.PlaySFX(SFXType.UICancel));
            }
            else if (btnName.Contains("book"))
            {
                btn.onClick.AddListener(() => SoundManager.Instance?.PlaySFX(SFXType.OpenBook));
            }
        }
    }
}