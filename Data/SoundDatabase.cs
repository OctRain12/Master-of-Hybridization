using System;
using System.Collections.Generic;
using UnityEngine;

public enum BGMType
{
    MainMenu,
    Day,
    Night
}

public enum SFXType
{
    // UI
    UIHover,
    UIClick,
    UICancel,
    // 农场操作
    PlantSeed,
    HarvestFruit,
    HarvestSeed,
    ToolSwitch,
    Flowering,
    MutationSuccess,
    // 经济与图鉴成就
    CoinGain,
    SpendFail,
    ClueUnlocked,
    PerfectAchieved,
    OpenBook,
    Matured
}
[Serializable]
public struct BGMClipEntry
{
    public BGMType type;
    public AudioClip clip;
}

[Serializable]
public struct SFXClipEntry
{
    public SFXType type;
    public List<AudioClip> clips; // 允许配置单个或多个音频片段
}

