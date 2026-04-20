using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TickManager : MonoBehaviour
{
    [SerializeField]private float tickInterval = 1.0f;  //设置每次事件广播为1秒
    private float timer;
    void Update()
    {
        timer += Time.deltaTime;
        if(timer > tickInterval)
        {
            timer = 0;
            //通过eventBus全局广播
            EventBus.TriggerTick();
        }
    }
}
