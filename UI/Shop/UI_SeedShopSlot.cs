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

    [Header("锁定状态表现 (可选组件)")]
    public GameObject lockedOverlay;          // 锁定时的半透明黑底遮罩
    public TextMeshProUGUI lockedReasonText;  // 显示解锁条件的文本 (如"需图鉴解锁")

    private ShopDatabase.PresetSeedGoods currentItemData;    
    private UI_MerchantWindow merchantWindow;
    private bool isUnlocked;

    // 初始化商品数据
    public void Init(ShopDatabase.PresetSeedGoods data, UI_MerchantWindow window, ShopUnlockModule.UnlockState unlockState)
    {
        currentItemData = data;
        merchantWindow = window;
        isUnlocked = unlockState.isUnlocked;

        itemIcon.sprite = data.species.speciesSeedIcon;
        itemNameText.text = data.species.speciesName;
        itemPriceText.text = $"{data.species.seedPrice}"; // 直接显示种子价格
        // 根据解锁状态设置UI
        if (isUnlocked)
        {
            // 已解锁：正常显示价格，允许点击
            itemPriceText.gameObject.SetActive(true);
            itemPriceText.text = $"{currentItemData.species.seedPrice} G"; 
            
            if (lockedOverlay != null) lockedOverlay.SetActive(false);
            if (lockedReasonText != null) lockedReasonText.gameObject.SetActive(false);

        }
        else
        {
            // --- 未解锁：隐藏价格，显示锁遮罩和原因 ---
            itemPriceText.gameObject.SetActive(false);
            
            if (lockedOverlay != null) lockedOverlay.SetActive(true);
            if (lockedReasonText != null) 
            {
                lockedReasonText.gameObject.SetActive(true);
                lockedReasonText.text = unlockState.lockReason; // 填入文案
            }
        }
        }
    // 点击直接通知商店魔方弹出“买种子”弹窗
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"[商店] 点击种子格子：{currentItemData.species.speciesName}");
        merchantWindow.OpenPopupForSeed(currentItemData);
    }
}
