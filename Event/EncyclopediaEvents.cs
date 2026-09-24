/// <summary>
/// 首次收集点亮某种作物的图鉴事件
/// </summary>
public struct OnSpeciesDiscoveredEvent
{
    public SpeciesData species;
    public OnSpeciesDiscoveredEvent(SpeciesData species) { this.species = species; }
}

/// <summary>
/// 首次成功培育出 aabbcc 终极纯合隐性基因事件
/// </summary>
public struct OnSpeciesPerfectedEvent
{
    public SpeciesData species;
    public OnSpeciesPerfectedEvent(SpeciesData species) { this.species = species; }
}

/// <summary>
/// 购买高阶培育线索事件
/// </summary>
public struct OnCluePurchasedEvent
{
    public SpeciesData species;
    public OnCluePurchasedEvent(SpeciesData species) { this.species = species; }
}
