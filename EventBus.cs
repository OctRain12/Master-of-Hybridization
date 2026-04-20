using System;
using UnityEngine;

public static class EventBus
{
    //-------------全局时间事件-------------
    public static event Action OnTickTrigger;
    public static void TriggerTick() => OnTickTrigger?.Invoke();

    //-------------后续事件----------------
    //public static event Action<Vector2Int> OnplantEnterFlowering;
    //public static void TriggerPlantEnterFlowering(Vector2Int pos) => OnPlantEnterFlowering?.Invoke();
}