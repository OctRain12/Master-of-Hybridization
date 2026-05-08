using System;
using UnityEngine;

public static class EventBus
{
    //-------------全局时间事件-------------
    public static event Action OnTickTrigger;
    public static void TriggerTick() => OnTickTrigger?.Invoke();

    //-------------开花授粉事件----------------
    //传递 LandTile 脚本本身，方便 GridManager 获取其坐标和数据
    public static event Action<LandTile> OnPlantFlowering;
    public static void TriggerPlantFlowering(LandTile tile) => OnPlantFlowering?.Invoke(tile);
    
}