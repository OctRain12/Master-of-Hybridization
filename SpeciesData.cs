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

    //各阶段对应颜色
    public Color growingColor = Color.green;
    public Color flowerColor = Color.magenta;
    public Color matureColor = Color.yellow;

}
