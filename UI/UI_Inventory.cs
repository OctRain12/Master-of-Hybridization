using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Dynamic;

public class UI_Inventory : MonoBehaviour
{
    public Transform contentTransform;  //存放格子的父节点


    [Header("当前激活分类")]
    public ItemCategory currentCategory = ItemCategory.Seed; // 默认显示种子页

    private UI_InventorySlot[] allSlots;


    void Awake()
    {
        //启动时获取父节点下的所有格子
         allSlots = contentTransform.GetComponentsInChildren<UI_InventorySlot>(true);
    }

    void OnEnable()
    {
        
        InventoryManager.OnInventoryChanged += RedRawUI;
        RedRawUI();
    }
    void OnDisable()
    {
        InventoryManager.OnInventoryChanged -= RedRawUI;
    }
    void Start()
    {

    }
    /// <summary>
    /// 提供给 UI 按钮调用的方法：切换背包标签页
    /// </summary>
    public void SwitchCategory(int categoryIndex)
    {
        currentCategory = (ItemCategory)categoryIndex;
        CursorManager.Instance.ReturnHeldItemToInventory(); // 切换分类前先把鼠标上的物品放回背包，避免切换后物品不见了
        RedRawUI(); // 切换分类后，立刻重新绘制格子内容
    }
    public void RedRawUI()
    {
        if(currentCategory == ItemCategory.Seed)
        {
            DrawSeedPage();
        }
        if(currentCategory == ItemCategory.Fruit)
        {
            DrawFruitPage();
        }
    }
    // --- 渲染种子页 ---
    private void DrawSeedPage()
    {
        if (InventoryManager.Instance == null) return;
        //将种子字典转换为列表方便索引
        var seedList = InventoryManager.Instance.seedInventory.ToList();

        //把所有有效数据种子装入格子中，比如20个格子，3种种子，则只计算三个格子，后续格子清空
        for(int i = 0; i < allSlots.Length; i++)
        {
            if(i < seedList.Count)
            {
                //把有效数据提取出来
                var pair = seedList[i];
                //确认下玩家所打的标签
                InventoryManager.Instance.seedTags.TryGetValue(pair.Key,out string tag);
                //更新格子的信息
                allSlots[i].Refresh(pair.Key, pair.Value,tag);
            }
            else
            {
                //空闲格子，并没有这么多实际数据，清空
                allSlots[i].ClearSlot();
            }
        }

    }
    // --- 渲染果实页 ---
    private void DrawFruitPage()
    {
        // 果实字典是 Dictionary<SpeciesData, int>
        if (InventoryManager.Instance == null) return;
        
        var fruitList = InventoryManager.Instance.fruitInventory.ToList();
        //同上装入所有数据，且清空多余格子
        for(int i = 0; i < allSlots.Length; i++)
        {
            if(i < fruitList.Count)
            {
                var pair = fruitList[i];
                // 填充果实数据 (果实不需要 DNA 字符串，也不需要玩家标签)
                allSlots[i].Refresh(pair.Key,pair.Value);
            }
            else
            {
                allSlots[i].ClearSlot();
            }
        }
    }
    public void RedrawUI()
    {
        if (InventoryManager.Instance == null) return;
        //将种子字典转换为列表方便索引
        var seedList = InventoryManager.Instance.seedInventory.ToList();
        
        //把所有有效数据种子装入格子中，比如20个格子，3种种子，则只计算三个格子，后续格子清空
        for(int i = 0; i < allSlots.Length; i++)
        {
            if(i < seedList.Count)
            {
                //把有效数据提取出来
                var pair = seedList[i];
                //确认下玩家所打的标签
                InventoryManager.Instance.seedTags.TryGetValue(pair.Key,out string tag);
                //更新格子的信息
                allSlots[i].Refresh(pair.Key,pair.Value, tag);
            }
            else
            {
                //空闲格子，并没有这么多实际数据，清空
                allSlots[i].ClearSlot();
            }
        }
    }
}
