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
    Fruit,
    Tool
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
    public SpeciesData heldFruit;
    [Header("快捷栏种植模式追踪")]
    public bool isHotbarPlantMode = false;
    public int activeHotbarIndex = -1; // 记录当前点的是哪一个快捷栏格子
    [Header("工具模式")]
    public ToolMode currentToolMode = ToolMode.None;

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
        FollowModeDetect();
    }
    // 快捷栏专属：激活虚拟播种模式
    public void ActivateHotbarPlantMode(int hotbarIndex, SeedEntry entry, int amount)
    {
        // 切换回普通/播种模式，关闭果篮等工具
        SwitchToolMode(ToolMode.None);

        isHotbarPlantMode = true;
        activeHotbarIndex = hotbarIndex;

        // 让鼠标视觉上变成种子
        cursorItemType = CursorItemType.Seed;
        heldSeed = entry;
        heldAmount = amount;

        cursorIcon.gameObject.SetActive(true);
        cursorIcon.sprite = entry.species.speciesSeedIcon;
        cursorIcon.color = Color.white;
        cursorAmountText.text = amount.ToString();
    }
    // 更新种植数量
    public void RefreshHotbarPlantAmount(int newAmount)
    {
        // 如果数量为0，直接退出种植模式
        if (newAmount <= 0)
        {
            ExitHotbarPlantMode();
        }
        else
        {
            heldAmount = newAmount;
            cursorAmountText.text = newAmount.ToString();
        }
    }
    // 退出快捷栏种植模式
    public void ExitHotbarPlantMode()
    {
        isHotbarPlantMode = false;
        activeHotbarIndex = -1;
        DropItem();
    }
    // 取消播种模式
    private void CancelPlantModeCheck()
    {
        // 如果判定没有点到可交互土地，退出模式
        // 在 LandTile 里成功种植时，会提早取消或刷新这个 Check
        if (isHotbarPlantMode)
        {
            // 通过射线检测是否点在了土地上，如果没有，强行退出
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (hit.collider == null || hit.collider.GetComponent<LandTile>() == null)
            {
                ExitHotbarPlantMode();
                Debug.Log("[播种] 点击了无效区域，退出播种模式。");
            }
        }
    }
    /// <summary>
    /// 💡 全局重置指针状态（安全退回物品，清除工具形态，恢复普通指针）
    /// </summary>
    public void ResetCursorState()
    {
        // 情况 1：如果是快捷栏虚拟播种模式点空了
        if (isHotbarPlantMode)
        {
            // 因为是快捷栏映射，真实的种子数量根本没离开过数据层
            // 只需要干脆利落地“退出播种状态”并清理鼠标贴图即可
            ExitHotbarPlantMode();
            //Debug.Log("[指针重置] 快捷栏播种取消，种子安然无恙。");
        }
        // 情况 2：如果是主背包里真实抓起来的物品（种子或果实）
        else if (cursorItemType != CursorItemType.None && (heldSeed.species != null || !string.IsNullOrEmpty(heldFruit.speciesName)))
        {
            // 触发安全回收，把实体物品退回 InventoryManager 仓库，防止物品蒸发
            ReturnHeldItemToInventory();
            Debug.Log("[指针重置] 实物抓取取消，手持物品已安全退回背包。");
        }
        // 情况 3：如果手里什么都没抓，但是正拿着果篮或种子提取器工具
        else if (currentToolMode != ToolMode.None)
        {
            // 直接将工具模式切回 None，DropItem 会自动把手持的工具贴图清掉
            currentToolMode = ToolMode.None;
            EventBus.TriggerToolModeChanged(ToolMode.None);
            DropItem();
            Debug.Log("[指针重置] 工具已收回，恢复普通指针。");
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
        heldFruit = null;         //置空果实数据
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
        heldFruit = species;
        heldAmount = amount;
        heldSeed = default;     //置空种子数据

        //传入果实图标
        cursorIcon.sprite = species.speciesFruitIcon;
        cursorAmountText.text = amount.ToString();
        cursorItemUI.SetActive(true);
    }


    /// <summary>
    /// 强制将手里的物品退回到背包仓库中（用于关闭背包、强行中断等安全保护）
    /// </summary>
    public void ReturnHeldItemToInventory()
    {
        if (cursorItemType == CursorItemType.None) return;

        if (cursorItemType == CursorItemType.Seed) // 说明手里拿的是种子
        {
            InventoryManager.Instance.AddSeed(heldSeed.species, heldSeed.dna, heldAmount);
        }
        else if (cursorItemType == CursorItemType.Fruit) // 说明手里拿的是果实
        {
            InventoryManager.Instance.AddFruit(heldFruit, heldAmount);
        }

        DropItem(); // 清空手
}
    //放下/取消方法
    public void DropItem()
    {
        cursorItemType = CursorItemType.None;
        heldSeed = default;
        heldFruit = null;
        heldAmount = 0;
        cursorItemUI.SetActive(false);
    }

    // 切换工具模式（果篮/种子提取器）
    public void SwitchToolMode(ToolMode newMode)
    {
        if (isHotbarPlantMode) ExitHotbarPlantMode();

        currentToolMode = newMode;
        // 触发工具模式切换事件
        EventBus.TriggerToolModeChanged(newMode);
        
        if (currentToolMode != ToolMode.None)
        {
            cursorItemType = CursorItemType.Tool;
            //cursorItemUI.SetActive(true);
            cursorIcon.gameObject.SetActive(true);
            cursorIcon.color = Color.white;
            cursorAmountText.text = "";
            // 改变光标为工具图标
            // cursorIcon.sprite = 工具图标; 
            Debug.Log($"[工具切换] 当前手持工具: {currentToolMode}");
        }
        else
        {
            DropItem();
        }
    }
    /// <summary>
    /// 绑按钮：通过传入字符串切换工具（例如输入 "FruitBasket"），解决Unity不识别枚举的bug
    /// </summary>
    public void SwitchToolModeString(string modeName)
    {
        // 将字符串尝试转换成对应的枚举
        if (System.Enum.TryParse(modeName, out ToolMode parsedMode))
        {
            SwitchToolMode(parsedMode);
        }
        else
        {
            Debug.LogError($"[工具错误] 找不到名为 {modeName} 的工具模式！请检查拼写。");
        }
    }

    private void FollowModeDetect()
    {
        // 只要不是 None，就让 UI 跟随鼠标
        if(cursorItemType != CursorItemType.None)
        {
            cursorItemUI.transform.position = Input.mousePosition;
        }
        // 检测取消点击：如果处于种植/虚拟拾取状态，点击了非土地、非UI的地方，取消状态
        if (isHotbarPlantMode && Input.GetMouseButtonDown(0))
        {
            // 如果鼠标没有悬停在土地地块上，且没有点在 UI 上，则在 LateUpdate 或下一帧判定取消
            Invoke("CancelPlantModeCheck", 0.05f);
        }
    }
}
