#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Script.Gacha.Base;
using Script.Gacha.Machine;
using Script.Utils;
using Unity.Burst.Intrinsics;
using UnityEngine;

namespace Script.HumanResource.Administrator {
    [CreateAssetMenu(fileName = "PolicyGacha", menuName = "HumanResource/Administrator/Gacha")]
    public class PolicyGacha : ScriptableGachaMachineBase<Policy>, ILootbox<Policy, PolicySettings> {
        public override Policy? Pull(IEnumerable<Policy> itemPool) {
            var pool = Requirement.ProcessItemPool(this, itemPool).ToList();

            if (pool.Any()) {
                Debug.LogWarning("Pull cannot be completed due to acting requirement(s).");
                return null;
            }

            Policy? pull = null;
            int pullCount = 0;
            while (pull == null) {
                pullCount++;
                pool = pool.Shuffle().ToList();
                var weightedPool = new List<WeightedOption<Policy>>();
                pool.ForEach(item => {
                    var settings = this.GetSettingsByGrade(item.Grade);
                    var option = new WeightedOption<Policy>() { Option = item, Weight = settings.Rate };
                    weightedPool.Add(option);
                });

                pull = Requirement.ProcessPulledItem(this, weightedPool.ToArray().PickRandom());
                if (pullCount > _requirementFailPullsCount) {
                    Debug.LogWarning("Pull cannot be completed due to acting requirement(s).");
                    return null;
                }
            }

            Pulls++;
            PullHistory.Add(pull);
            return pull;
        }

        public Policy? PullFromGrade(Grade grade, bool allowsLower = false) {
            var pool = new List<Policy>();
            switch (grade) {
                case Grade.Legendary:
                    pool.AddRange(ItemPool.Where(item => item.Grade == Grade.Legendary));
                    if (!allowsLower) goto case Grade.Epic;
                    break;
                case Grade.Epic:
                    pool.AddRange(ItemPool.Where(item => item.Grade == Grade.Epic));
                    if (!allowsLower) goto case Grade.Special;
                    break;
                case Grade.Special:
                    pool.AddRange(ItemPool.Where(item => item.Grade == Grade.Special));
                    if (!allowsLower) goto case Grade.Rare;
                    break;
                case Grade.Rare:
                    pool.AddRange(ItemPool.Where(item => item.Grade == Grade.Rare));
                    if (!allowsLower) goto case Grade.Common;
                    break;
                case Grade.Common:
                    pool.AddRange(ItemPool.Where(item => item.Grade == Grade.Common));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(grade), grade, null);
            }

            return Pull(pool);
        }

        public IEnumerable<Policy> PullByAdminGrade(Grade grade) {
            List<Policy> pulledPolicies = new();
            var settings = this.GetSettingsByGrade(grade);
            var currentLevel = 0;
            var currentPolicies = 0;

            if (settings.MaximumPolicies == 0 || settings.MaximumTotalLevel < CommonSettings.GradeLevel) {
                Debug.LogWarning($"Maximum total level is less than common item level");
                return pulledPolicies;
            }

            while (true) {
                //if level and policies is within range
                if ((currentLevel >= settings.MinimumTotalLevel && currentLevel <= settings.MaximumTotalLevel)
                    && (currentPolicies >= settings.MinimumPolicies && currentPolicies <= settings.MaximumPolicies)) {
                    var grades = Enum.GetValues(typeof(Grade)).Cast<Grade>().ToList();
                    grades.Reverse();
                    foreach (var g in grades) {
                        if (currentLevel + this.GetSettingsByGrade(g).GradeLevel <= settings.MaximumPolicies
                            && currentPolicies + 1 <= settings.MaximumPolicies) {
                            var newPull = PullFromGrade(g, true);
                            if (newPull) pulledPolicies.Add(newPull);
                            break;
                        }
                    }

                    break;
                }
                //if level or policies exceed maximum
                else if (currentLevel > settings.MaximumTotalLevel || currentPolicies > settings.MaximumPolicies) {
                    var p = pulledPolicies.OrderBy(e => e.Grade).First();
                    pulledPolicies.Remove(p);
                    PullHistory.Remove(p);
                    Pulls--;
                }
                else {
                    var newPull = Pull();
                    if(newPull) pulledPolicies.Add(newPull);
                }
            }

            return pulledPolicies;
        }

        public PolicySettings CommonSettings {
            get => _commonSettings;
        }

        [SerializeField] private PolicySettings _commonSettings = new();

        public PolicySettings RareSettings {
            get => _rareSettings;
        }

        [SerializeField] private PolicySettings _rareSettings = new();

        public PolicySettings SpecialSettings {
            get => _specialSettings;
        }

        [SerializeField] private PolicySettings _specialSettings = new();

        public PolicySettings EpicSettings {
            get => _epicSettings;
        }

        [SerializeField] private PolicySettings _epicSettings = new();

        public PolicySettings LegendarySettings {
            get => _legendarySettings;
        }

        [SerializeField] private PolicySettings _legendarySettings = new();
    }
}