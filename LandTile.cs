using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileState{Empty,Growing,Flowering,Mature} 
public class LandTile : MonoBehaviour
{
    public TileState currentState = TileState.Empty;    //初始化土地状态
    public SpeciesData currentSpecies;
    private int ticksPassed = 0;
    private SpriteRenderer sr;

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

    //种植方法
    public void Plant(SpeciesData species)
    {
        //作物种下更新信息
        currentSpecies = species;
        currentState = TileState.Growing;
        ticksPassed = 0;
        UpdateVisuals();    //更新视觉信息
    }

    //用于被时间广播调用
    public void OnTick()
    {
        //如果土地为空直接跳过
        if(currentState == TileState.Empty) return;
        //每次触发一次，节拍+1
        ticksPassed++;
        //状态逻辑切换,当同时满足状态与节拍要求时，调用切换方法
        if(currentState == TileState.Growing && ticksPassed > currentSpecies.growingTicks)
        {
            TransitionTo(TileState.Flowering);
        }
        else if(currentState == TileState.Flowering && ticksPassed > currentSpecies.flowerTicks)
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
    }
    void UpdateVisuals()
    {
        //根据不同状态切换颜色
        if(currentState == TileState.Empty) sr.color = Color.white;
        else if(currentState == TileState.Growing) sr.color = currentSpecies.growingColor;
        else if(currentState == TileState.Flowering) sr.color = currentSpecies.flowerColor;
        else if(currentState == TileState.Mature) sr.color = currentSpecies.matureColor;
    }
}
