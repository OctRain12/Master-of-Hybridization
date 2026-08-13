using System.Collections;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
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

    [Header("图标")]
    public Sprite speciesSeedIcon;
    public Sprite speciesFruitIcon;
    //各阶段对应图片
    public Sprite growingSprite;
    public Sprite flowerSprite;
    public Sprite matureSprite;

    public Color growingColor = Color.green;
    public Color flowerColor = Color.magenta;
    public Color matureColor = Color.yellow;

    [Header("价格设置")]
    public int seedPrice;        // 种子价格
    public int fruitPrice;       // 果实价格

    [Header("环境/基因突变进化配置")]
public List<MutationRecipe> possibleMutations; // 该物种可能发生的变异列表
}
