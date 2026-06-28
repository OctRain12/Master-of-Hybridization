using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    [Header("时间设置")]
    [Tooltip("游戏内通过1小时，在现实中需要多少秒？")]
    public float realSecondsPerHour = 10f; // 现实中每过多少秒，游戏内时间过1小时

    [Header("当前时间数据 (只读)")]
    public int currentDay = 1;
    public int currentHour = 6; // 默认早上6点开始游戏

    [Header("日夜区间定义")]
    public int nightStartHour = 18; // 18点入夜
    public int dayStartHour = 6;    // 6点天亮

    private float hourTimer = 0f;
    private DayPhase currentPhase = DayPhase.Day;
    // 提供给全局的只读快捷属性
    //public bool IsDay => currentPhase == DayPhase.Day;
    public bool IsNight => currentPhase == DayPhase.Night;
    public static TimeManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // 游戏启动时，初始化一次日夜状态
        currentPhase = (currentHour >= nightStartHour || currentHour < dayStartHour) ? DayPhase.Night : DayPhase.Day;
    }
    void Update()
    {
        UpdateClock();
    }
    //时钟
    private void UpdateClock()
    {
        hourTimer += Time.deltaTime;
        // 当现实秒数达到配置，说明游戏内过去了1小时
        if(hourTimer >= realSecondsPerHour)
        {
            hourTimer = 0f;
            currentHour++;

            // 每过1小时触发事件，天数+
            if (currentHour >= 24)
            {
                currentHour = 0;
                currentDay++;
                EventBus.TriggerDayChanged(currentDay);
            }

            //1. 触发每小时的全局广播 (植物生长会听这个)
            EventBus.TriggerHourChanged(currentDay, currentHour);
            // Debug.Log($"[时间] 第 {currentDay} 天, 此时是：{currentHour}:00 ({currentPhase})");
            // 2. 检测日夜状态切换
            CheckPhaseChange();
        }
    }
    //检测日夜状态切换
    private void CheckPhaseChange()
    {
        DayPhase nextPhase = currentPhase;
        if(currentHour >= nightStartHour || currentHour < dayStartHour)
        {
            nextPhase = DayPhase.Night;
        }
        else
        {
            nextPhase = DayPhase.Day;
        }
        // 如果状态变了，抛出事件
        if(nextPhase != currentPhase)
        {
            currentPhase = nextPhase;
            EventBus.TriggerPhaseChanged(currentPhase);
            Debug.Log($"[时间] 进入了 {(currentPhase == DayPhase.Day ? "白天" : "黑夜")} 阶段");
        }
    }
    /// <summary>
    /// 辅助方法：获取全天时间的 0~1 归一化比例 (用于平滑控制光照颜色)
    /// </summary>
    public float GetTimePercent()
    {
        // 计算当前时间在24小时中的比例
        // 因为没有算分钟，这里加上 hourTimer 的微调，让比例在每小时之间能平滑递增
        float preciseHour = currentHour + (hourTimer / realSecondsPerHour);
        return preciseHour / 24f;
    }
}
