using System.Collections.Generic;
using System;

[Serializable]
public class GameSaveData
{
    // 1. 存档元数据
    public string saveTimestamp;

    // 2. 经济与时间
    public int playerGold;
    public int currentDay;
    public int currentHour;
    public float hourProgress; // 0.0f ~ 1.0f 当前小时走完的比例

    // 3. 背包精简数据
    public List<SavedSeedEntry> seedInventory = new List<SavedSeedEntry>();
    public List<SavedFruitEntry> fruitInventory = new List<SavedFruitEntry>();

    // 4. 地块网格状态（核心：只存坐标、状态、物种、DNA和生长比例）
    public List<SavedPlotEntry> plotEntries = new List<SavedPlotEntry>();

    // 5. 图鉴状态
    public List<string> unlockedSpeciesNames = new List<string>();
    public List<string> perfectedSpeciesNames = new List<string>();
    public List<string> clueUnlockedSpeciesNames = new List<string>();

    [Serializable]
    public struct SavedPlotEntry
    {
        public int gridX;
        public int gridY;
        public int tileState;          // TileState 枚举强转为 int
        public bool hasPlant;
        public string speciesName;     // 作物名称（用于重新索取对应的 SpeciesData）
        public string geneString;      // 基因，如 "AaBbCc"
        public string calculatedSeedEntry;
        public float currentGrowthTimer;
        // public float totalGrowthDuration;
        public float speedBuffMultiplier;
    }
    [Serializable]
    public struct SavedSeedEntry
    {
        public string speciesName;
        public string geneString;      // "AaBbCc"
        public int count;
    }
    [Serializable]
    public struct SavedFruitEntry
    {
        public string speciesName;
        public int count;
    }
    // TODO: 图鉴数据
    public struct SavedSpeciesEntry
    {
        public string speciesName;
        public bool isDiscovered;
        public bool isPerfected;
        public bool isClueUnlocked;
    }
}
