using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public enum TileState{Empty,Growing,Flowering,Mature} 
public class UI_PlotSlot : MonoBehaviour{
    public Image shadowImage;    // 根部阴影
    public Image plantImage;     // 植物/小芽图片

    [Header("网格坐标 (用于数据层匹配)")]
    public Vector2Int gridPos;
    
    [Header("当前土坑数据")]
    public GenoType calculatedSeed; // 存放杂交/自交后的种子基因等待收获
    public int currentMatchPriority = 4; // 记录当前种子的匹配优先级：0:左, 1:上, 2:右, 3:下, 4:自交(默认)
    public TileState currentState = TileState.Empty;    //初始化土地状态
    public PlantInstanceData currentPlantData;          //当前持有该植物的实例数据（含基因）

    [Header("播种时间")]    
    public int plantedDay;
    public int plantedHour;
    private float growthOffset; // 每个人独一无二的生长偏置（小时）
    // public SpeciesData currentSpecies;
    private int ticksPassed = 0;
    
    // 缓存计算出的阶段时长，避免每帧重复计算
    private int targetGrowingTicks;
    private int targetFloweringTicks;

    // 由 PlotGridManager 初始化时调用获取坐标信息
    public void Init(Vector2Int pos)
    {
        gridPos = pos;
        UpdateVisuals();
    }
    // 事件订阅与退订
    void OnEnable()
    {
        EventBus.OnHourChanged += HandleHourlyGrowth;
    }

    void OnDisable()
    {
        EventBus.OnHourChanged -= HandleHourlyGrowth;
    }
    /// <summary>
    /// 播种方法：接收的是一个包含基因信息的实例数据包
    /// </summary>
    /// public void Plant(PlantInstanceData plantData)
    public void Plant(PlantInstanceData plantData)
    {
        currentPlantData = plantData;

        // 预计算该植物基于基因的实际生长时长
        targetGrowingTicks = currentPlantData.GetActualGrowingTicks();
        targetFloweringTicks = currentPlantData.GetActualFloweringTicks();

        // 记录种植时刻的时间戳
        if (TimeManager.Instance != null)
        {
            plantedDay = TimeManager.Instance.currentDay;
            plantedHour = TimeManager.Instance.currentHour;
        }
        // 利用随机数来产生种植成熟偏差
        int dnaHash = Random.Range(0, 1000);
        growthOffset = (dnaHash % 17) / 10f - 0.8f; // 浮动在 -0.8 ~ +0.9 小时之间
        currentState = TileState.Growing;
        ticksPassed = 0;
        
        UpdateVisuals(); // 更新视觉信息
        Debug.Log($"播种成功：{currentPlantData.speciesTemplate.speciesName}，基因：{currentPlantData.dna}");
    }
    // 对应的时间监听方法
    private void HandleHourlyGrowth(int day, int hour)
    {
        // 只有地块里有植物，且植物还没完全成熟，才需要成长
        if (currentState != TileState.Empty && currentState != TileState.Mature)
        {
            // 计算实际经历的绝对游戏小时数
            int totalHoursPassed = (day - plantedDay) * 24 + (hour - plantedHour);
            ticksPassed = totalHoursPassed;

            // 检查是否达到状态切换点 
            //状态逻辑切换,当同时满足状态与节拍要求时，调用切换方法
            if (currentState == TileState.Growing && ticksPassed + growthOffset >= targetGrowingTicks)
            {
                TransitionTo(TileState.Flowering);
            }
            else if (currentState == TileState.Flowering && ticksPassed + growthOffset >= targetFloweringTicks)
            {
                TransitionTo(TileState.Mature);
            }
        }
    }
    // 状态切换与授粉事件触发
    private void TransitionTo(TileState newState)
    {
        currentState = newState;
        ticksPassed = 0;
        UpdateVisuals();

        if (newState == TileState.Flowering)
        {
            currentMatchPriority = 4; // 每次开花重置优先级
            Debug.Log($"{currentPlantData.speciesTemplate.speciesName} 在 {gridPos} 发起授粉请求");
            
            // 触发开花授粉事件，把当前 UI_PlotSlot 传递给杂交计算器
            EventBus.TriggerPlantFlowering(this); 
        }
        else if (newState == TileState.Mature)
        {
            Debug.Log($"{currentPlantData.speciesTemplate.speciesName} 已成熟！");
        }
    }
    /// <summary>
    /// 收获方法（先判断收获果实还是种子）
    /// </summary>
    public void Harvest(bool isSeedMode)
    {
        //若植物没成熟直接返回
        if (currentState != TileState.Mature) return;
        //针对收获果实和种子的不同情况
        if (isSeedMode)
        {
            //取走算好的种子calculatedSeed
            int count = currentPlantData.speciesTemplate.seedCount;
            InventoryManager.Instance.AddSeed(currentPlantData.speciesTemplate, calculatedSeed, count);
        }
        else
        {
            //取走果实。根据其基因产量决定数量
            int count = currentPlantData.GetActualHarvestQuantity();
            InventoryManager.Instance.AddFruit(currentPlantData.speciesTemplate, count);
        }
        //收获完成后需要将地块清空
        currentPlantData = null;
        calculatedSeed = null;
        currentState = TileState.Empty;
        UpdateVisuals();
    }
    /// <summary>
    /// UI 视觉更新逻辑
    /// </summary>
    void UpdateVisuals()
    {
        // 默认置空
        if (currentState == TileState.Empty)
        {
            shadowImage.gameObject.SetActive(false);
            plantImage.gameObject.SetActive(false);
            return;
        }

        // 有植物：激活阴影与植物图片
        shadowImage.gameObject.SetActive(true);
        plantImage.gameObject.SetActive(true);
        // 根据当前植物的基因数据获取对应的物种模板
        var template = currentPlantData.speciesTemplate;
        
        // 优先读取 Sprite 贴图；若尚未制作贴图，则回退使用 template 配置的 Color 染色以防报错
        Sprite currentSprite = GetSpriteByState(template);
        if (currentSprite != null)
        {
            plantImage.sprite = currentSprite;
            plantImage.color = Color.white;
            //plantImage.SetNativeSize();
        }
        else
        {
            // 色彩兼容备选方案
            switch (currentState)
            {
                case TileState.Growing: plantImage.color = template.growingColor; break;
                case TileState.Flowering: plantImage.color = template.flowerColor; break;
                case TileState.Mature: plantImage.color = template.matureColor; break;
            }
        }
    }
    // 根据当前状态获取对应的 Sprite 贴图
    private Sprite GetSpriteByState(SpeciesData template)
    {
        if (template == null) return null;
        switch (currentState)
        {
            case TileState.Growing: return template.growingSprite;   // 小芽/幼苗图
            case TileState.Flowering: return template.flowerSprite; // 开花图
            case TileState.Mature: return template.matureSprite;   // 成熟/结果图
            default: return null;
        }
    }
}
