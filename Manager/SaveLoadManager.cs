using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class SaveLoadManager : MonoBehaviour
{
    public static SaveLoadManager Instance { get; private set; }
    private string saveFilePath;
    public bool isPendingLoadFromSave { get; private set; } // 全局加载标记
    public GameSaveData cachedSaveData { get; private set; }

    [Header("数据库")]
    public SpeciesDatabase allSpeciesDatabase; // 全物种数据库（存放所有作物SO）
    private Dictionary<string, SpeciesData> speciesLookup; // 根据 speciesName 快速查找 SpeciesData 的字典

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            saveFilePath = Path.Combine(Application.persistentDataPath, "savegame.json");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 是否存在本地存档（供主菜单判断继续按钮是否高亮）
    /// </summary>
    public bool HasSaveFile() => File.Exists(saveFilePath);

    /// <summary>
    /// 主菜单点击【开始新游戏】
    /// </summary>
    public void StartNewGame()
    {
        isPendingLoadFromSave = false;
        cachedSaveData = null;
        SceneManager.LoadScene("GameIn");
    }

    /// <summary>
    /// 主菜单点击【继续游戏】
    /// </summary>
    public void ContinueGame()
    {
        if (!HasSaveFile()) return;

        string json = File.ReadAllText(saveFilePath);
        cachedSaveData = JsonUtility.FromJson<GameSaveData>(json);
        isPendingLoadFromSave = true;

        SceneManager.LoadScene("GameIn");
    }
    /// <summary>
    /// 字典查询
    /// </summary>
    private void InitSpeciesLookup()
    {
        speciesLookup = new Dictionary<string, SpeciesData>();
        foreach (var sp in allSpeciesDatabase.allSpecies)
        {
            if (sp != null && !speciesLookup.ContainsKey(sp.speciesName))
            {
                speciesLookup.Add(sp.speciesName, sp);
            }
        }
    }
    /// <summary>
    /// 根据作物名称获取 SpeciesData
    /// </summary>
    /// <param name="name">speciesName</param>
    /// <returns></returns>
    public SpeciesData GetSpeciesByName(string name)
    {
        if (speciesLookup == null) InitSpeciesLookup();
        speciesLookup.TryGetValue(name, out var data);
        return data;
    }

    /// <summary>
    /// 游戏中执行保存并写入本地
    /// </summary>
    public void SaveGame()
    {
        GameSaveData data = new GameSaveData();
        data.saveTimestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        // 1. 采集经济
        data.playerGold = WalletManager.Instance != null ? WalletManager.Instance.GetGold() : 0;
        // 2. 采集时间
        if (TimeManager.Instance != null)
        {
            data.currentDay = TimeManager.Instance.currentDay;
            data.currentHour = TimeManager.Instance.currentHour;
            // data.hourProgress = TimeManager.Instance.GetHourProgress(); // 返回 0~1 的当前小时进度
        }
        // 3. 采集地块（读取各个土坑的比例）
        if (PlotGridManager.Instance != null)
        {
            foreach (var kvp in PlotGridManager.Instance.plotDictionary)
            {
                var slot = kvp.Value;
                GameSaveData.SavedPlotEntry entry = new GameSaveData.SavedPlotEntry
                {
                    gridX = slot.gridPos.x,
                    gridY = slot.gridPos.y,
                    tileState = (int)slot.currentState,
                    hasPlant = slot.currentPlantData != null
                };

                if (slot.currentPlantData != null)
                {
                    entry.speciesName = slot.currentPlantData.speciesTemplate.speciesName;
                    entry.geneString = slot.currentPlantData.dna.GetGeneString(); // 获取标准 "AaBbCc"
                    // 存入归一化比例 (0.0 ~ 1.0)
                    entry.currentGrowthTimer = slot.currentGrowthTimer;
                    entry.speedBuffMultiplier = slot.speedBuffMultiplier; 
                }

                data.plotEntries.Add(entry);
            }
        }
        // 4. 采集背包（种子和果实）
        if (InventoryManager.Instance != null)
        {
            foreach (var kvp in InventoryManager.Instance.seedInventory)
            {
                var seed = kvp.Key;
                GameSaveData.SavedSeedEntry seedEntry = new GameSaveData.SavedSeedEntry
                {
                    speciesName = seed.species.speciesName,
                    geneString = seed.dna.GetGeneString(),
                    count = kvp.Value
                };
                data.seedInventory.Add(seedEntry);
            }

            foreach (var kvp in InventoryManager.Instance.fruitInventory)
            {
                var fruit = kvp.Key;
                GameSaveData.SavedFruitEntry fruitEntry = new GameSaveData.SavedFruitEntry
                {
                    speciesName = fruit.speciesName,
                    count = kvp.Value
                };
                data.fruitInventory.Add(fruitEntry);
            }
        }
        // 5. 采集图鉴状态
        if(EncyclopediaManager.Instance != null)
        {
            foreach (var speciesName in EncyclopediaManager.Instance.GetUnlockedSpeciesIDs())
            {
                data.unlockedSpeciesNames.Add(speciesName);
            }
            foreach (var speciesName in EncyclopediaManager.Instance.GetPerfectedSpeciesIDs())
            {
                data.perfectedSpeciesNames.Add(speciesName);
            }
            foreach (var speciesName in EncyclopediaManager.Instance.GetClueUnlockedSpeciesIDs())
            {
                data.clueUnlockedSpeciesNames.Add(speciesName);
            }
        }
        // 写入本地文件
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(saveFilePath, json);
        Debug.Log($"[存档系统] 存档保存成功: {saveFilePath}");
    }

}
