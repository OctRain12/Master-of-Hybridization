using UnityEngine;
using UnityEngine.UI;

public class UI_MainMenu : MonoBehaviour
{
    [Header("主菜单按钮组")]
    public Button continueButton;
    public Button newGameButton;
    public Button settingsButton;
    public Button quitButton;

    [Header("设置面板引用")]
    public GameObject settingsPanel;

    private void Start()
    {
        // 检查是否有存档数据
        bool hasSave = SaveLoadManager.Instance.HasSaveFile();
        Debug.Log($"主菜单检查存档: {hasSave}");
        if (!hasSave)
        {
            continueButton.interactable = false; // 禁用继续按钮
        }
        // continueButton.interactable = false; // 禁用继续按钮

        // 添加按钮点击事件监听器
        continueButton.onClick.AddListener(OnContinueClicked);
        newGameButton.onClick.AddListener(OnNewGameClicked);
        settingsButton.onClick.AddListener(OnSettingsClicked);
        quitButton.onClick.AddListener(OnQuitClicked);
        SoundManager.Instance?.PlayBGM(BGMType.MainMenu);

        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    private void OnContinueClicked()
    {
        SaveLoadManager.Instance.ContinueGame();
    }

    private void OnNewGameClicked()
    {
        // 此处后续可接入加载遮罩动效
        SaveLoadManager.Instance.StartNewGame();
    }

    private void OnSettingsClicked()
    {
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    private void OnQuitClicked()
    {
        Application.Quit();
    }
}
