using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Dynamic;

public class UI_Inventory : MonoBehaviour
{
    public Transform contentTransform;  //存放格子的父节点
    public UI_InventorySlot[] allSlots;

    void Awake()
    {
        //启动时获取父节点下的所有格子
         allSlots = contentTransform.GetComponentsInChildren<UI_InventorySlot>(true);
    }

    void OnEnable()
    {
        
        InventoryManager.OnInventoryChanged += RedRawUI;
        RedRawUI();
    }
    void OnDisable()
    {
        InventoryManager.OnInventoryChanged -= RedRawUI;
    }
    void Start()
    {

    }
    public void RedRawUI()
    {
        if (InventoryManager.Instance == null) return;
        //将种子字典转换为列表方便索引
        var seedList = InventoryManager.Instance.seedInventory.ToList();
        
        //把所有有效数据种子装入格子中，比如20个格子，3种种子，则只计算三个格子，后续格子清空
        for(int i = 0; i < allSlots.Length; i++)
        {
            if(i < seedList.Count)
            {
                //把有效数据提取出来
                var pair = seedList[i];
                //确认下玩家所打的标签
                InventoryManager.Instance.seedTags.TryGetValue(pair.Key,out string tag);
                //更新格子的信息
                allSlots[i].Refresh(pair.Key,pair.Value, tag);
            }
            else
            {
                //空闲格子，并没有这么多实际数据，清空
                allSlots[i].ClearSlot();
            }
        }
    }
}
