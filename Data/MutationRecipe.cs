using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 单条突变规则配置
[System.Serializable]
public class MutationRecipe
{
    public string recipeName;                 // 配方标识（如："水稻-夜间月光突变"）

    [Header("触发主体配置")]
    public SpeciesData sourceSpecies;         // 触发突变的原物种（如：水稻）

    [Header("产出物种配置")]
    public SpeciesData resultSpecies;         // 突变后的新物种（如：云稻）

    [Header("概率与条件")]
    [Range(0f, 1f)] public float successRate = 0.15f; // 触发概率 (15%)

    public bool requireNight = false;         // 是否需要夜间 (20点~次日4点)
    public bool requireRain = false;          // 是否需要下雨
    public string requiredMutationGene = "";  // 是否需要携带特定基因插槽 (如 "SS")
}