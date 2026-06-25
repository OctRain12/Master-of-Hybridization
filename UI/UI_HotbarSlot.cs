using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
public class UI_HotbarSlot : MonoBehaviour, IPointerClickHandler
{
    [Header("配置索引 (0-5)")]
    public int slotIndex;

    [Header("UI 组件引用")]
    public GameObject inventoryUI;
    public Image iconImage;
    public TextMeshProUGUI amountText;

    private bool isEmpty = true;
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
        }
        else
        {
            isEmpty = false;
            currentEntry = data.seedEntry.Value;
            currentAmount = data.amount;

            iconImage.sprite = currentEntry.species.speciesSeedIcon;
            iconImage.color = Color.white;
            amountText.text = currentAmount.ToString();
        }
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        // 场景 A：打开背包时，点击快捷栏槽位
        if (inventoryUI != null && inventoryUI.activeSelf)
        {
            HandleBackpackModeClick();
        }
        // 场景 B：关闭背包时，点击底部快捷栏槽位 -> 进入虚拟拾取播种模式
        else
        {
            if (isEmpty) return;
            CursorManager.Instance.ActivateHotbarPlantMode(slotIndex, currentEntry, currentAmount);
        }
    }
    // 当背包打开时，快捷栏允许玩家把手里抓着的种子“落户”到快捷栏里
    private void HandleBackpackModeClick()
    {
        
        if (CursorManager.Instance.cursorItemType == CursorItemType.Seed && CursorManager.Instance.heldSeed.species != null)
        {
            // 把手里抓着的种子放到这个快捷栏坑位
            SeedEntry held = CursorManager.Instance.heldSeed;
            int count = CursorManager.Instance.heldAmount;

            InventoryManager.Instance.SetHotbarSlot(slotIndex, held, count);
            CursorManager.Instance.DropItem(); // 清空鼠标
        }
        else if (!isEmpty && CursorManager.Instance.cursorItemType == CursorItemType.None)
        {
            // 如果手是空的，点击快捷栏则把里面的种子“拔出来”放回鼠标上，清空该快捷栏
            CursorManager.Instance.PickUp(currentEntry, currentAmount);
            InventoryManager.Instance.SetHotbarSlot(slotIndex, null, 0);
        }
    }
}
