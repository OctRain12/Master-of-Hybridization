using UnityEngine;

public class GameSceneBootstrapper : MonoBehaviour
{
    private void Start()
    {
        if (SaveLoadManager.Instance != null && SaveLoadManager.Instance.isPendingLoadFromSave)
        {
            // --- 走读档恢复分支 ---
            ApplyLoadedGameData(SaveLoadManager.Instance.cachedSaveData);
        }
        else
        {
            // --- 走新游戏默认分支 ---
            InitDefaultNewGame();
        }
    }

    private void ApplyLoadedGameData(GameSaveData data)
    {
        if (data == null) return;

        // 1. 还原金币
        WalletManager.Instance?.LoadSaveData(data.playerGold);

        // 2. 还原时间
        TimeManager.Instance?.LoadSaveData(data.currentDay, data.currentHour, data.hourProgress);

        // 3. 还原图鉴
        EncyclopediaManager.Instance?.LoadSaveData(data.unlockedSpeciesNames, data.perfectedSpeciesNames, data.clueUnlockedSpeciesNames);

        // 4. 还原背包（种子与果实）
        InventoryManager.Instance?.LoadSaveData(data.seedInventory, data.fruitInventory);

        // 5. 还原地块状态与生长比例
        PlotGridManager.Instance?.LoadSaveData(data.plotEntries);



        Debug.Log($"[存档系统] 读档成功，时间戳: {data.saveTimestamp}");
    }

    private void InitDefaultNewGame()
    {
        // 给初始启动资金、基础小麦种子等
        Debug.Log("[游戏系统] 新游戏初始化完成");
    }
}