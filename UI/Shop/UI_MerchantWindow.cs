using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;

public class UI_MerchantWindow : MonoBehaviour
{
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

    
    [Header("商店数据库")]
    public ShopDatabase shopDatabase;

    [Header("种子与果实动态格子配置")]
    public GameObject seedSlotPrefab;  // 拖入 UI_SeedShopSlot 的 Prefab
    public GameObject fruitSlotPrefab; // 拖入 UI_FruitShopSlot 的 Prefab
    private List<UI_FruitShopSlot> spawnedFruitSlots = new List<UI_FruitShopSlot>();
    private List<UI_SeedShopSlot> seedSlots = new List<UI_SeedShopSlot>(); // 拖入的种子格子数组

    [Header("果实批量出售组件")]
    public TextMeshProUGUI totalRevenueText; // 底部栏总价文本
    public Button batchSellConfirmBtn;       // 底部栏出售按钮  

    // 商店专属的格子的数组
    // private UI_SeedShopSlot[] seedSlots;
    // private UI_FruitShopSlot[] fruitSlots;   // 已切换为自动获取
    // 内部状态控制
    private ItemCategory currentShopTab = ItemCategory.Seed;
    // 当前正在弹窗交易的目标
    private bool isPopupForSeed = true; // true=买种子, false=卖果实
    private ShopDatabase.PresetSeedGoods currentSelectedSeed;
    private SpeciesData currentSelectedFruit;
    private int currentItemPrice;
    private int currentQuantity = 1;
    private int maxQuantityLimit = 99;
    
