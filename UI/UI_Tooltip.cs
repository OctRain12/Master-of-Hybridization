using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UI_Tooltip : MonoBehaviour
{
    public static UI_Tooltip Instance;

    [Header("UI 引用")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI categoryText;
    public TextMeshProUGUI tagText;
    public TextMeshProUGUI dnaText;
    public TextMeshProUGUI descriptionText; 

    [Header("位置偏移设置")]
    public Vector2 positionOffset = new Vector2(150f, -150f); // 格子偏移量，防止遮挡
    private RectTransform rectTransform;

    void Awake()
    {
        Instance = this;
        rectTransform = GetComponent<RectTransform>();
        gameObject.SetActive(false); // 初始时隐藏
    }


    /// <summary>
    /// 显示并填充种子信息
    /// </summary>
    public void Show(SeedEntry entry, Vector3 slotPosition)
    {

        gameObject.SetActive(true);
        // 1. 设置标题
        rectTransform.position = slotPosition + (Vector3)positionOffset;
        titleText.text = entry.species.speciesName + " 种子";

        // 2. 获取并设置玩家标记
        string tag = InventoryManager.Instance.GetSeedTag(entry);
        tagText.text = string.IsNullOrEmpty(tag) ? "<color=#888888>未标记</color>" : $"标记: <color=#FFCC00>{tag}</color>";

        // 3. 翻译并格式化基因型数据 (美化文本表现)
        GenoType dna = entry.dna;
        dnaText.text = $"基因型: [<color=#FF5555>{dna.speedGene}</color> | <color=#55FF55>{dna.yieldGene}</color> | <color=#5555FF>{dna.resistGene}</color>]";
    }
    /// <summary>
    /// 重载Show方法显示并填充果实信息
    /// </summary>
    public void Show(SpeciesData species, Vector3 slotPosition)
    {
        gameObject.SetActive(true);
        rectTransform.position = slotPosition + (Vector3)positionOffset;
        titleText.text = species.speciesName + " 果实";
        tagText.text = ""; // 果实没有玩家标记
        dnaText.text = ""; // 果实没有基因信息
    }
    /// <summary>
    /// 隐藏悬浮窗
    /// </summary>
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
