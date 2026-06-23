using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class UI_FruitShopSlot : MonoBehaviour, IPointerClickHandler
{
    [Header("格子UI组件引用")]
    public Image itemIcon;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemPriceText;
    public bool isEmpty = true;
    private SpeciesData fruitData;
    //private string fruitName;
    private int fruitAmount;
    private UI_MerchantWindow merchantWindow;

    // 初始化商品数据
    public void Refresh(SpeciesData data, int amount, UI_MerchantWindow window)
    {
        isEmpty = false;
        fruitData = data;
        fruitAmount = amount;
        merchantWindow = window;
        itemIcon.color = Color.white; // 显示图标
        itemIcon.sprite = data.speciesFruitIcon;
        itemNameText.text = data.speciesName;
        itemPriceText.text = $"{data.fruitPrice}"; // 显示单价和数量
    }
    public void ClearSlot()
    {
        fruitData = null;
        fruitAmount = 0;
        isEmpty = true;
        itemIcon.color = Color.clear; // 隐藏图标
        itemIcon.sprite = null;
        itemNameText.text = "";
        itemPriceText.text = "";
    }
    // 点击直接通知商店魔方弹出“卖果实”弹窗
    public void OnPointerClick(PointerEventData eventData)
    {
        if (isEmpty)
        {
            Debug.Log("[商店] 点击了空的果实格子，没有任何操作。");
            return; // 如果格子为空，则不执行任何操作
        }
        Debug.Log($"[商店] 点击果实格子：{fruitData.speciesName}，数量：{fruitAmount}");
        merchantWindow.OpenPopupForFruit(fruitData, fruitAmount);
    }
}
