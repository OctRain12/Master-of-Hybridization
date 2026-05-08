using UnityEngine;

public class PlantInstanceData
{
    public SpeciesData speciesTemplate; // 指向在 Unity 中配置的 ScriptableObject (比如“小麦模板”)
    public GenoType dna;                // 这株植物独一无二的基因序列

    //构造函数，当播种种子时，把模板和dna数据塞进去
    public PlantInstanceData(SpeciesData template, GenoType plantdna)
    {
        speciesTemplate = template;
        dna = plantdna;
    }

    // ==========================================
    // 动态计算：表现型 = 模板基础值 * 基因加成
    // ==========================================

    // 计算实际需要的生长时间 (Tick 数)
    public int GetActualGrowingTicks()
    {
        // 获取基础时间，乘以基因带来的减免，然后四舍五入
        float calcTicks = speciesTemplate.growingTicks * dna.GetSpeedModifier();
        return Mathf.RoundToInt(calcTicks);
    }
    // 计算实际的授粉期时长 (假设授粉期也受速度基因影响，或者你可以让它固定)
    public int GetActualFloweringTicks()
    {
        float calcTicks = speciesTemplate.flowerTicks * dna.GetSpeedModifier();
        // 保证授粉期最短也得有 1 个 Tick，不然错过了就尴尬了
        return Mathf.Max(1, Mathf.RoundToInt(calcTicks));
    }
    // 计算成熟后实际产出的果实/作物数量
    public int GetActualHarvestQuantity()
    {
        // 假设物种模板基础产量是 2，极品基因系数是 2.0，最终就会掉落 4 个。
        
        // return Mathf.RoundToInt(speciesTemplate.baseYieldCount * dna.GetYieldModifier()); 
        return 1;
    }

    // 检查是否有特定突变 (方便后期替换模型特效)
    public bool HasMutation(string mutationName)
    {
        return dna.specialMutation == mutationName;
    }
}
