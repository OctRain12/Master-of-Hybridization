using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UI_StatusHUD : MonoBehaviour
{
    [Header("UI 文本引用")]
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI goldText;

    void OnEnable()
    {
        // 订阅时间变化
        EventBus.OnHourChanged += UpdateTimeUI;
        // 订阅仓库/金币变化
        InventoryManager.OnInventoryChanged += UpdateGoldUI;
        
        // 初始刷新一次（时间管理器可能已就绪）
        if(TimeManager.Instance != null) UpdateTimeUI(TimeManager.Instance.currentDay, TimeManager.Instance.currentHour);
    }
    void Start()
    {
        // 在 Start 中做初始刷新，保证 InventoryManager.Awake() 已执行，Instance 可用
        UpdateGoldUI();
    }
    void OnDisable()
    {
        EventBus.OnHourChanged -= UpdateTimeUI;
        InventoryManager.OnInventoryChanged -= UpdateGoldUI;
    }

    private void UpdateTimeUI(int day, int hour)
    {
        // 格式化显示，例如: "Day 3 | 08:00"
        timeText.text = $"第 {day} 天 | {hour:D2}:00";
    }
    private void UpdateGoldUI()
    {
        if (InventoryManager.Instance == null) return;
        goldText.text = $"金币: {InventoryManager.Instance.currentGold}";
    }
}
