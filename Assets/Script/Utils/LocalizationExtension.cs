using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace Script.Utils
{
    public static class LocalizationExtension
    {
        private static int currentLocale = 0; // 0: English, 1: Vietnamese
        private static readonly Dictionary<string, Dictionary<int, string>> translations = new()
        {
            // MachineProgressionPolicy
            ["IncreaseAllMachinesProgressionSpeed"] = new Dictionary<int, string>
            {
                [0] = "Increase All Machines Progression Speed {0}",
                [1] = "Tăng tốc độ tiến triển của tất cả máy móc {0}"
            },
            ["IncreaseMachineProgressionSpeed"] = new Dictionary<int, string>
            {
                [0] = "Increase {0} Progression Speed {1}",
                [1] = "Tăng tốc độ tiến triển của {0} {1}"
            },
            // CoreChangeOnWorkPolicy
            ["DecreaseAllWorkersHunger"] = new Dictionary<int, string>
            {
                [0] = "Decrease All Workers Hunger {0}",
                [1] = "Giảm độ đói của tất cả công nhân {0}"
            },
            ["IncreaseAllWorkersCore"] = new Dictionary<int, string>
            {
                [0] = "Increase All Workers {0} {1}",
                [1] = "Tăng {0} của tất cả công nhân {1}"
            },
            // IncreaseMachineResourceGainPolicy
            ["IncreaseAllResourcesGain"] = new Dictionary<int, string>
            {
                [0] = "Increase all resources gain for Resource Extractor Machines {0}",
                [1] = "Tăng lượng tài nguyên thu được cho các máy khai thác tài nguyên {0}"
            },
            // StorageModificationPolicy
            ["IncreaseAllStoragesCapacity"] = new Dictionary<int, string>
            {
                [0] = "Increase All Storages Capacity {0}",
                [1] = "Tăng dung lượng của tất cả kho lưu trữ {0}"
            },
            // IncreaseGeneratorPowerPolicy
            ["IncreaseAllGeneratorCapacity"] = new Dictionary<int, string>
            {
                [0] = "Increase All Generator Capacity {0}",
                [1] = "Tăng dung lượng của tất cả máy phát điện {0}"
            }
        };

        public static void SetLocale(int locale)
        {
            if (locale == 0 || locale == 1)
            {
                LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[locale];
                currentLocale = locale;
                OnLanguageChanged?.Invoke();
            }
            else
            {
            }
        }

        public static string GetTranslation(string key, params object[] args)
        {
            if (translations.TryGetValue(key, out var localeDict) && localeDict.TryGetValue(currentLocale, out var template))
            {
                return string.Format(template, args);
            }
            return $"Missing translation: {key}";
        }

        public static event System.Action OnLanguageChanged;
    }
}