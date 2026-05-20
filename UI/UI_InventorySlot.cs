using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.Android;

public class UI_InventorySlot : MonoBehaviour, IPointerClickHandler
{
    [Header("UI 引用")]
    public Image iconImage;
    public TextMeshProUGUI amountText;
    public TextMeshProUGUI tagText;
    public GameObject goldHighlight;    // 金色边框物体
    [Header("槽位当前数据")]
    public ItemCategory slotCategory; // 记录这个格子当前装的是种子还是果实
    public bool isEmpty;                //用于格子是否为空判断
    private SeedEntry? currentEntry;    // 使用可空类型或判定默认值
    private int currentAmount;          //当前数量
    private SpeciesData currentFruitData; //当前果实
    public void Refresh(SeedEntry entry, int amount, string tag)
    {
        isEmpty = false;
        slotCategory = ItemCategory.Seed;
        currentEntry = entry;
        currentAmount = amount;
        // 1. 基础信息显示
        iconImage.sprite = entry.species.speciesSeedIcon; // 假设 SpeciesData 里有图标
        iconImage.color = Color.white;                  // 确保图片可见
        amountText.text = amount.ToString();

        // 2. 显示玩家标记
        tagText.text = string.IsNullOrEmpty(tag)?"":tag;

        // 3. 核心逻辑：自动高亮逻辑 (根据玩家标记)
        // 如果玩家标记里包含 "aa" 或 "bb" (隐性纯合)，且玩家设置了高亮
        UpdateHighlight(tag);
    }
    //重载Refresh,适配果实更新
    public void Refresh(SpeciesData speciesData, int amount)
    {
        isEmpty = false;
        slotCategory = ItemCategory.Fruit;
        currentAmount = amount;
        currentFruitData = speciesData;
        iconImage.sprite = speciesData.speciesFruitIcon;
        iconImage.color = Color.white;
        amountText.text = amount.ToString();
        // 基础信息显示
        
        // 果实没有玩家标记和金标高亮
        tagText.text = "";
        goldHighlight.SetActive(false);
    }

    //清空格子方法
    public void ClearSlot()
    {
        isEmpty = true;
        currentEntry = null;
        //currentAmount = null;
        iconImage.sprite = null;
        iconImage.color = Color.clear; // 透明
        amountText.text = "";
        tagText.text = "";
        goldHighlight.SetActive(false);
    }
    // --- 悬浮窗自适应 ---
    public void OnPointerEnter(PointerEventData eventData)
    {
        // 只有当格子不空时或者鼠标没有东西才显示悬浮窗
        if(isEmpty|| CursorManager. Instance.cursorItemType != CursorItemType.None) return; // 空格子不显示悬浮窗
        if(slotCategory == ItemCategory.Seed)
        {
            // 显示种子悬浮窗，传入 currentEntry.Value
            UI_Tooltip.Instance.Show(currentEntry.Value);
        }
        else if(slotCategory == ItemCategory.Fruit)
        {
            // 显示果实悬浮窗，传入 currentFruitData
            UI_Tooltip.Instance.Show(currentFruitData);
        }
    }
    // 鼠标离开时隐藏悬浮窗
    public void OnPointerExit(PointerEventData eventData)
    {
        if (UI_Tooltip.Instance != null)
        {
            UI_Tooltip.Instance.Hide();
        }
    }
    //鼠标点击格子
    public void OnPointerClick(PointerEventData eventData)
    {
        // 1. Shift + 左键：打开标签输入框
        if(Input.GetKey(KeyCode.LeftShift))
        {
            if(eventData.button == PointerEventData.InputButton.Left && !isEmpty)
            {
                //TODO：
                // 打开标签页面
                OpenTagInputWindow();
            }
            return;
        }
        // 2. 左键单击：拾取 / 放下
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            HandleLeftClick();
        }
        //3. 右键单击：拆分
        else if(eventData.button == PointerEventData.InputButton.Right)
        {
            HandleRightClick();
        }
        if(currentEntry == null) return; //避免点击空格子生效
        Debug.Log($"准备种植：{currentEntry.Value.species.speciesName}");
        // 这里后续接入：CursorManager.Instance.SetHoldingSeed(currentEntry);
    }
    private void UpdateHighlight(string tag)
    {
        // 定制金标条件。
        // 比如：只要玩家标记了 "aa"，就显示金标
        if(!string.IsNullOrEmpty(tag) && tag.Contains("aa"))
        {
            goldHighlight.SetActive(true);
        }
        else
        {
            goldHighlight.SetActive(false);
        }
    }

    private void HandleLeftClick()
    {
        //如果鼠标是空的，且格子里面有物品的时候，抓取全部物品
        if(CursorManager.Instance.cursorItemType == CursorItemType.None)
        {
            //如果是种子格子，且不空
            if(!isEmpty && slotCategory == ItemCategory.Seed)
            {
                CursorManager.Instance.PickUp(currentEntry.Value, currentAmount);
                // 告诉数据层扣除数量 (或者直接清空)
                InventoryManager.Instance.Remove(currentEntry.Value, currentAmount);
                // 这里需要调用 InventoryManager 的方法去修改数据
                Debug.Log("拿起了全部物品");
            }
            //如果是果实格子，且不空
            else if(!isEmpty && slotCategory == ItemCategory.Fruit)
            {
                CursorManager.Instance.PickUp(currentFruitData, currentAmount);
                // 告诉数据层扣除数量 (或者直接清空)
                InventoryManager.Instance.Remove(currentFruitData, currentAmount);
                // 这里需要调用 InventoryManager 的方法去修改数据
                Debug.Log("拿起了全部物品");
            }
        }
        else
        {
            // 鼠标拿着东西 -> 放入当前格子 (如果基因一样就堆叠，不一样就交换)

            CursorManager.Instance.DropItem(); // 先放下当前物品（这里可以改成交换逻辑）
            
            Debug.Log("放下了物品");
        }
    }
    private void HandleRightClick()
    {
        //如果鼠标是空的，且格子里有>=1的物品
        if(CursorManager.Instance.cursorItemType == CursorItemType.None)
        {
            if(!isEmpty && currentAmount >=1)
            {
                // 拆分数量=currentAmount/2
                int splitAmount = currentAmount / 2;
                //currentEntry可能为空，因此用.Value
                CursorManager.Instance.PickUp(currentEntry.Value, splitAmount);
                Debug.Log($"拆分拿起了 {splitAmount} 个物品");
                // 告知数据层扣除数量
                InventoryManager.Instance.Remove(currentEntry.Value, splitAmount);
                // 这里需要调用 InventoryManager 的方法去修改数据
            }
        }
    }

    // 弹窗方法
    private void OpenTagInputWindow()
    {
        TagUIManager.Instance.ShowWindow(currentEntry.Value);
        Debug.Log("弹出输入框，给种子打标签！");
    }
}
