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
        // 订阅金币变化
        WalletManager.OnGoldChanged += UpdateGoldUI;
        // InventoryManager.OnInventoryChanged += UpdateGoldUI;
        
        // 初始刷新一次（时间管理器可能已就绪）
        if(TimeManager.Instance != null) UpdateTimeUI(TimeManager.Instance.currentDay, TimeManager.Instance.currentHour);
    }
    void Start()
    {
        // 在 Start 中做初始刷新，保证 WalletManager.Awake() 已执行，Instance 可用
        UpdateGoldUI(WalletManager.Instance.GetGold(), 0);
    }
    void OnDisable()
    {
        EventBus.OnHourChanged -= UpdateTimeUI;
        WalletManager.OnGoldChanged -= UpdateGoldUI;
    }

    private void UpdateTimeUI(int day, int hour)
    {
        // 格式化显示，例如: "Day 3 | 08:00"
        timeText.text = $"第 {day} 天 | {hour:D2}:00";
    }
    private void UpdateGoldUI(int currentGold, int change)
    {
        Debug.Log($"金币变动: 当前总额 = {currentGold}, 变动差值 = {change}");
        goldText.text = $"金币: {currentGold}";
    }
}
