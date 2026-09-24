using System;
using UnityEngine;

public class WalletManager : MonoBehaviour
{
    public static WalletManager Instance { get; private set; }
    [SerializeField] private int currentGold = 10; // 初始金币数量
    // 事件：金币变动 (当前总额, 变动差值)
    public static event Action<int, int> OnGoldChanged;
    // 事件：余额不足警报
    public static event Action OnInsufficientFunds;
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    public int GetGold() => currentGold;
    public bool HasEnough(int amount) => currentGold >= amount;

    public bool TrySpend(int amount)
    {
        if (amount < 0) return false;
        
        if (currentGold >= amount)
        {
            currentGold -= amount;
            OnGoldChanged?.Invoke(currentGold, -amount);
            return true;
        }
        else
        {
            // 触发余额不足的全域警告
            OnInsufficientFunds?.Invoke();
            return false;
        }
    }
    public void AddGold(int amount)
    {
        if (amount <= 0) return;
        currentGold += amount;
        OnGoldChanged?.Invoke(currentGold, amount);
    }
}
