using System;
using UnityEngine;

// 定义昼夜阶段枚举
public enum DayPhase
{
    Day,
    Night
}
public static class EventBus
{
    //-------------全局时间事件-------------
    public static event Action OnTickTrigger;
    public static void TriggerTick() => OnTickTrigger?.Invoke();
    //-------------时间系统事件-------------
    // 每过一小时触发，传递参数：(当前天数, 当前小时)
    public static event Action<int, int> OnHourChanged;
    public static void TriggerHourChanged(int day, int hour) => OnHourChanged?.Invoke(day, hour);
    // 每过一天触发，传递参数：(新的一天是第几天)
    public static event Action<int> OnDayChanged;
    public static void TriggerDayChanged(int day)=> OnDayChanged?.Invoke(day);
    // 昼夜交替时触发，传递参数：(进入了白天还是黑夜)
    public static event Action<DayPhase> OnPhaseChanged;
    public static void TriggerPhaseChanged(DayPhase phase) => OnPhaseChanged?.Invoke(phase);
    //-------------开花授粉事件----------------
    //传递 LandTile 脚本本身，方便 GridManager 获取其坐标和数据
    public static event Action<LandTile> OnPlantFlowering;
    public static void TriggerPlantFlowering(LandTile tile) => OnPlantFlowering?.Invoke(tile);
    
}