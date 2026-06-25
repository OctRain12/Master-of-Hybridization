using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System;

public enum ItemCategory
{
    Seed,   // 种子页
    Fruit   // 果实页
}

public class InventoryManager : MonoBehaviour
{
    [Header("经济系统")]
    public int currentGold = 100; // 初始金币
    [Header("快捷栏数据 (固定6个槽位)")]
    public int hotbarSize = 6;
    public List<HotbarSlotData> hotbarSlots = new List<HotbarSlotData>();

    // 仓库作为唯一单例,方便被调用
    public static InventoryManager Instance;
    //两个字典对应两个仓库，果实和种子
    //1.果实仓库，每次收割传入作物id与数量count
    public Dictionary<SpeciesData, int> fruitInventory = new Dictionary<SpeciesData, int>();
    //2.种子仓库，每次收割传入种子id与基因对比和数量count，基因对比一致才叠加到一起
    //为了方便判断是否基因一致，引入新结构体SeedEntry
    public Dictionary<SeedEntry, int> seedInventory = new Dictionary<SeedEntry, int>();

    //3.元数据仓库 (保存玩家的标记)
    // Key 是种子条目，Value 是玩家输入的字符串 (如 "aa", "速度型")
    public Dictionary<SeedEntry, string> seedTags = new Dictionary<SeedEntry, string>();

    //刷新背包的事件
    public static event Action OnInventoryChanged;

    //初始化赋值
    void Awake()
    {
        Instance = this;
        Debug.Log("[仓库] InventoryManager 已初始化。");
    }

    void Start()
    {
        InitializeHotbar();
        Debug.Log("[仓库] 快捷栏已初始化，槽位数量：" + hotbarSize);
    }

    //---收获果实---
    public void AddFruit(SpeciesData species, int count)
    {
        //先查找一下有没有同类作物在仓库
        if(fruitInventory.ContainsKey(species)) fruitInventory[species] += count;
        else fruitInventory[species] = count;
        //触发事件广播
        OnInventoryChanged?.Invoke();
        Debug.Log($"[仓库] 收获果实：{species.speciesName} x{count}。当前总量：{fruitInventory[species]}");
    }

    //---收获种子---
    public void AddSeed(SpeciesData species, GenoType dna, int count)
    {
        //同理先查找是否有同类同基因种子
        SeedEntry entry = new SeedEntry(species, dna);
        if(seedInventory.ContainsKey(entry)) seedInventory[entry] += count;
        else seedInventory[entry] = count;
        //触发事件广播
        OnInventoryChanged?.Invoke();
        Debug.Log($"[仓库] 存入种子：{species.speciesName}，基因：{dna}。当前堆叠：{seedInventory[entry]}");
    }

    //辅助判断结构seedEntry
    //重写了比较方法以使字典正确判断（不再根据地址判断，防止无法堆叠）

