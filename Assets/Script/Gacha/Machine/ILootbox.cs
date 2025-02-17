using System;
using System.Collections.Generic;
using Script.Gacha.Base;
using UnityEngine;
using Random = System.Random;

namespace Script.Gacha.Machine {
    public interface ILootbox<TLoot, TGachaSettings> : IRandomMachine<TLoot> where TLoot : class, ILoot where TGachaSettings : ILootboxSettings<TLoot>  {
        TGachaSettings CommonSettings { get; }
        TGachaSettings RareSettings { get; }
        TGachaSettings SpecialSettings { get; }
        TGachaSettings EpicSettings { get; }
        TGachaSettings LegendarySettings { get; }

        
    }

    public static class LootboxExtensions {
        public static TGachaSettings GetSettingsByGrade<TLoot, TGachaSettings>(this ILootbox<TLoot, TGachaSettings> lootbox, Grade grade) where TLoot : class, ILoot where TGachaSettings : ILootboxSettings<TLoot>  {
            return grade switch {
                Grade.Common => lootbox.CommonSettings,
                Grade.Rare => lootbox.RareSettings,
                Grade.Special => lootbox.SpecialSettings,
                Grade.Epic => lootbox.EpicSettings,
                Grade.Legendary => lootbox.LegendarySettings,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}