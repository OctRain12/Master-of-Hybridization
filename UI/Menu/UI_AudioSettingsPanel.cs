using UnityEngine;
using UnityEngine.UI;

public class UI_AudioSettingsPanel : MonoBehaviour
{
    [Header("音量滑块引用")]
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("返回/关闭按钮 (可选)")]
    [SerializeField] private Button closeButton;
    private void OnEnable()
    {
        // 面板打开时，从 SoundManager 读取最新值赋给 Slider
        if (SoundManager.Instance != null)
        {
            bgmSlider.value = SoundManager.Instance.GetBGMVolume();
            sfxSlider.value = SoundManager.Instance.GetSFXVolume();
        }

        // 绑定监听
        bgmSlider.onValueChanged.AddListener(OnBGMValueChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXValueChanged);

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(OnCloseClicked);
        }
    }
    private void OnDisable()
    {
        // 关闭时解除监听
        bgmSlider.onValueChanged.RemoveListener(OnBGMValueChanged);
        sfxSlider.onValueChanged.RemoveListener(OnSFXValueChanged);

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(OnCloseClicked);
        }
    }
    private void OnBGMValueChanged(float value)
    {
        SoundManager.Instance?.SetBGMVolume(value);
    }

    private void OnSFXValueChanged(float value)
    {
        SoundManager.Instance?.SetSFXVolume(value);
    }

    private void OnCloseClicked()
    {
        // 播放关闭音效，并隐藏自身
        SoundManager.Instance?.PlaySFX(SFXType.UICancel);
        gameObject.SetActive(false);
    }
}
