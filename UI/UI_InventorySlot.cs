using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_InventorySlot : MonoBehaviour
{
    [Header("UI 引用")]
    public Image iconImage;
    public TextMeshProUGUI amountText;
    public TextMeshProUGUI tagText;
    public GameObject goldHighlight; // 金色边框物体
    private SeedEntry currentEntry;

    public void Refresh(SeedEntry entry, int amount, string tag)
    {
        currentEntry = entry;
        // 1. 基础信息显示
        iconImage.sprite = entry.species.speciesIcon; // 假设 SpeciesData 里有图标
        amountText.text = amount.ToString();

        // 2. 显示玩家标记
        tagText.text = string.IsNullOrEmpty(tag)?"":tag;
        // 3. 核心逻辑：自动高亮逻辑 (根据玩家标记)
        // 如果玩家标记里包含 "aa" 或 "bb" (隐性纯合)，且玩家设置了高亮
        UpdateHighlight(tag);
    }

    private void UpdateHighlight(string tag)
    {
        // 这里可以根据你的需求定制。
        // 比如：只要玩家标记了 "aa"，就显示金标
        if(!string.IsNullOrEmpty(tag) && tag.ToLower().Contains("aa"))
        {
            goldHighlight.SetActive(true);
        }
        else
        {
            goldHighlight.SetActive(false);
        }
    }
    // 鼠标点击格子的逻辑 (准备种植)
    public void OnSlotClick()
    {
        Debug.Log($"选中了种子：{currentEntry.species.speciesName}，准备种植！");
        // 这里后续接入：CursorManager.Instance.SetHoldingSeed(currentEntry);
    }
}
