using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public enum TileState{Empty,Growing,Flowering,Mature} 
public class LandTile : MonoBehaviour
{
    
    [HideInInspector] 
    public Vector2Int gridPos; // 地块的坐标身份证
    public GenoType calculatedSeed; // 存放杂交/自交后的种子基因等待收获
    public int currentMatchPriority = 4; // 记录当前种子的匹配优先级：0:左, 1:上, 2:右, 3:下, 4:自交(默认)
    public TileState currentState = TileState.Empty;    //初始化土地状态
    public PlantInstanceData currentPlantData;          //当前持有该植物的实例数据（含基因）
    // public SpeciesData currentSpecies;
    private int ticksPassed = 0;
    private SpriteRenderer sr;

    // 缓存计算出的阶段时长，避免每帧重复计算
    private int targetGrowingTicks;
    private int targetFloweringTicks;


    // 由 GridManager 在生成或初始化时调用获取坐标信息
    public void Init(Vector2Int pos)
    {
        gridPos = pos;
    }

    //事件订阅与退订
    void OnEnable()
    {
        EventBus.OnTickTrigger += OnTick; 
    }

    //物体被隐藏或消失的时候退订
    void OnDisable()
    {
        EventBus.OnTickTrigger -= OnTick;
    }
    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// 播种方法：接收的是一个包含基因信息的实例数据包
    /// </summary>
    public void Plant(PlantInstanceData plantData)
    {
        currentPlantData = plantData;

        //预计算该植物基于基因的实际生长时长
        targetGrowingTicks = currentPlantData.GetActualGrowingTicks();
        targetFloweringTicks = currentPlantData.GetActualFloweringTicks();

        //作物种下更新信息
        //currentSpecies = species;
        currentState = TileState.Growing;
        ticksPassed = 0;
        UpdateVisuals();    //更新视觉信息
        Debug.Log($"播种成功：{currentPlantData.speciesTemplate.speciesName}，基因：{currentPlantData.dna}");
        
    }

    //用于被时间广播调用
    public void OnTick()
    {
        //如果土地为空或成熟直接跳过
        if(currentState == TileState.Empty || currentState == TileState.Mature) return;
        //每次触发一次，节拍+1
        ticksPassed++;
        //状态逻辑切换,当同时满足状态与节拍要求时，调用切换方法
        if(currentState == TileState.Growing && ticksPassed > targetGrowingTicks)
        {
            TransitionTo(TileState.Flowering);
        }
        else if(currentState == TileState.Flowering && ticksPassed > targetFloweringTicks)
        {
            TransitionTo(TileState.Mature);
        }
    }

    //进入状态方法，用于切换状态
    private void TransitionTo(TileState newState)
    {
        currentState = newState;
        ticksPassed = 0;
        UpdateVisuals();
        if (newState == TileState.Flowering)
        {
            // 进入开花状态同时触发授粉事件，传递当前 LandTile 实例
            currentMatchPriority = 4; // 每次开花重置优先级
            Debug.Log($"{currentPlantData.speciesTemplate.speciesName} 在 {gridPos} 发起授粉请求");
            EventBus.TriggerPlantFlowering(this);
        }
        else if (newState == TileState.Mature)
        {
            Debug.Log($"{currentPlantData.speciesTemplate.speciesName} 已成熟！");
        }
    }
    void UpdateVisuals()
    {
        //根据不同状态切换颜色
        if(currentState == TileState.Empty)
        {
            sr.color = Color.white; // 空地显示白色
            return;
        }
        // 从物种模板中读取配置的颜色
        var template = currentPlantData.speciesTemplate;
        switch (currentState)
        {
            case TileState.Growing: sr.color = template.growingColor; break;
            case TileState.Flowering: sr.color = template.flowerColor; break;
            case TileState.Mature: sr.color = template.matureColor; break;
        }
    }

    //---收获方法---
    //先判断是收获种子还是收获果实
    public void Harvest(bool isSeedMode)
    {
        //若植物没成熟直接返回
        if(currentState != TileState.Mature) return;
        //针对收获果实和种子的不同情况
        if(isSeedMode)
        {
            //取走算好的种子calculatedSeed
            int count = currentPlantData.speciesTemplate.seedCount;
            InventoryManager.Instance.addSeed(currentPlantData.speciesTemplate, calculatedSeed, count);
        }
        else
        {
            //取走果实。根据其基因产量决定数量
            int count = currentPlantData.GetActualHarvestQuantity();
            string name = currentPlantData.speciesTemplate.speciesName;
            InventoryManager.Instance.addFruit(name, count);
        }
        //收获完成后需要将地块清空
        currentPlantData = null;
        calculatedSeed = null;
        currentState = TileState.Empty;
        UpdateVisuals();

    }


}
