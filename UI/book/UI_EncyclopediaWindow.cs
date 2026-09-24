using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class UI_EncyclopediaWindow : MonoBehaviour
{
    [Header("左侧列表配置")]
    public Transform gridContext;
    public GameObject slotPrefab;
    //public List<SpeciesData> allSpeciesDatabase;
    public SpeciesDatabase allSpeciesDatabase; // 全物种数据库（存放所有作物SO）

    [Header("右侧展台：核心信息")]
    public Image detailPreviewImage;
    public GameObject detailPerfectGlow;
    public TextMeshProUGUI detailNameText;
    public TextMeshProUGUI detailDescText;
    public TextMeshProUGUI detailGeneStatusText; // 极品状态标签

    [Header("右侧展台：线索解锁操作区")]
    public GameObject unlockCover;        // 解锁长条总容器
    public TextMeshProUGUI unlockCostText;       // "研究解锁: 200 G" 或 "线索已解锁"
    public Button unlockClueButton;

    private List<UI_EncyclopediaSlot> spawnedSlots = new List<UI_EncyclopediaSlot>();   // 动态生成的格子列表
    private SpeciesData currentSelectedSpecies;

    private void OnEnable() 
    {
        RefreshLeftGrid();
        // 默认选中第一个

        if (allSpeciesDatabase.allSpecies.Count > 0) ShowDetail(allSpeciesDatabase.allSpecies[0]);
    }

    private void RefreshLeftGrid()
    {
        // 动态生成/复用左侧卡槽
        while (spawnedSlots.Count < allSpeciesDatabase.allSpecies.Count)
        {
            GameObject obj = Instantiate(slotPrefab, gridContext);
            spawnedSlots.Add(obj.GetComponent<UI_EncyclopediaSlot>());
        }

        for (int i = 0; i < allSpeciesDatabase.allSpecies.Count; i++)
        {
            spawnedSlots[i].gameObject.SetActive(true);
            spawnedSlots[i].Init(allSpeciesDatabase.allSpecies[i], this);
        }
        
        for(int i = allSpeciesDatabase.allSpecies.Count; i < spawnedSlots.Count; i++)
        {
            spawnedSlots[i].gameObject.SetActive(false);        // 超过格子隐藏
        }
    }
    /// <summary>
    /// Slot 被点击时调用，刷新右侧面板
    /// </summary>
    public void ShowDetail(SpeciesData species)
    {
        currentSelectedSpecies = species;
        // 查询解锁状态
        bool isDiscovered = EncyclopediaManager.Instance.IsDiscovered(species);
        bool isClueUnlocked = EncyclopediaManager.Instance.IsClueUnlocked(species);
        bool isPerfected = EncyclopediaManager.Instance.IsPerfected(species);

        // 立绘图标处理
        detailPreviewImage.sprite = species.matureSprite;
        // 立绘颜色处理：未解锁时变暗，已解锁时正常显示
        detailPreviewImage.color = isDiscovered ? Color.white : new Color(0f, 0f, 0f, 0.9f);
        detailPerfectGlow.SetActive(isDiscovered && isPerfected);

        if (isDiscovered)
        {
            // 【阶段 2：完全点亮】
            detailNameText.text = species.speciesName;
            detailDescText.text = species.speciesDescription;
            // 极品状态标签
            detailGeneStatusText.text = isPerfected ? "<color=#FFD700>★ 终极纯合型达成 ★</color>" : "基础图鉴已收录";
            
            unlockCover.SetActive(false); // 隐藏解锁覆盖层
        }
        else if (isClueUnlocked)
        {
            // 【阶段 1：已付费购买线索】
            detailNameText.text = "???";
            detailDescText.text = species.advancedClue; // 显示高阶线索
            detailGeneStatusText.text = "尚未发现";

            unlockCover.SetActive(true);
            unlockClueButton.interactable = false; // 按钮置灰
            unlockCostText.text = "线索已研究";
        }
        else
        {
            // 【阶段 0：完全未知】
            detailNameText.text = "???";
            detailDescText.text = species.initialHint; // 显示初阶谜题
            detailGeneStatusText.text = "尚未发现";

            unlockCover.SetActive(true);
            unlockClueButton.interactable = true;
            
            // 绑定购买按钮事件
            unlockCostText.text = $"研究线索: {species.clueUnlockCost} G";
            unlockClueButton.onClick.RemoveAllListeners();
            unlockClueButton.onClick.AddListener(OnBuyClueClicked);
        }
    }
    private void OnBuyClueClicked()
    {
        if (currentSelectedSpecies == null) return;

        if (EncyclopediaManager.Instance.TryUnlockClueWithGold(currentSelectedSpecies))
        {
            // 购买成功后，刷新右侧面板展示 advancedClue
            ShowDetail(currentSelectedSpecies);
        }
        // 如果失败，TrySpend 内部自动触发抖动事件，这里无需额外处理
    }
}
