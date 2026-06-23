using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UI_SeedShopSlot : MonoBehaviour, IPointerClickHandler
{
    [Header("格子UI组件引用")]
    public Image itemIcon;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemPriceText;
    private UI_MerchantWindow.PresetSeedGoods goodsData;    //
    private UI_MerchantWindow merchantWindow;

    // 初始化商品数据
    public void Init(UI_MerchantWindow.PresetSeedGoods data, UI_MerchantWindow window)
    {
        goodsData = data;
        merchantWindow = window;

        itemIcon.sprite = data.species.speciesSeedIcon;
        itemNameText.text = data.species.speciesName;
        itemPriceText.text = $"{data.species.seedPrice}"; // 直接显示种子价格
    }
    // 点击直接通知商店魔方弹出“买种子”弹窗
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"[商店] 点击种子格子：{goodsData.species.speciesName}，价格：{goodsData.buyPrice}");
        merchantWindow.OpenPopupForSeed(goodsData);
    }
}
