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

    private RectTransform rectTransform;

    void Awake()
    {
        Instance = this;
        rectTransform = GetComponent<RectTransform>();
        gameObject.SetActive(false); // 初始时隐藏
    }

    void Update()
    {
        // 让悬浮窗稍微偏离鼠标光标一点距离，防止挡住鼠标
        Vector2 mousePos = Input.mousePosition;
        rectTransform.position = mousePos + new Vector2(15f, -15f);
    }

    /// <summary>
    /// 显示并填充种子信息
    /// </summary>
    public void Show(SeedEntry entry)
    {
        gameObject.SetActive(true);
        // 1. 设置标题
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
    public void Show(SpeciesData species)
    {
        gameObject.SetActive(true);
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
