using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//创建物种建立菜单
[CreateAssetMenu(fileName ="NewSpecies", menuName = "BreedingGame/SpeciesData")]
public class SpeciesData : ScriptableObject
{
    public string speciesName;
    [Header("生长阶段计数")]
    public int growingTicks;
    public int flowerTicks;
    public GenoType defaultGenoType;
    [Header("产量设置")]
    public int seedCount;                  //这株植物的种子产量，默认4
    public int baseYieldCount;          //基础产量
    //各阶段对应颜色
    public Color growingColor = Color.green;
    public Color flowerColor = Color.magenta;
    public Color matureColor = Color.yellow;

}
