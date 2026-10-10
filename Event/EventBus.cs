using System;
using UnityEngine;

// 定义昼夜阶段枚举
public enum DayPhase
{
    Day,
    Night
}
// 定义交互逻辑
public enum ToolMode
{
    None,            // 普通指针/播种模式
    FruitBasket,     // 果篮（仅收获果实）
    SeedExtractor    // 种子提取器（仅收获种子）
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
    public static event Action<UI_PlotSlot> OnPlantFlowering;
    public static void TriggerPlantFlowering(UI_PlotSlot slot) => OnPlantFlowering?.Invoke(slot);
    // UI 土坑相关事件
    // 土坑点击事件
    public static event Action<UI_PlotSlot> OnPlotClicked;
    public static void TriggerPlotClicked(UI_PlotSlot slot) => OnPlotClicked?.Invoke(slot);
    // 当玩家切换工具模式时触发
    public static event Action<ToolMode> OnToolModeChanged;
    public static void TriggerToolModeChanged(ToolMode newMode) => OnToolModeChanged?.Invoke(newMode);

    //-------------图鉴系统事件-------------
    // 1. 首次收集点亮某种作物的图鉴事件
    public static event Action<OnSpeciesDiscoveredEvent> OnSpeciesDiscovered;
    public static void TriggerSpeciesDiscovered(SpeciesData species) => 
        OnSpeciesDiscovered?.Invoke(new OnSpeciesDiscoveredEvent(species));

    // 2. 首次成功培育出 aabbcc 终极纯合隐性基因事件
    public static event Action<OnSpeciesPerfectedEvent> OnSpeciesPerfected;
    public static void TriggerSpeciesPerfected(SpeciesData species) => 
        OnSpeciesPerfected?.Invoke(new OnSpeciesPerfectedEvent(species));

    // 3. 购买高阶培育线索事件
    public static event Action<OnCluePurchasedEvent> OnCluePurchased;
    public static void TriggerCluePurchased(SpeciesData species) => 
        OnCluePurchased?.Invoke(new OnCluePurchasedEvent(species));


}