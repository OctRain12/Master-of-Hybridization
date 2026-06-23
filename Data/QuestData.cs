using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewQuest", menuName = "育种游戏/订单数据")]
public class QuestData : ScriptableObject
{
    public string questID; // 订单名称
    [TextArea] public string questDescription; // 订单描述

    [Header("需求物品")]
    public bool isRequireSeed; // 是需要种子还是果实？
    public SpeciesData targetSpecies; // 目标物种
    [Tooltip("如果是需要种子，这里写目标基因型标记，如 aa; 如果是果实，这里写果实名字")]
    public string targetItemRequirement; 
    public int requiredCount;
    [Header("订单奖励")]
    public int rewardGold;
}