    //玩家标记基因型
    public void SetSeedTag(SeedEntry entry, string tag)
    {
        seedTags[entry] = tag;
        OnInventoryChanged?.Invoke(); // 标记变了也要刷新 UI
    }
    /// <summary>
    /// 从背包中扣除指定数量的种子
    /// </summary>
    public void Remove(SeedEntry entry, int count)
    {
        if(seedInventory.ContainsKey(entry))
        {
            seedInventory[entry] -= count;
            if(seedInventory[entry] <= 0)
            {
                seedInventory.Remove(entry);
                seedTags.Remove(entry); // 同时移除标记
            }
            OnInventoryChanged?.Invoke();// 广播：数据变了，UI 会自动重绘
            Debug.Log($"[仓库] 扣除种子：{entry.species.speciesName}，基因：{entry.dna}，数量：{count}。剩余：{seedInventory.GetValueOrDefault(entry, 0)}");
        }
    }
    /// <summary>
    /// 重载Remove方法从背包中扣除指定数量的果实
    /// </summary>
    public void Remove(SpeciesData species, int count)
    {
        if(fruitInventory.ContainsKey(species))
        {
            fruitInventory[species] -= count;
            if(fruitInventory[species] <= 0)
            {
                fruitInventory.Remove(species);
            }
            OnInventoryChanged?.Invoke();// 广播：数据变了，UI 会自动重绘
            Debug.Log($"[仓库] 扣除果实：{species.speciesName}，数量：{count}。剩余：{fruitInventory.GetValueOrDefault(species, 0)}");
        }
    }
    /// <summary>
    /// 获取指定种子的玩家标记（安全查询）
    /// </summary>
    public string GetSeedTag(SeedEntry entry)
    {
        if (seedTags.TryGetValue(entry, out string tag))
        {
            return tag;
        }
        return "";
    }
    // 购买种子或卖出果实时调用
    public bool ModifyGold(int amount)
    {
        if(currentGold + amount < 0)
        {
            Debug.LogWarning("[仓库] 金币不足，无法完成交易。");
            return false; // 金币不足，交易失败
        }
        currentGold += amount;
        OnInventoryChanged?.Invoke(); // 金币变了也属于背包/仓库数据变动，触发UI刷新
        return true; // 交易成功
    }
    /// <summary>
    /// 检查并扣除指定数量的果实（用于完成订单）
    /// </summary>
    public bool RemoveFruit(SpeciesData species, int count)
    {
        if (fruitInventory.ContainsKey(species) && fruitInventory[species] >= count)
        {
            fruitInventory[species] -= count;
            if (fruitInventory[species] <= 0)
            {
                fruitInventory.Remove(species);
            }
            OnInventoryChanged?.Invoke();
            Debug.Log($"[仓库] 扣除果实：{species.speciesName}，数量：{count}。剩余：{fruitInventory.GetValueOrDefault(species, 0)}");
            return true;
        }
        Debug.LogWarning("[仓库] 果实数量不足，无法完成交易。");
        return false;
    }
    [System.Serializable]
    public class HotbarSlotData
    {
        public bool isEmpty = true;
        public SeedEntry? seedEntry;
        public int amount;
        // 清空快捷栏槽位
        public void Clear()
        {
            isEmpty = true;
            seedEntry = null;
            amount = 0;
        }
        // 给快捷栏槽位赋值
        public void Assign(SeedEntry entry, int count)
        {
            isEmpty = false;
            seedEntry = entry;
            amount = count;
        }
    }
    // 在 Awake 或 Init 中初始化快捷栏
    private void InitializeHotbar()
    {
        hotbarSlots.Clear();
        for (int i = 0; i < hotbarSize; i++)
        {
            hotbarSlots.Add(new HotbarSlotData());
        }
    }
    /// <summary>
    /// 设置快捷栏某个槽位的数据（由 UI 拖拽交互或点击触发）
    /// </summary>
    public void SetHotbarSlot(int index, SeedEntry? entry, int count)
    {
        if (index < 0 || index >= hotbarSize) return;

        if (count <= 0)
        {
            hotbarSlots[index].Clear();
        }
        else
        {
            hotbarSlots[index].Assign(entry.Value, count);
        }
        OnInventoryChanged?.Invoke(); // 广播刷新：主背包、顶部映射栏、底部常驻栏会同时重绘
    }
    /// <summary>
    /// 快捷栏种子消耗（种植时调用）
    /// </summary>
    public void ConsumeHotbarSeed(int index, int count = 1)
    {
        if (index < 0 || index >= hotbarSize || hotbarSlots[index].isEmpty) return;

        hotbarSlots[index].amount -= count;
        if (hotbarSlots[index].amount <= 0)
        {
            hotbarSlots[index].Clear();
        }
        OnInventoryChanged?.Invoke();
    }
}
public struct SeedEntry
    {
        //该结构体应该包含种子的属性：物种，dna
        public SpeciesData species;
        public GenoType dna;
        public SeedEntry(SpeciesData s, GenoType d)
        {
            species = s;
            dna = d;
        }
        //重写比较方法
        public override bool Equals(object obj)
        {
            if(!(obj is SeedEntry)) return false;
            SeedEntry other = (SeedEntry)obj;
            return species == other.species && dna.Equals(other.dna);
        }
        public override int GetHashCode()
        {
            // 结合物种Hash和基因Hash
            return (species.GetHashCode() * 31) + dna.GetHashCode();
        }
    }

