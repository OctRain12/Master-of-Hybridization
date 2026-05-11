using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridManager : MonoBehaviour
{
    
    [Header("地块设置")]
    public int high = 5;
    public int width = 5;
    public float cellsize = 1.1f;

    [Header("引用")]
    public GameObject gridPrefab;
    [Header("物种库")]
    public SpeciesData fastGrowsSpecies;
    public SpeciesData slowGrowSpecies;
    
    //关键字典，用于以后地块的索引等
    private Dictionary<Vector2Int,GameObject> gridDictionary = new Dictionary<Vector2Int, GameObject>();
    
    
    void Start()
    {
        Camera.main.transform.position = new Vector3(width*cellsize/2-cellsize/2,high*cellsize/2-cellsize/2,-10);
        GenerateGrid();    
    }
    void OnEnable()
    {
        EventBus.OnPlantFlowering += HandleFlowering;   //接收到授粉事件后，调用 HandleFlowering 方法处理
    }
    void OnDisable()
    {
        EventBus.OnPlantFlowering -= HandleFlowering;
    }

    void Update()
    {
        /*-------------改用TestInputManager实现
        //临时测试种植逻辑，采用鼠标左右键
        if(Input.GetMouseButtonDown(0))
        {
            HandlePlanting(fastGrowsSpecies);
        }
        if(Input.GetMouseButtonDown(1))
        {
            HandlePlanting(slowGrowSpecies);
        }
        */
    }
    /*-------------改用TestInputManager实现
    //种植作物(射线检测)
    void HandlePlanting(SpeciesData species)
    {
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition),Vector2.zero);
        if(hit.collider != null)
        {
            LandTile tile = hit.collider.GetComponent<LandTile>();
            if(tile != null && tile.currentState == TileState.Empty)
            {
                GenoType mockDna = species.defaultGenoType; //获取Dna模板
                PlantInstanceData newSeed = new PlantInstanceData(species, mockDna);
                tile.Plant(newSeed);
            }
        }
        //采用射线检测点击的土地块
        /*RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition),Vector2.zero);
        if(hit.collider != null)
        {
            LandTile tile = hit.collider.GetComponent<LandTile>();
            if(tile != null && tile.currentState == TileState.Empty)
            {
                tile.Plant(species);
            }
        }
        */
    
    // 创建数组，搜索优先级：左 -> 上 -> 右 -> 下
    private Vector2Int[] searchOrder = new Vector2Int[]
    {
        new Vector2Int(-1, 0), // 左
        new Vector2Int(0, 1),  // 上
        new Vector2Int(1, 0),  // 右
        new Vector2Int(0, -1)  // 下
    };
    //生成地块
    void GenerateGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < high; y++)
            {
                //世界坐标
                Vector2 pos = new Vector2(x*cellsize,y*cellsize);
                //实例化地块
                GameObject newTile = Instantiate(gridPrefab, pos, quaternion.identity, transform);
                //命名
                newTile.name = $"Tile_{x}_{y}";
                //赋予地块身份信息
                LandTile tile = newTile.GetComponent<LandTile>();
                Vector2Int coord = new Vector2Int(x, y);
                tile.Init(coord); //调用 Init 方法设置坐标信息
                //添加到字典
                gridDictionary.Add(coord, newTile);
            }
        }
    }
    // 杂交匹配逻辑
    private void HandleFlowering(LandTile requester)
    {
        // 1. 初始状态：先给自己设一个保底的自交种子（优先级 4）
        requester.calculatedSeed = BreedingCalculator.CalculateNextGeneration(requester.currentPlantData, null);
        requester.currentMatchPriority = 4; 

        // 2. 按照优先级顺序寻找邻居 (0:左, 1:上, 2:右, 3:下)
        for (int i = 0; i < searchOrder.Length; i++)
        {
            Vector2Int offset = searchOrder[i];
            Vector2Int targetPos = requester.gridPos + offset;
            // 通过字典检查边界
            if (gridDictionary.ContainsKey(targetPos))
            {
                LandTile neighbor = gridDictionary[targetPos].GetComponent<LandTile>();
                // 判定条件：邻居正在开花且同种
                if(neighbor.currentState == TileState.Flowering && 
                   neighbor.currentPlantData != null &&
                   neighbor.currentPlantData.speciesTemplate == requester.currentPlantData.speciesTemplate)
                {
                    // A. 更新自己的种子,按顺序找
                    if(i < requester.currentMatchPriority)
                    {
                        requester.calculatedSeed = BreedingCalculator.CalculateNextGeneration(requester.currentPlantData, neighbor.currentPlantData);
                        requester.currentMatchPriority = i; // 更新匹配优先级
                    }
                    // B. [关键] 尝试更新邻居的种子(为在开花的邻居尝试更新为优先级更高的状态)
                    // 计算“我”在邻居的哪个方向。比如我在邻居的右边，那对邻居来说，我的索引是 2
                    int myDirectionIndexForNeighbor = GetOppositeDirectionIndex(i);
                    // 如果邻居目前的匹配优先级不如我（或者他还是自交），则强制邻居跟我杂交
                    if(myDirectionIndexForNeighbor < neighbor.currentMatchPriority)
                    {
                        neighbor.calculatedSeed = BreedingCalculator.CalculateNextGeneration(neighbor.currentPlantData, requester.currentPlantData);
                        neighbor.currentMatchPriority = myDirectionIndexForNeighbor; // 更新邻居的匹配优先级
                        Debug.Log($"[反向匹配] 邻居 {neighbor.gridPos} 的种子被 {requester.gridPos} 更新了！新优先级: {myDirectionIndexForNeighbor}");
                    }
                    // 注意：这里由于是分支B（父本共享），我虽然找到了一个伴侣，
                    // 但为了不破坏其他邻居可能的匹配，我们只在找到最心仪的(i=0)时可以提前break，
                    // 否则建议跑完循环，确保周围所有邻居都能感知到“我”这个新伴侣的加入。
                }
            }
        }
        /*-----------旧检测方法-----------
        // 默认置空
        LandTile partner= null;
        // 遍历搜索优先级
        foreach(Vector2Int offset in searchOrder)
        {
            Vector2Int targetPos = requester.gridPos + offset;
            // 通过字典检查边界
            if(gridDictionary.ContainsKey(targetPos))
            {
                LandTile neighborTile = gridDictionary[targetPos].GetComponent<LandTile>();
                // 检查是否满足授粉条件（有植物且开花且同物种）
                if(neighborTile.currentState == TileState.Flowering && 
                   neighborTile.currentPlantData != null &&
                   neighborTile.currentPlantData.speciesTemplate == requester.currentPlantData.speciesTemplate)
                {
                    partner= neighborTile;
                    break; // 找到第一个符合条件的邻居后停止搜索
                }
            }
        }
        // 调用计算器得出下一代基因
        // 如果 partner 为 null，BreedingCalculator 内部会自动处理为自交
        GenoType nextGen = BreedingCalculator.CalculateNextGeneration(requester.currentPlantData, partner?.currentPlantData);
        // 将结果存回地块，等玩家收割时取用
        requester.calculatedSeed = nextGen;
        // 输出日志
        Debug.Log($"[育种] 坐标 {requester.gridPos} 已完成匹配。父本: {(partner != null ? partner.gridPos.ToString() : "自交")}. 结果: {nextGen}");
        -----------旧检测方法-----------*/
    }
    
    // 辅助方法：获取相反方向的索引
    // 顺序：0:左, 1:上, 2:右, 3:下
    private int GetOppositeDirectionIndex(int index)
    {
        if (index == 0) return 2; // 左 -> 右
        if (index == 1) return 3; // 上 -> 下
        if (index == 2) return 0; // 右 -> 左
        if (index == 3) return 1; // 下 -> 上
        return 4;
    }


}
