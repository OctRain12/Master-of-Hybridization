using UnityEngine;

// 核心算法，用于实现作物杂交
public static class BreedingCalculator
{
    /// <summary>
    /// 核心计算器：传入两个植物数据，返回它们产生的下一代基因序列,优先计算是否发生突变
    /// 若发生突变则直接返回突变后的物种基因序列，否则按正常遗传逻辑计算下一代
    /// </summary>
    /// <param name="parentA">本方植物 (母本)</param>
    /// <param name="parentB">相邻植物 (父本)。如果为 null，或者与母本不同种，则触发自交</param>
    /// <returns>计算出的下一代 GenoType</returns>
    public static SeedEntry CalculateNextGeneration(PlantInstanceData parentA, PlantInstanceData parentB)
    {
        // 特殊环境突变检测
        SpeciesData mutatedSpecies = MutationDatabase.Instance?.CheckMutation(
        parentA.speciesTemplate,  
        parentA.dna
        );
        if (mutatedSpecies != null)
        {
            // 突变为新物种，同时继承并组合亲本基因
            GenoType inheritedDNA = NormalNextGeneration(parentA, parentB);
            return new SeedEntry(mutatedSpecies, inheritedDNA);
        }
        // 跨物种杂交检测
        if (parentB != null && parentB.speciesTemplate != parentA.speciesTemplate)
        {
            SpeciesData hybridSpecies = HybridDatabase.Instance?.TryGetHybridResult(
            parentA.speciesTemplate, 
            parentB.speciesTemplate
        );
            if (hybridSpecies != null)
            {
                // 跨物种杂交成功，返回杂交后新物种的基因序列
                Debug.Log($"杂交成功：{parentA.speciesTemplate.speciesName} + {parentB.speciesTemplate.speciesName} -> {hybridSpecies.speciesName}");
                GenoType hybridDNA = NormalNextGeneration(parentA, parentB);
                return new SeedEntry(hybridSpecies, hybridDNA);
            }
        }
        // 正常同物种杂交/自交
        GenoType standardDNA = NormalNextGeneration(parentA, parentB);
        return new SeedEntry(parentA.speciesTemplate, standardDNA);
    }
    public static GenoType NormalNextGeneration(PlantInstanceData parentA, PlantInstanceData parentB)
    {
        // 1. 处理自交/生殖隔离
        // 如果没有找到合法邻居，或者邻居不是同类，则父本设为自己（自交）
        if(parentB == null || parentB.speciesTemplate != parentA.speciesTemplate)
        {
            parentB = parentA; // 自交
        }
        // 2. 逐一计算三个基础槽位的等位基因组合
        string newSpeed = CombineAlleles(parentA.dna.speedGene, parentB.dna.speedGene);
        string newYield = CombineAlleles(parentA.dna.yieldGene, parentB.dna.yieldGene);
        string newResist = CombineAlleles(parentA.dna.resistGene, parentB.dna.resistGene);

        // 3. 特殊突变槽位继承逻辑
        string newSpecial = InheritSpecialMutation(parentA.dna.specialMutation, parentB.dna.specialMutation); 
        // 默认突变槽位为 "ss"（无突变）
        return new GenoType(newSpeed, newYield, newResist, newSpecial);
    }

    /// <summary>
    /// 模拟孟德尔分离定律：从两个亲本的等位基因中各随机取一个，并组合
    /// 例如：geneA = "Aa", geneB = "aa" -> 返回可能是 "Aa" 或 "aa"
    /// </summary>
    private static string CombineAlleles(string geneA, string geneB)
    {
        // 从亲本A的两个字母中随机挑一个 (Random.Range(0, 2) 会返回 0 或 1)
        char alleleFromA = geneA[Random.Range(0,2)];
        // 从亲本B的两个字母中随机挑一个
        char alleleFromB = geneB[Random.Range(0,2)];
        // 标准化拼接：确保大写字母(显性)永远在小写字母(隐性)前面
        return NormalizeGeneString(alleleFromA, alleleFromB);
    }

    /// <summary>
    /// 字符串标准化，防止出现 "aA" 这种导致背包堆叠失败的脏数据
    /// </summary>
    private static string NormalizeGeneString(char char1, char char2)
    {
        // char.IsUpper 用于判断是否是大写
        if(char.IsUpper(char1) && char.IsLower(char2))
        {
            return $"{char1}{char2}"; // 已经是大+小，直接返回 (如 Aa)
        }
        else if(char.IsLower(char1) && char.IsUpper(char2))
        {
            return $"{char2}{char1}"; // 小+大，反转位置 (如 aA -> Aa)
        }
        else
        {
            return $"{char1}{char2}"; // 全大写或全小写，顺序无所谓 (如 AA, aa)
        }
    }
    /// <summary>
    /// 突变性状的简单遗传逻辑
    /// </summary>
    private static string InheritSpecialMutation(string specA, string specB)
    {
        // 如果双方都没有突变，下一代也没有突变
        if(string.IsNullOrEmpty(specA) && string.IsNullOrEmpty(specB)) return "";

        // 如果双方都有同种突变，100%继承
        if(specA == specB) return specA;

        // 如果只有一方有，或者双方不同，则有50%几率丢失，25%几率随机取一方（模拟非等位冲突）
        float r = Random.value;
        if(r<0.5f)  return ""; // 突变丢失
        else if(r<0.75f) return specA; // 继承母本突变
        else return specB; // 继承父本突变
    }
}
