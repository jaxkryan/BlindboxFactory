using System;
using Script.Gacha.Base;
using Script.Gacha.Machine;
using UnityEngine;

namespace Script.Utils {
    public static class LootboxExtensions {
        
        public static Grade PullGrade<TGacha, TGachaSettings>(this ILootbox<TGacha, TGachaSettings> lootbox) where TGacha : class, ILoot where TGachaSettings : ILootboxSettings<TGacha> {
            float totalWeight = lootbox.CommonSettings.Rate + lootbox.RareSettings.Rate + lootbox.SpecialSettings.Rate + lootbox.EpicSettings.Rate + lootbox.LegendarySettings.Rate;
            if (!Mathf.Approximately(totalWeight, 100f))Debug.LogWarning("Total rate is not 100%");
            var r = new System.Random();

            var randomNumber = (float)r.Next(0, (int)totalWeight);

            var currentSettings = lootbox.CommonSettings;
            var grade = Grade.Common;

            while (randomNumber >= currentSettings.Rate)
            {
                randomNumber -= currentSettings.Rate;
                ToNextGrade(lootbox, ref currentSettings, ref grade);
            }

            return grade;
        }

        public static Grade PullGrade<TGacha, TGachaSettings>(this ILootbox<TGacha, TGachaSettings> lootbox, params Grade[] grades)
            where TGacha : class, ILoot where TGachaSettings : ILootboxSettings<TGacha> {
            float totalWeight = 0f;
            grades.ForEach(g => {
                switch (g) {
                    case Grade.Common:
                        totalWeight += lootbox.CommonSettings.Rate;
                        break;
                    case Grade.Rare:
                        totalWeight += lootbox.RareSettings.Rate;
                        break;
                    case Grade.Special:
                        totalWeight += lootbox.SpecialSettings.Rate;
                        break;
                    case Grade.Epic:
                        totalWeight += lootbox.EpicSettings.Rate;
                        break;
                    case Grade.Legendary:
                        totalWeight += lootbox.LegendarySettings.Rate;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(g), g, null);
                }
            });
            var r = new System.Random();

            var randomNumber = (float)r.Next(0, (int)totalWeight);

            var currentSettings = lootbox.CommonSettings;
            var grade = Grade.Common;

            randomNumber -= currentSettings.Rate;
            while (randomNumber > 0) {
                randomNumber -= currentSettings.Rate;
                ToNextGrade(lootbox, ref currentSettings, ref grade);
            }
            
            return grade;

        }

        private static void ToNextGrade<TGacha, TGachaSettings>(ILootbox<TGacha, TGachaSettings> lootbox, ref TGachaSettings currentSettings, ref Grade grade) where TGacha : class, ILoot where TGachaSettings : ILootboxSettings<TGacha> {
            if (currentSettings.Equals(lootbox.CommonSettings)) currentSettings = lootbox.RareSettings;
            else if (currentSettings.Equals(lootbox.RareSettings)) currentSettings = lootbox.SpecialSettings;
            else if (currentSettings.Equals(lootbox.SpecialSettings)) currentSettings = lootbox.EpicSettings;
            else if (currentSettings.Equals(lootbox.EpicSettings)) currentSettings = lootbox.LegendarySettings;

            if (grade != Grade.Legendary) grade++;
        }
    }
}