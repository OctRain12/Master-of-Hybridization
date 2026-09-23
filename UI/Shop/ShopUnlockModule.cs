using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public static class ShopUnlockModule
{
    public struct UnlockState
    {
        public bool isUnlocked;     // 是否解锁
        public string lockReason;   // 锁定原因
    }

    public static UnlockState GetUnlockState(SpeciesData species)
    {
        UnlockState state = new UnlockState { isUnlocked = false, lockReason = "" };

        switch (species.unlockType)
        {
            case ShopUnlockType.DefaultUnlocked:
                state.isUnlocked = true;
                break;

            case ShopUnlockType.RequireEncyclopedia:
                // 伪代码：对接图鉴系统
                // state.isUnlocked = EncyclopediaManager.Instance.IsDiscovered(species);
                state.isUnlocked = false; // 暂代
                if (!state.isUnlocked) state.lockReason = "图鉴尚未收录";
                break;

            case ShopUnlockType.RequireFruitSold:
                // 伪代码：对接统计系统
                // int currentSold = GameStatisticsManager.Instance.GetFruitSold(species);
                int currentSold = 0; // 暂代
                state.isUnlocked = currentSold >= species.unlockThreshold;
                if (!state.isUnlocked) state.lockReason = $"需累计售卖 {species.speciesName} x{species.unlockThreshold}";
                break;
        }
        return state;
    }
}
