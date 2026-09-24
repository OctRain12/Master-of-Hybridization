using System.Collections.Generic;
using UnityEngine;

public class EncyclopediaManager : MonoBehaviour
{
    public static EncyclopediaManager Instance { get; private set; }

    // 持久化存储集合（存档时保存这三个列表即可）
    private HashSet<string> unlockedSpeciesIDs = new HashSet<string>();
    private HashSet<string> perfectedSpeciesIDs = new HashSet<string>();
    private HashSet<string> clueUnlockedSpeciesIDs = new HashSet<string>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);   
    }
    /// <summary>
    /// 在作物收获或活体突变成功时调用，注册发现
    /// </summary>
    public void RegisterDiscovery(PlantInstanceData plant)
    {
        if (plant == null || plant.speciesTemplate == null) return;
        SpeciesData species = plant.speciesTemplate;

        // 1. 基础图鉴点亮
        if (unlockedSpeciesIDs.Add(species.speciesName)) // 以speciesName作为唯一标识
        {
            EventBus.TriggerSpeciesDiscovered(species);
        }

        // 2. 极品基因检测 
        if (plant.dna.IsPerfectRecessive() && perfectedSpeciesIDs.Add(species.speciesName))
        {
            EventBus.TriggerSpeciesPerfected(species);
        }
    }

    // 暴露给外部的查询接口
    public bool IsDiscovered(SpeciesData species) => unlockedSpeciesIDs.Contains(species.speciesName);
    public bool IsPerfected(SpeciesData species) => perfectedSpeciesIDs.Contains(species.speciesName);
    public bool IsClueUnlocked(SpeciesData species) => clueUnlockedSpeciesIDs.Contains(species.speciesName);

    /// <summary>
    /// 尝试花费金币解锁高阶线索
    /// </summary>
    public bool TryUnlockClueWithGold(SpeciesData species)
    {
        if (IsClueUnlocked(species) || IsDiscovered(species)) return false;

        if (WalletManager.Instance.TrySpend(species.clueUnlockCost))
        {
            clueUnlockedSpeciesIDs.Add(species.speciesName);
            EventBus.TriggerCluePurchased(species);
            return true;
        }
        return false;
    }

}
