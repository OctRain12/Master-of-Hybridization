using System;
using System.Net.Http.Headers;
using TMPro;
using Unity.VisualScripting;

[Serializable]  // 这个标签，方便以后在 Unity 面板里或者存档文件里看到它
public class GenoType
{
    // ------总共3个基础槽位，采用大写+小写字母表示（AA,Aa,aa）
    // 且约定：如果是杂合子，大写字母必须在前，即永远是 "Aa"，不能是 "aA"，这有助于系统判断相同。
    public string speedGene;    //控制生长速度
    public string yieldGene;    //控制产量
    public string resistGene;   //控制基础抗性

    // ------1个特殊突变插槽
    public string specialMutation; //留空

    public GenoType(string speed, string yieldG, string resist, string special = "")
    {
        speedGene = speed;
        yieldGene = yieldG;
        resistGene = resist;
        specialMutation = special;
    }

    // ==========================================
    // 核心逻辑 1：数值换算 (将字母变成实际的游戏加成系数)
    // ==========================================
    
    // 解析速度基因：A=慢，a=快。返回值是时间乘数（越小越快）
    public float GetSpeedModifier()
    {
        if(speedGene == "aa") return 0.5f;          // 纯隐性：极速！时间减半
        else if(speedGene == "Aa") return 1.0f;     // 杂合：标准时间
        else return 1.5f;                           // 纯显性 (AA)：生长极其缓慢
    }
    //解析产量基因：B=低产，b=高产。返回值是价值乘数
    public float GetYieldModifier()
    {
        if(yieldGene == "bb") return 2.0f;          // 纯隐性：高产
        else if(yieldGene == "Bb") return 1.0f;     // 杂合：标准产量
        else return 0.5f;                           // 纯显性 (BB)：低产
    }
    // 解析抗性基因：C=脆皮，c=皮糙肉厚。
    public float GetResistGene()
    {
        if(resistGene == "cc") return 2.0f;          // 纯隐性：皮糙肉厚
        else if(resistGene == "Cc") return 1.0f;     // 杂合：标准抗性
        else return 0.5f;                           // 纯显性 (CC)：脆皮
    }

    // ==========================================
    // 核心逻辑 2：严格判定 (防污染法则，用于背包堆叠)
    // ==========================================
    // 重写Equals方法
    public override bool Equals(object obj)
    {
        if (obj is GenoType other)
        {
            return this.speedGene == other.speedGene &&
                   this.yieldGene == other.yieldGene &&
                   this.resistGene == other.resistGene &&
                   this.specialMutation == other.specialMutation;
        }
        return false;
    }
    public override int GetHashCode()
    {
        // 将字符串拼接后生成 Hash 码，供字典和 List 高效比对
        return (speedGene + yieldGene + resistGene + specialMutation).GetHashCode();
    }

    // 为了 Debug 方便，重写 ToString
    public override string ToString()
    {
        string specialStr = string.IsNullOrEmpty(specialMutation) ? "无" : specialMutation; //三元运算符（条件 ? 结果1 : 结果2）
        return $"[{speedGene} | {yieldGene} | {resistGene} | 突变:{specialStr}]";
    }
}

