using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UI_InGamePauseMenu : MonoBehaviour
{
    [Header("UI 容器与组件")]
    public GameObject pausePanel;
    public Button resumeButton;
    public Button settingsButton;
    public Button saveAndQuitButton;
    public GameObject settingsPanel;

    private bool isPaused = false;

    private void Start()
    {
        pausePanel.SetActive(false);

        resumeButton.onClick.AddListener(ResumeGame);
        settingsButton.onClick.AddListener(OpenSettings);
        saveAndQuitButton.onClick.AddListener(SaveAndQuitToMainMenu);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }
    public void PauseGame()
    {
        isPaused = true;
        pausePanel.SetActive(true);
        Time.timeScale = 0f; // 核心暂停机制
        Debug.Log("游戏已暂停");
    }
    public void ResumeGame()
    {
        isPaused = false;
        pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        Time.timeScale = 1f; // 恢复时间流逝
    }
    private void OpenSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }
    private void SaveAndQuitToMainMenu()
    {
        // 1. 保存当前进度
        SaveLoadManager.Instance.SaveGame();

        // 2. 切场景前必须恢复时间流速！否则新场景会被冻结
        Time.timeScale = 1f;

        // 3. 返回主菜单场景
        SceneManager.LoadScene("MainMenu");
    }
}
