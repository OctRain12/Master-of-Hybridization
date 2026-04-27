using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileState{Empty,Growing,Flowering,Mature} 
public class LandTile : MonoBehaviour
{
    public TileState currentState = TileState.Empty;    //初始化土地状态
    public PlantInstanceData currentPlantData;          //当持有该植物的实例数据（含基因）
    // public SpeciesData currentSpecies;
    private int ticksPassed = 0;
    private SpriteRenderer sr;

    // 缓存计算出的阶段时长，避免每帧重复计算
    private int targetGrowingTicks;
    private int targetFloweringTicks;

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
            // TODO: 后续在这里触发 EventBus.TriggerPlantFlowering(this);
            Debug.Log($"{currentPlantData.speciesTemplate.speciesName} 进入授粉期！");
        }
        else if (newState == TileState.Mature)
        {
            Debug.Log($"{currentPlantData.speciesTemplate.speciesName} 已成熟！");
        }
    }
    void UpdateVisuals()
    {
        //根据不同状态切换颜色
        if(currentState == TileState.Empty) sr.color = Color.white;
            // 从物种模板中读取配置的颜色
            var template = currentPlantData.speciesTemplate;
            switch (currentState)
            {
                case TileState.Growing: sr.color = template.growingColor; break;
                case TileState.Flowering: sr.color = template.flowerColor; break;
                case TileState.Mature: sr.color = template.matureColor; break;
            }
    }
}
