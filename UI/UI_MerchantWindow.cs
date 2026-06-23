using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;

public class UI_MerchantWindow : MonoBehaviour
{
    [System.Serializable]
    // 预设种子商品结构体
    public struct PresetSeedGoods
    {
        public SpeciesData species;
        public int buyPrice;
    }

    [Header("页面容器引用")]
    public GameObject merchantPanel;
    public GameObject seedShopPage;
    public GameObject fruitShopPage;
    public Transform seedGridContext;
    public Transform fruitGridContext;

    [Header("数量选择弹窗组件")]
    public GameObject purchasePopupPanel;
    public Image popupItemIcon;
    public TextMeshProUGUI popupItemNameText;
    public TextMeshProUGUI popupItemPriceText;  //单价
    public TextMeshProUGUI popupPriceText;
    public TMP_InputField popupQuantityInput;

    [Header("商品种子配置")]
    public List<PresetSeedGoods> availableSeeds = new List<PresetSeedGoods>();
    [Tooltip("果实的统一基础收购价格配置")]
    public int defaultFruitSellPrice = 15;

    // 商店专属的格子的数组
    private UI_SeedShopSlot[] seedSlots;
    private UI_FruitShopSlot[] fruitSlots;
    // 内部状态控制
    private ItemCategory currentShopTab = ItemCategory.Seed;
    // 当前正在弹窗交易的目标
    private bool isPopupForSeed = true; // true=买种子, false=卖果实
    private PresetSeedGoods currentSelectedSeed;
    private SpeciesData currentSelectedFruit;
    private int currentItemPrice;
    private int currentQuantity = 1;
    private int maxQuantityLimit = 99;

    void Awake()
    {
        // 游戏启动时一次性抓取两个页面的所有格子
        seedSlots = seedGridContext.GetComponentsInChildren<UI_SeedShopSlot>();
        fruitSlots = fruitGridContext.GetComponentsInChildren<UI_FruitShopSlot>();
    }
    void OnEnable()
    {
        InventoryManager.OnInventoryChanged += RefreshShopUI;
        SwitchTab(0); // 默认打开种子页
    }
    void OnDisable()
    {
        InventoryManager.OnInventoryChanged -= RefreshShopUI;
    }
    public void SwitchTab(int tabIndex)
    {
        currentShopTab = (ItemCategory)tabIndex;    // 0=种子, 1=果实
        
        seedShopPage.SetActive(currentShopTab == ItemCategory.Seed);    // 仅当当前标签为种子时显示种子页
        fruitShopPage.SetActive(currentShopTab == ItemCategory.Fruit);
        
        purchasePopupPanel.SetActive(false); // 切换标签时关闭弹窗
        RefreshShopUI();
    }
    private void RefreshShopUI()
    {
        if(currentShopTab == ItemCategory.Seed)
        {
            DrawSeedShop();
        }
        else
        {
            DrawFruitShop();
        }
    }
    // 渲染种子商品页
    private void DrawSeedShop()
    {
        for (int i = 0; i < seedSlots.Length; i++)
        {
            if (i < availableSeeds.Count)
            {
                seedSlots[i].Init(availableSeeds[i], this);
            }
            else
            {
                seedSlots[i].gameObject.SetActive(false); // 超出范围的格子隐藏
            }
        }
    }
    // 渲染果实商品页
    private void DrawFruitShop()
    {
        var fruitList = InventoryManager.Instance.fruitInventory.ToList();

        for (int i = 0; i < fruitSlots.Length; i++)
        {
            if (i < fruitList.Count)
            {
                var pair = fruitList[i];
                fruitSlots[i].Refresh(pair.Key, pair.Value, this);
            }
            else
            {
                fruitSlots[i].ClearSlot();
            }
        }
    }
    // --- 核心弹窗触发接口（提供给专属Slot调用） ---
    public void OpenPopupForSeed(PresetSeedGoods seedGoods)
    {
        isPopupForSeed = true;
        currentSelectedSeed = seedGoods;
        currentItemPrice = seedGoods.species.seedPrice;
        maxQuantityLimit = 99; // 买种子没有上限限制

        // 临时获取对应物种的图片
        popupItemIcon.sprite = seedGoods.species.speciesSeedIcon;
        popupItemNameText.text = $"购买: {seedGoods.species.speciesName} 种子";
        popupItemPriceText.text = $"单价: {seedGoods.species.seedPrice}";
        
        SetQuantity(1);
        purchasePopupPanel.SetActive(true);
    }

    public void OpenPopupForFruit(SpeciesData specie, int maxPlayerHas)
    {
        isPopupForSeed = false;
        currentSelectedFruit = specie;
        //currentItemPrice = defaultFruitSellPrice;
        currentItemPrice = specie.fruitPrice; // 果实价格直接从物种数据读取
        maxQuantityLimit = maxPlayerHas; // 最多只能卖背包里拥有的数量

        // 临时获取对应物种的图片
        popupItemIcon.sprite = specie.speciesFruitIcon;
        popupItemNameText.text = $"出售: {specie.speciesName} 果实";
        popupItemPriceText.text = $"单价: {specie.fruitPrice}";
        
        SetQuantity(1);
        purchasePopupPanel.SetActive(true);
    }

    // --- 数量加减控制 (Minus/Plus 按钮绑定) ---
    public void ModifyQuantity(int amount)
    {
        SetQuantity(currentQuantity + amount);
    }

    private void SetQuantity(int newQuantity)
    {
        // 限制数量在 1 到最大值之间
        currentQuantity = Mathf.Clamp(newQuantity, 1, maxQuantityLimit);
        popupQuantityInput.text = currentQuantity.ToString();
        
        // 计算总价
        int totalPrice = currentItemPrice * currentQuantity;
        // 更新弹窗价格显示
        popupPriceText.text = isPopupForSeed ? $"总计支付: {totalPrice}" : $"预计收入: {totalPrice}";
    }
    // --- 4. 确定 / 取消 操作 (Confirm/Cancel 按钮绑定) ---
    public void OnConfirmClick()
    {
        int totalPrice = currentItemPrice * currentQuantity;

        if (isPopupForSeed)
        {
            // 买种子逻辑：扣玩家钱，给玩家发货基础基因种子
            if (InventoryManager.Instance.ModifyGold(-totalPrice))
            {
                // 商店购买的种子统一发放基础杂合 DNA？？
                GenoType baseDNA = new GenoType("Aa", "Bb", "Cc");
                InventoryManager.Instance.AddSeed(currentSelectedSeed.species, baseDNA, currentQuantity);
                Debug.Log($"[商店] 成功购买 {currentSelectedSeed.species.speciesName} 种子 x{currentQuantity}");
            }
        }
        else
        {
            // 卖果实逻辑：收走玩家果实，给玩家加钱
            if (InventoryManager.Instance.RemoveFruit(currentSelectedFruit, currentQuantity))
            {
                InventoryManager.Instance.ModifyGold(totalPrice);
                Debug.Log($"[商店] 成功卖出 {currentSelectedFruit.speciesName} 果实 x{currentQuantity}，收入 {totalPrice}");
            }
        }
        // 关闭弹窗并刷新商店 UI
        purchasePopupPanel.SetActive(false);
        RefreshShopUI();
    }
    public void OnCancelClick()
    {
        purchasePopupPanel.SetActive(false);
    }
    // 窗口外壳开关
    public void ToggleWindow()
    {
        merchantPanel.SetActive(!merchantPanel.activeSelf);
    }
}
