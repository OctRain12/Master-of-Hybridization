using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using Unity.VisualScripting;
public class UI_HotbarSlot : MonoBehaviour, IPointerClickHandler
{
    [Header("配置索引 (0-5)")]
    public int slotIndex;

    [Header("UI 组件引用")]
    public Image iconImage;
    public TextMeshProUGUI amountText;
    public TextMeshProUGUI tagText;

    public bool isEmpty = true;
    private SeedEntry currentEntry;
    private int currentAmount;

    void OnEnable()
    {
        InventoryManager.OnInventoryChanged += RefreshUI;
        //RefreshUI();
    }
     void OnDisable()
    {
        InventoryManager.OnInventoryChanged -= RefreshUI;
    }
    void Start()
    {
        RefreshUI();
    }
    public void RefreshUI()
    {
        // 从 InventoryManager 获取当前槽位数据
        var data = InventoryManager.Instance.hotbarSlots[slotIndex];
        if (data.isEmpty)
        {
            isEmpty = true;
            iconImage.color = Color.clear;
            amountText.text = "";
            tagText.text = "";
        }
        else
        {
            isEmpty = false;
            currentEntry = data.seedEntry.Value;
            currentAmount = data.amount;

            iconImage.sprite = currentEntry.species.speciesSeedIcon;
            iconImage.color = Color.white;
            amountText.text = currentAmount.ToString();
            // 获取并设置玩家标记
            string tag = InventoryManager.Instance.GetSeedTag(currentEntry);
            tagText.text = tag;
        }
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        // 场景 A：打开背包时，点击快捷栏槽位
        if (UI_Inventory.Instance != null && UI_Inventory.Instance.IsOpen)
        {
            HandleBackpackModeClick();
        }
        // 场景 B：关闭背包时，点击底部快捷栏槽位 -> 进入虚拟拾取播种模式
        else
        {
            if (isEmpty) 
            {
                return;
            }
            Debug.Log($"[快捷栏] 点击槽位 {slotIndex}，尝试进入虚拟播种模式");
            CursorManager.Instance.ActivateHotbarPlantMode(slotIndex, currentEntry, currentAmount);
        }
    }
    // 当背包打开时，快捷栏允许玩家把手里抓着的种子“落户”到快捷栏里
    private void HandleBackpackModeClick()
    {
        
        if (CursorManager.Instance.cursorItemType == CursorItemType.Seed && CursorManager.Instance.heldSeed.species != null)
        {
            SeedEntry held = CursorManager.Instance.heldSeed;
            int count = CursorManager.Instance.heldAmount;
            // 情况 1：目标格子是空的 -> 直接放下
            if (isEmpty)
            {
                // 把手里抓着的种子放到这个快捷栏坑位
                InventoryManager.Instance.SetHotbarSlot(slotIndex, held, count);
                CursorManager.Instance.DropItem(); // 清空鼠标
            }
            // 情况 2：目标格子有东西 -> 判断基因和物种是否完全一样 
            else if (currentEntry.Equals(held))
            {
                // 完全一样 -> 完美叠加
                int newCount = currentAmount + count;
                InventoryManager.Instance.SetHotbarSlot(slotIndex, held, newCount);
                CursorManager.Instance.DropItem();
            }
            // 情况 3：目标格子有东西，且基因或物种不一样 -> 经典位置交换 (Swap)
            else
            {
                // 暂存格子里的旧数据
                SeedEntry oldSeedInSlot = currentEntry;
                int oldAmountInSlot = currentAmount;

                // 第一步：把格子里的旧东西先强行“蒸发”（先从数据层删掉）
                InventoryManager.Instance.SetHotbarSlot(slotIndex, null, 0);

                // 第二步：把手里拿着的新东西放入数据仓库
                InventoryManager.Instance.SetHotbarSlot(slotIndex, held, count);

                // 第三步：让鼠标把格子里的旧东西“抓”起来，完成交换
                CursorManager.Instance.PickUp(oldSeedInSlot, oldAmountInSlot);
                
            }

        }
        else if (!isEmpty && CursorManager.Instance.cursorItemType == CursorItemType.None)
        {
            // 如果手是空的，点击快捷栏则把里面的种子“拔出来”放回鼠标上，清空该快捷栏
            CursorManager.Instance.PickUp(currentEntry, currentAmount);
            InventoryManager.Instance.SetHotbarSlot(slotIndex, null, 0);
        }
    }
}
