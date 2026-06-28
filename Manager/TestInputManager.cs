using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class TestInputManager : MonoBehaviour
{
    private bool isOnUI; //当前鼠标指针是否正悬停在任何 UI 元素上
    //面板选择对应物种
    [Header("选择物种")]
    public SpeciesData speciesA;
    public SpeciesData speciesB;
    public GridManager gridManager;
    [Header("UI 引用")]
    public GameObject inventoryUI;
    [Header("HUD面板引用")]
    public CanvasGroup hudCanvasGroup;

    void Start()
    {
        inventoryUI.SetActive(false); //初始状态关闭背包UI
    }

    // Update is called once per frame
    void Update()
    {
        OpenInventoryUI();
        isOnUI = EventSystem.current.IsPointerOverGameObject();
        OnMouseOver();
        if(!isOnUI)
        {
            InputManager(); //当不在UI上才能射线检测操作
        }

    }
    //输入操作方法
    private void InputManager()
    {
        //获取鼠标下的地块
        LandTile hoveredTile = GetTileUnderMouse();
        if(hoveredTile == null) return;
        //种植模式(鼠标左键)
        if(Input.GetMouseButtonDown(0))
        {
            OnMouseDownPlant(hoveredTile); //调用地块的鼠标点击方法
        }
        //按键1：种植A,需要判断是否为空土地
        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            if(hoveredTile.currentState == TileState.Empty)
            {
                hoveredTile.Plant(new PlantInstanceData(speciesA, speciesA.defaultGenoType));
            }
        }
        //按键2：种植B,需要判断是否为空土地
        if(Input.GetKeyDown(KeyCode.Alpha2))
        {
            if(hoveredTile.currentState == TileState.Empty)
            {
                hoveredTile.Plant(new PlantInstanceData(speciesB, speciesB.defaultGenoType));
            }
        }
        //按键3：收获果实，需要判断是否成熟
        if(Input.GetKeyDown(KeyCode.Alpha3))
        {
            if(hoveredTile.currentState == TileState.Mature)
            {
                hoveredTile.Harvest(false);
            }
        }
        //按键4：收获种子
        if(Input.GetKeyDown(KeyCode.Alpha4))
        {
            if(hoveredTile.currentState == TileState.Mature)
            {
                hoveredTile.Harvest(true);
            }
        }
    }
    //打开背包面板
    private void OpenInventoryUI()
    {
        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
        {
            // 如果当前选中的 UI 是一个输入框 (TMP_InputField 或原生的 InputField)
            if (EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>() != null)
            {
                return; // 直接终止！不要检测任何快捷键，让玩家安心打字
            }
        }
        //开关背包逻辑
        if(Input.GetKeyDown(KeyCode.B))
        {
            if(inventoryUI != null)
            {   
                bool nextState = !inventoryUI.activeSelf;   //取反当前的状态（开变关，关变开）
                //安全回收
                // 如果接下来的状态是“关闭”，且鼠标正抓着东西，先回收
                /*if (nextState == false && CursorManager.Instance.cursorItemType != CursorItemType.None)
                {
                    CursorManager.Instance.ResetCursorState();
                }*/
                inventoryUI.SetActive(!inventoryUI.activeSelf);
                CursorManager.Instance.ResetCursorState(); //切换背包时，重置鼠标状态，避免物品丢失
            // 退出背包时，HUD 的可交互和射线检测恢复
            if (hudCanvasGroup != null)
                {
                    // 如果背包打开了（nextState为true），HUD 的可交互和射线检测变为 false（直接被彻底无视）
                    // 如果背包关闭了（nextState为false），HUD 恢复可交互（true）
                    hudCanvasGroup.interactable = !nextState;
                    hudCanvasGroup.blocksRaycasts = !nextState;
                }
            }
        }
    }

    //采用射线检测当前鼠标下的地块
    private LandTile GetTileUnderMouse()
    {
        //获取对应世界坐标
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos,Vector2.zero);
        //成功获取到collider的时候，返回对应地块
        if(hit.collider != null)
        {
            return hit.collider.GetComponent<LandTile>();
        }
        return null;
    }

    // 鼠标检测
    private void OnMouseOver()
    {
        if (CursorManager.Instance.cursorItemType == CursorItemType.None) return;
        if (Input.GetMouseButtonDown(0)) // 当玩家按下左键
        {
            // 1. 检查是否点在了 UI 上 (如果是点在背包里或者切换工具按钮上，绝对不能触发重置)
            if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                return; // 点的是 UI，跳过重置判定
            }

            // 2. 发射射线，看看鼠标底下有没有带 Collider 的土地
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);

            // 如果鼠标底下什么都没点到，或者点到的物体身上没有 LandTile 组件
            if (hit.collider == null || hit.collider.GetComponent<LandTile>() == null)
            {
                // 说明玩家点在了土地外面的世界，执行全局重置
                CursorManager.Instance.ResetCursorState();
            }
        }
    }
        private void OnMouseDownPlant(LandTile hoveredTile)
    {
        // ==========================================
        // 1. 快捷栏播种模式检测
        // ==========================================
        if (CursorManager.Instance.isHotbarPlantMode)
        {
            if (hoveredTile.currentState == TileState.Empty)
            {
                int hotbarIndex = CursorManager.Instance.activeHotbarIndex;
                SeedEntry seedToPlant = CursorManager.Instance.heldSeed;
                // 获取对应种子数据信息，临时变量
                SpeciesData speciesTemplate = seedToPlant.species;
                GenoType dna = seedToPlant.dna;

                // 种植对应种子

                hoveredTile.Plant(new PlantInstanceData(speciesTemplate, dna));

                // 扣除数据层快捷栏里的种子数量
                InventoryManager.Instance.ConsumeHotbarSeed(hotbarIndex, 1);

                // 联动刷新鼠标上的数字镜像
                var updatedData = InventoryManager.Instance.hotbarSlots[hotbarIndex];
                if (updatedData.isEmpty)
                {
                    CursorManager.Instance.ExitHotbarPlantMode(); // 种子扣光了，退出播种
                }
                else
                {
                    CursorManager.Instance.RefreshHotbarPlantAmount(updatedData.amount); // 还有剩，更新鼠标数字
                }
                Debug.Log($"[快捷栏播种] 成功在 {hoveredTile.gridPos} 种下种子，快捷栏数量同步扣除。");
            }
            else
            {
                // 土地已经有作物，取消播种模式
                CursorManager.Instance.ExitHotbarPlantMode();
                Debug.Log("[播种失败] 该地块已有作物，退出播种模式。");
            }
            return;
        }
        // ==========================================
        // 2. 工具收获/提取模式检测
        // ==========================================
        ToolMode currentTool = CursorManager.Instance.currentToolMode;

        if (currentTool == ToolMode.FruitBasket)
        {
            hoveredTile.Harvest(false); // 收获果实
        }
        else if (currentTool == ToolMode.SeedExtractor)
        {
            hoveredTile.Harvest(true); // 收获种子
        }
    }
}
