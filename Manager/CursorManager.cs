using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//果实和种子拾取切换枚举
public enum CursorItemType
{
    None,
    Seed,
    Fruit
}
public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance;

    [Header("UI引用")]
    public GameObject cursorItemUI;     //拿去的物品UI
    public Image cursorIcon;            //拿去物品UI图标
    public TextMeshProUGUI cursorAmountText; //拿取物品数量

    [Header("当前拾取物品")]
    public CursorItemType cursorItemType = CursorItemType.None; //默认无拿取
    public int heldAmount;
    
    // 两种不同的数据容器，谁有用就存谁
    public SeedEntry heldSeed;
    public string heldFruit;

    void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        cursorItemUI.SetActive(false);
    }
    void Update()
    {
        // 只要不是 None，就让 UI 跟随鼠标
        if(cursorItemType != CursorItemType.None)
        {
            cursorItemUI.transform.position = Input.mousePosition;
        }
    }

    // ==========================================
    // 重载方法 1：专用于抓起【种子】
    // ==========================================
    public void PickUp(SeedEntry entry, int amount)
    {
        //拾取种子后切换为拾取种子模式
        cursorItemType = CursorItemType.Seed;
        heldSeed = entry;
        heldFruit = "";         //置空果实数据
        heldAmount = amount;

        //设置传入种子图标
        cursorIcon.sprite = entry.species.speciesSeedIcon;
        cursorAmountText.text = amount.ToString();
        cursorItemUI.SetActive(true);   
    }

    // ==========================================
    // 重载方法 2：专用于抓起【果实】
    // ==========================================
    public void PickUp(SpeciesData species, int amount)
    {
        //拾取果实后切换为拾取果实模式
        cursorItemType = CursorItemType.Fruit;
        heldFruit = species.speciesName;
        heldAmount = amount;
        heldSeed = default;     //置空种子数据

        //传入果实图标
        //todo:果实图标
        //cursorIcon.sprite = species.speciesIcon;
        cursorAmountText.text = amount.ToString();
        cursorItemUI.SetActive(true);
    }

    //放下/取消方法
    public void DropItem()
    {
        cursorItemType = CursorItemType.None;
        heldSeed = default;
        heldFruit = "";
        heldAmount = 0;
        cursorItemUI.SetActive(false);
    }
}