    // 批量出售暂存池：<物种数据, 准备卖出的数量>
    private Dictionary<SpeciesData, int> pendingFruitSales = new Dictionary<SpeciesData, int>();
    void Awake()
    {
        // 游戏启动时一次性抓取两个页面的所有格子
        // seedSlots = seedGridContext.GetComponentsInChildren<UI_SeedShopSlot>();
        // fruitSlots = fruitGridContext.GetComponentsInChildren<UI_FruitShopSlot>();
    }
    void Start()
{
    // 给输入框绑定监听：当玩家手动在输入框打字时触发
    if (popupQuantityInput != null)
    {
        popupQuantityInput.onValueChanged.AddListener(OnInputQuantityChanged);
        // 新增：当玩家输入完毕点击回车或点击空白处时，纠偏文本
        popupQuantityInput.onEndEdit.AddListener(OnInputEndEdit);
    }
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
        // 读取数据库里的列表，而不是 UI 自身的列表
        var seedList = shopDatabase.availableSeeds;
        while (seedSlots.Count < seedList.Count)
        {
            GameObject newSlotObj = Instantiate(seedSlotPrefab, seedGridContext);
            UI_SeedShopSlot slot = newSlotObj.GetComponent<UI_SeedShopSlot>();
            seedSlots.Add(slot);
        }
        for (int i = 0; i < seedList.Count; i++)
        {
            seedSlots[i].gameObject.SetActive(true);
            //seedSlots[i].Refresh(seedList[i], this);
        }
        for (int i = 0; i < seedSlots.Count; i++)
        {
            if (i < seedSlots.Count)
            {
                // 校验是否解锁
                var unlockState = ShopUnlockModule.GetUnlockState(seedList[i].species);
                seedSlots[i].Init(seedList[i], this, unlockState); // 把解锁状态传给格子
                seedSlots[i].gameObject.SetActive(true);
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
        var fruitDict = InventoryManager.Instance.fruitInventory; // 获取只包含数量>0的字典
        var fruitList = fruitDict.ToList();
        // 1. 扩充不足的格子
        while (spawnedFruitSlots.Count < fruitList.Count)
        {
            GameObject newSlotObj = Instantiate(fruitSlotPrefab, fruitGridContext);
            UI_FruitShopSlot slot = newSlotObj.GetComponent<UI_FruitShopSlot>();
            spawnedFruitSlots.Add(slot);
        }
        // 2. 刷新有效数据
        for (int i = 0; i < fruitList.Count; i++)
        {
            spawnedFruitSlots[i].gameObject.SetActive(true);
            spawnedFruitSlots[i].Refresh(fruitList[i].Key, fruitList[i].Value, this);
        }
        // 3. 隐藏多余的已实例化格子（循环复用，不频繁 Destroy）
        for (int i = fruitList.Count; i < spawnedFruitSlots.Count; i++)
        {
            spawnedFruitSlots[i].gameObject.SetActive(false);
        }
    }


    // --- 核心弹窗触发接口（提供给专属Slot调用） ---
    public void OpenPopupForSeed(ShopDatabase.PresetSeedGoods seedGoods)
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

    // 监听玩家手打输入的数字
    private void OnInputQuantityChanged(string input)
    {
        if (int.TryParse(input, out int parsedQuantity))
        {
            // 限制在 1 ~ maxQuantityLimit 之间
            currentQuantity = Mathf.Clamp(parsedQuantity, 1, maxQuantityLimit);
        }
        else
        {
            currentQuantity = 1;
        }

        // 刷新价格显示（注意：不要在 OnInputQuantityChanged 内部再重新赋值 popupQuantityInput.text，否则光标会跳）
        UpdatePriceDisplay();
    }
    private void OnInputEndEdit(string input)
    {
        // 强制将输入框文本重置为合法的数字，防止玩家留下 99999
        popupQuantityInput.text = currentQuantity.ToString();
    }

    // 统一价格刷新逻辑
    private void UpdatePriceDisplay()
    {
        int totalPrice = currentItemPrice * currentQuantity;
        popupPriceText.text = isPopupForSeed ? $"总计支付: {totalPrice}" : $"预计收入: {totalPrice}";
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
        // int totalPrice = currentItemPrice * currentQuantity;
        // 更新弹窗价格显示
        UpdatePriceDisplay();
        // popupPriceText.text = isPopupForSeed ? $"总计支付: {totalPrice}" : $"预计收入: {totalPrice}";
    }
    // --- 4. 确定 / 取消 操作 (Confirm/Cancel 按钮绑定) ---
    public void OnConfirmClick()
    {
        int totalPrice = currentItemPrice * currentQuantity;
        Debug.Log($"[商店] 确认交易：{(isPopupForSeed ? "买种子" : "卖果实")} x{currentQuantity}，总价 {totalPrice}");
        if (isPopupForSeed)
        {
            // 买种子逻辑：扣玩家钱，给玩家发货基础基因种子
            if (WalletManager.Instance.TrySpend(totalPrice))
            {
                // 商店购买的种子统一发放基础杂合 DNA
                GenoType baseDNA = new GenoType("Aa", "Bb", "Cc");
                InventoryManager.Instance.AddSeed(currentSelectedSeed.species, baseDNA, currentQuantity);
                
                // purchasePopupPanel.SetActive(false);
                Debug.Log($"[商店] 成功购买 {currentSelectedSeed.species.speciesName} 种子 x{currentQuantity}");
            }
            // Debug.Log($"[商店] 购买失败，余额不足。当前金币：{WalletManager.Instance.GetGold()}，所需金币：{totalPrice}");
        }
        
        else
        {
            // 记录批量售卖数据，而不是直接卖出
            StagePendingSale(currentSelectedFruit, currentQuantity);
        
        }
        // 关闭弹窗并刷新商店 UI
        purchasePopupPanel.SetActive(false);
        RefreshShopUI();
    }
    // 果实批量售卖核心
    public void StagePendingSale(SpeciesData species, int amount)
    {
        if (amount <= 0)
            pendingFruitSales.Remove(species);
        else
            pendingFruitSales[species] = amount;

        UpdateBatchBottomBar();
    }
    /// <summary>
    /// 查询某个果实当前在草稿池里准备卖多少。
    /// 不在草稿池里则返回 0。
    /// </summary>
    public int GetPendingSaleAmount(SpeciesData species)
    {
        return pendingFruitSales.TryGetValue(species, out int amount) ? amount : 0;
    }

    /// <summary>
    /// 是否在草稿池里
    /// </summary>
    public bool IsPendingSale(SpeciesData species)
    {
        return pendingFruitSales.ContainsKey(species);
    }
    // 底部栏总价刷新
    public void UpdateBatchBottomBar()
    {
        int totalEstimatedGold = 0;
        foreach (var pair in pendingFruitSales)
        {
            totalEstimatedGold += pair.Key.fruitPrice * pair.Value;
        }

        totalRevenueText.text = totalEstimatedGold.ToString();     // 更新总价文本
        batchSellConfirmBtn.interactable = pendingFruitSales.Count > 0; // 设置按钮是否可点
    }

    public void CommitBatchSale()
    {
        if (pendingFruitSales.Count == 0) return;   // 没有待售物品则直接返回

        int totalRevenue = 0;
        // 计算总收入
        foreach (var pair in pendingFruitSales)
        {
            {
            if (InventoryManager.Instance.GetFruitCount(pair.Key) < pair.Value)
                return; // 任何一条不够，整笔取消
}
 
        }

        foreach (var pair in pendingFruitSales)
        {
            InventoryManager.Instance.RemoveFruit(pair.Key, pair.Value);
            totalRevenue += pair.Key.fruitPrice * pair.Value;
        }
        WalletManager.Instance.AddGold(totalRevenue);
        // 清空暂存池并刷新 UI
        pendingFruitSales.Clear();
        UpdateBatchBottomBar();
        RefreshShopUI();
    }
    /// <summary>
    /// 绑定给果实出售页底部的 "一键全选" 按钮
    /// </summary>
    public void OnSelectAllFruitsClick()
    {
        // 获取背包中所有的果实数据
        // var allFruits = InventoryManager.Instance.fruitInventory;
        var allFruits = InventoryManager.Instance.fruitInventory;
        if (allFruits == null || allFruits.Count == 0) return;
        // if (pendingFruitSales.Count == 0) return;

        // 将所有果实按最大数量压入待售卖池
        foreach (var pair in allFruits)
        {
            if (pair.Value > 0)
            {
                pendingFruitSales[pair.Key] = pair.Value;
            }
            // pair.Key = SpeciesData, pair.Value = 拥有数量
            // pendingFruitSales[pair.Key] = pair.Value;
        }

        UpdateBatchBottomBar();
        RefreshShopUI(); // 刷新全部格子的选中高亮状态
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
