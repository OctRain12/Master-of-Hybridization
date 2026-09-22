using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class PlotGridManager : MonoBehaviour
{
    public static PlotGridManager Instance;

    [Header("网格排版设置")]
    [Tooltip("勾选此项将自动从父物体的 GridLayoutGroup 组件读取 ConstraintCount 动态计算列数")]
    public bool autoGetColumnsFromLayout = true;
        
    [Tooltip("如果未勾选自动读取，则使用此处的自定义列数（比如 4代表 4x3 的田地）")]
    public int customColumns = 4;

    [Header("存储所有 UI_PlotSlot 的字典")]
    public Dictionary<Vector2Int, UI_PlotSlot> plotDictionary = new Dictionary<Vector2Int, UI_PlotSlot>();

    // 搜索优先级：0:左, 1:上, 2:右, 3:下
    private Vector2Int[] searchOrder = new Vector2Int[]
    {
        new Vector2Int(-1, 0), // 左
        new Vector2Int(0, 1),  // 上
        new Vector2Int(1, 0),  // 右
        new Vector2Int(0, -1)  // 下
    };

    void Awake()
    {
        Instance = this;
        InitGrid();
    }

    void OnEnable()
    {
        // 订阅开花授粉事件
        EventBus.OnPlantFlowering += HandleFlowering;
    }

    void OnDisable()
    {
        EventBus.OnPlantFlowering -= HandleFlowering;
    }
    
    /// <summary>
    /// 初始化网格：自动扫描所有子物体的 UI_PlotSlot 并绑定逻辑坐标
    /// </summary>
    public void InitGrid()
    {
        plotDictionary.Clear();

        // 1. 计算当前使用的实际列数 (Columns)
        int activeColumns = customColumns;
        if (autoGetColumnsFromLayout)
        {
            GridLayoutGroup layout = GetComponent<GridLayoutGroup>();
            if (layout != null && layout.constraint == GridLayoutGroup.Constraint.FixedColumnCount)
            {
                activeColumns = layout.constraintCount; // 自动读取 GridLayoutGroup 的 Column 数量
            }
        }

        // 确保列数不小于 1，防止除以 0 报错
        if (activeColumns < 1) activeColumns = 1;

        // 2. 扫描所有子节点里的土坑预制体
        UI_PlotSlot[] slots = GetComponentsInChildren<UI_PlotSlot>();

        for (int i = 0; i < slots.Length; i++)
        {
            // 核心算法：通过索引 i 和实际列数算坐标
            // 无论摆 4x3 (12个), 5x5 (25个), 还是 3x10 (30个)，都能完美精准对应
            int x = i % activeColumns;
            int y = i / activeColumns;

            Vector2Int coord = new Vector2Int(x, y);
            
            // 初始化土坑坐标并加入字典
            slots[i].Init(coord);
            slots[i].name = $"PlotSlot_{x}_{y}";

            if (!plotDictionary.ContainsKey(coord))
            {
                plotDictionary.Add(coord, slots[i]);
            }
            else
            {
                Debug.LogError($"[网格错误] 坐标 {coord} 重复录入！请检查布局。");
            }
        }

        Debug.Log($"[网格初始化完成] 成功录入 {plotDictionary.Count} 个土坑，当前列数为: {activeColumns}");
    }
    // ========================================================================
    // 核心育种授粉双向匹配算法
    // ========================================================================
    private void HandleFlowering(UI_PlotSlot requester)
    {
        if (requester == null || requester.currentPlantData == null) return;
        
        // 0.先检查母体是否在当前环境（如夜间）发生【活体当场突变】
        SpeciesData mutatedSpecies = MutationDatabase.Instance?.CheckMutation(
        requester.currentPlantData.speciesTemplate,
        requester.currentPlantData.dna
        );
        if (mutatedSpecies != null)
        {
            // 触发突变 水稻 -> 云稻（贴图刷新、数据重写、基因保留）
            requester.MutateCurrentPlant(mutatedSpecies);
        }

        // 1. 基于蜕变后的新实体，初始保底：给自己设置自交种子（优先级 defaultCurrentMatchPriority）
        requester.calculatedSeedEntry = BreedingCalculator.CalculateNextGeneration(requester.currentPlantData, null);
        requester.currentMatchPriority = requester.defaultCurrentMatchPriority;

        // 2. 遍历四个方向寻求匹配 (0:左, 1:上, 2:右, 3:下)
        for (int i = 0; i < searchOrder.Length; i++)
        {
            Vector2Int offset = searchOrder[i];
            Vector2Int targetPos = requester.gridPos + offset;

            // 在 UI 土坑字典中匹配坐标
            if (plotDictionary.ContainsKey(targetPos))
            {
                UI_PlotSlot neighbor = plotDictionary[targetPos];

                // 邻居基础校验：存在且处于开花期
                if (neighbor.currentState != TileState.Flowering || neighbor.currentPlantData == null) continue;
                // 邻居和自身物种数据
                SpeciesData mySpecies = requester.currentPlantData.speciesTemplate;
                SpeciesData neighborSpecies = neighbor.currentPlantData.speciesTemplate;

                // 判断条件
                bool isSameSpecies = (mySpecies == neighborSpecies);
                bool canHybridize = false;
                // 仅在异种时查询杂交表（避免同种重复查询）
                if (!isSameSpecies)
                {
                    canHybridize = (HybridDatabase.Instance?.TryGetHybridResult(mySpecies, neighborSpecies) != null);
                }
                // 只有“同物种”或者“能杂交的异种”才具备匹配资格
                if (isSameSpecies || canHybridize)
                {
                    // 分段权重：跨物种(0~3) 绝对优先于 同物种(10~13)
                    int priorityOffset = canHybridize ? 0 : 10;
                    int currentActionPriority = priorityOffset + i;

                    // A. 给自己更新更优优先级的种子
                    if (currentActionPriority < requester.currentMatchPriority)
                    {
                        requester.calculatedSeedEntry = BreedingCalculator.CalculateNextGeneration(requester.currentPlantData, neighbor.currentPlantData);
                        requester.currentMatchPriority = currentActionPriority;
                    }

                    // B. [关键反向匹配] 尝试更新邻居的种子
                    int myDirectionIndexForNeighbor = GetOppositeDirectionIndex(i);

                    if (myDirectionIndexForNeighbor < neighbor.currentMatchPriority)
                    {
                        neighbor.calculatedSeedEntry = BreedingCalculator.CalculateNextGeneration(neighbor.currentPlantData, requester.currentPlantData);
                        neighbor.currentMatchPriority = myDirectionIndexForNeighbor;
                        Debug.Log($"[反向匹配] 邻居 {neighbor.gridPos} 的种子被 {requester.gridPos} 更新了！新优先级: {myDirectionIndexForNeighbor}");
                    }
                }
            }
        }
    }
    // 辅助方法：获取相反方向索引 (0:左 <-> 2:右, 1:上 <-> 3:下)
    private int GetOppositeDirectionIndex(int index)
    {
        if (index == 0) return 2; // 左 -> 右
        if (index == 1) return 3; // 上 -> 下
        if (index == 2) return 0; // 右 -> 左
        if (index == 3) return 1; // 下 -> 上
        return 4;
    }
}
