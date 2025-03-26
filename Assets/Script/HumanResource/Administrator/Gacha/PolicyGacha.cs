#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using JetBrains.Annotations;
using MyBox;
using Script.Controller;
using Script.Gacha.Base;
using Script.Gacha.Machine;
using Script.HumanResource.Administrator.Policies;
using Script.HumanResource.Worker;
using Script.Machine;
using Script.Machine.Machines;
using Script.Machine.Machines.Canteen;
using Script.Machine.Machines.Generator;
using Script.Resources;
using Script.Utils;
using UnityEngine;

namespace Script.HumanResource.Administrator
{
    [CreateAssetMenu(fileName = "PolicyGacha", menuName = "HumanResource/Administrator/Gacha")]
    public class PolicyGacha : ScriptableGachaMachineBase<Policy>, ILootbox<Policy, PolicySettings>
    {
        private readonly List<(float coefficient, float probability, Grade grade, int storageAmount, string percentString)> coefficientRates = new()
        {
            (1.02f, 0.40f, Grade.Common, 20, "2"),   // 2% - 40%
            (1.05f, 0.20f, Grade.Rare, 50, "5"),     // 5% - 20%
            (1.10f, 0.15f, Grade.Special, 100, "10"), // 10% - 15%
            (1.15f, 0.10f, Grade.Epic, 150, "15"),    // 15% - 10%
            (1.20f, 0.05f, Grade.Legendary, 200, "20") // 20% - 5%
        };

        private readonly Type[] policyTypes = new Type[]
        {
            typeof(MachineProgressionPolicy),
            typeof(CoreChangeOnWorkPolicy),
            typeof(IncreaseMachineResourceGainPolicy),
            typeof(StorageModificationPolicy),
            typeof(IncreaseGeneratorPowerPolicy)
        };

        public IEnumerable<Policy> GeneratePoliciesForMascot(Grade grade)
        {
            int buffCount = grade switch
            {
                Grade.Common => 1,   // 2*
                Grade.Rare => 2,     // 3*
                Grade.Special => 3,  // 4*
                Grade.Epic => 4,     // 5*
                Grade.Legendary => 5, // 6*
                _ => 0
            };

            var policies = new List<Policy>();
            var usedPolicyTypes = new HashSet<Type>(); // Track used policy types

            for (int i = 0; i < buffCount; i++)
            {
                Policy? policy = null;
                int attempts = 0;
                const int maxAttempts = 10; // Prevent infinite loops

                // Keep trying to generate a policy until we get a unique type
                while (attempts < maxAttempts)
                {
                    policy = GenerateRandomPolicy(grade);
                    if (policy == null) break;

                    // Check if this policy type has already been used
                    if (!usedPolicyTypes.Contains(policy.GetType()))
                    {
                        usedPolicyTypes.Add(policy.GetType());
                        break; // Unique policy type found, exit the loop
                    }

                    // If the policy type is a duplicate, destroy the policy and try again
                    UnityEngine.Object.DestroyImmediate(policy);
                    policy = null;
                    attempts++;
                }

                if (policy != null)
                {
                    policies.Add(policy);
                    Pulls++;
                    PullHistory.Add(policy);
                }
                else
                {
                    Debug.LogWarning($"Failed to generate a unique policy after {maxAttempts} attempts for {grade} mascot");
                }
            }

            Debug.Log($"Generated {policies.Count} policies for {grade} mascot: [{string.Join(", ", policies.Select(p => $"{p.GetType().Name} ({p.Grade})"))}]");
            return policies;
        }

        private Policy? GenerateRandomPolicy(Grade mascotGrade)
        {
            // Pick a random policy type
            var policyType = policyTypes[UnityEngine.Random.Range(0, policyTypes.Length)];
            var policy = ScriptableObject.CreateInstance(policyType) as Policy;
            if (policy == null) return null;

            // Pick a random policy grade (weighted by probabilities in coefficientRates)
            float roll = UnityEngine.Random.value;
            float cumulative = 0f;
            (float coefficient, float probability, Grade policyGrade, int storageAmount, string percentString) rate = coefficientRates[0]; // Default to first rate
            foreach (var r in coefficientRates)
            {
                cumulative += r.probability;
                if (roll <= cumulative)
                {
                    rate = r;
                    break;
                }
            }
            var (coefficient, _, policyGrade, storageAmount, percentString) = rate;
            policy.SetGrade(policyGrade);

            switch (policy)
            {
                case MachineProgressionPolicy mp:
                    mp.Multiplier = new Vector2(coefficient, coefficient);
                    mp.Additives = Vector2.zero;
                    var facility = (MascotType)UnityEngine.Random.Range(0, 6);
                    var machineType = facility switch
                    {
                        MascotType.Generator => typeof(Generator),
                        MascotType.Canteen => typeof(Canteen),
                        MascotType.Restroom => typeof(ResourceExtractor),
                        MascotType.MiningMachine => typeof(ResourceExtractor),
                        MascotType.ProductFactory => typeof(BlindBoxMachine),
                        MascotType.Storage => typeof(StorageMachine),
                        _ => typeof(ResourceExtractor)
                    };
                    var machineInstance = GameController.Instance.MachineController.Machines
                        .FirstOrDefault(m => m.GetType() == machineType);
                    if (machineInstance != null)
                    {
                        mp.SetField("_forAllMachines", false);
                        var machineTypeList = new CollectionWrapperList<MachineBase> { Value = new List<MachineBase> { machineInstance } };
                        mp.SetField("_machineType", machineTypeList);
                        mp.SetField("_description", $"Increase {machineType.Name} Progression Speed by {percentString}%");
                    }
                    else
                    {
                        mp.SetField("_forAllMachines", true);
                        mp.SetField("_description", $"Increase All Machines Progression Speed by {percentString}%");
                    }
                    break;

                case CoreChangeOnWorkPolicy cc:
                    var coreTypes = System.Enum.GetValues(typeof(CoreType)).Cast<CoreType>().ToList();
                    var randomCoreType = coreTypes[UnityEngine.Random.Range(0, coreTypes.Count)];

                    cc.Multiplier = new SerializedDictionary<CoreType, Vector2> { { randomCoreType, new Vector2(coefficient, coefficient) } };
                    cc.Additives = new SerializedDictionary<CoreType, Vector2>();
                    cc.SetField("_forAllWorkers", true);

                    // khong phai increase hunger r nhe
                    if (randomCoreType == CoreType.Hunger)
                    {
                        cc.SetField("_description", $"Decrease All Workers Hunger by {percentString}%");
                    }
                    else
                    {
                        cc.SetField("_description", $"Increase All Workers {randomCoreType} by {percentString}%");
                    }
                    break;

                case IncreaseMachineResourceGainPolicy im:
                    im.Multiplier = new SerializedDictionary<Resource, Vector2>();
                    im.Additives = new SerializedDictionary<Resource, Vector2>();

                    foreach (Resource resource in System.Enum.GetValues(typeof(Resource)))
                    {
                        if (resource == Resource.Gold || resource == Resource.Gem)
                        {
                            continue;
                        }
                        im.Multiplier.Add(resource, new Vector2(coefficient, coefficient));
                    }

                    im.SetField("_description", $"Increase all resources gain for Resource Extractor Machines by {percentString}%");
                    break;

                case StorageModificationPolicy sm:
                    sm.SetField("amount", storageAmount);
                    sm.SetField("_forAllStorages", true);
                    sm.SetField("_description", $"Increase All Storages Capacity by {percentString}%");
                    break;

                case IncreaseGeneratorPowerPolicy gp:
                    gp.SetField("amount", 0); 
                    gp.Multiplier = new Vector2(coefficient, coefficient);
                    gp.Additives = Vector2.zero; 
                    gp.SetField("_forAllGenerators", true);
                    gp.SetField("_description", $"Increase All Generator Capacity by {percentString}%");
                    break;
            }

            return policy;
        }

        private (float coefficient, float probability, Grade grade, int storageAmount, string percentString) PickRandomCoefficient()
        {
            float roll = UnityEngine.Random.value;
            float cumulative = 0f;

            foreach (var rate in coefficientRates)
            {
                cumulative += rate.probability;
                if (roll <= cumulative) return rate;
            }
            return coefficientRates[0];
        }

        public override Policy? Pull(IEnumerable<Policy> itemPool)
        {
            if (itemPool == null || !itemPool.Any())
            {
                Debug.LogWarning("Pull failed: itemPool is null or empty.");
                return null;
            }

            var pool = Requirement.ProcessItemPool(this, itemPool).ToList();
            if (!pool.Any())
            {
                Debug.LogWarning("Pull cannot be completed due to acting requirement(s).");
                return null;
            }

            Policy? pull = null;
            int pullCount = 0;
            while (pull == null)
            {
                pullCount++;
                pool = pool.Shuffle().ToList();
                var weightedPool = new List<WeightedOption<Policy>>();
                pool.ForEach(item =>
                {
                    var settings = this.GetSettingsByGrade(item.Grade);
                    var option = new WeightedOption<Policy> { Option = item, Weight = settings.Rate };
                    weightedPool.Add(option);
                });

                pull = Requirement.ProcessPulledItem(this, weightedPool.ToArray().PickRandom());
                if (pullCount > _requirementFailPullsCount)
                {
                    Debug.LogWarning("Pull cannot be completed due to acting requirement(s).");
                    return null;
                }
            }

            Pulls++;
            PullHistory.Add(pull);
            return pull;
        }

        public Policy? PullFromGrade(Grade grade, bool allowsLower = false)
        {
            var pool = new List<Policy>();
            switch (grade)
            {
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

        public IEnumerable<Policy> PullByAdminGrade(Grade grade)
        {
            List<Policy> pulledPolicies = new();
            var settings = this.GetSettingsByGrade(grade);
            var currentLevel = 0;
            var currentPolicies = 0;
            int maxAttempts = 100;

            if (settings.MaximumPolicies == 0 || settings.MaximumTotalLevel < CommonSettings.GradeLevel)
            {
                Debug.LogWarning($"Maximum total level is less than common item level");
                return pulledPolicies;
            }

            while (maxAttempts-- > 0)
            {
                currentLevel = pulledPolicies.Sum(p => (int)p.Grade);
                currentPolicies = pulledPolicies.Count;

                if (currentLevel >= settings.MinimumTotalLevel && currentLevel <= settings.MaximumTotalLevel
                    && currentPolicies >= settings.MinimumPolicies && currentPolicies <= settings.MaximumPolicies)
                {
                    var grades = Enum.GetValues(typeof(Grade)).Cast<Grade>().ToList();
                    grades.Reverse();
                    foreach (var g in grades)
                    {
                        if (currentLevel + this.GetSettingsByGrade(g).GradeLevel <= settings.MaximumTotalLevel
                            && currentPolicies + 1 <= settings.MaximumPolicies)
                        {
                            var newPull = PullFromGrade(g, true);
                            if (newPull != null)
                            {
                                pulledPolicies.Add(newPull);
                            }
                            break;
                        }
                    }
                    break;
                }
                else if (currentLevel > settings.MaximumTotalLevel || currentPolicies > settings.MaximumPolicies)
                {
                    var p = pulledPolicies.OrderBy(e => e.Grade).First();
                    pulledPolicies.Remove(p);
                    PullHistory.Remove(p);
                    Pulls--;
                }
                else
                {
                    var newPull = Pull();
                    if (newPull != null)
                    {
                        pulledPolicies.Add(newPull);
                    }
                    else
                    {
                        Debug.LogWarning("Failed to pull a policy, breaking loop to prevent freeze.");
                        break;
                    }
                }
            }

            if (maxAttempts <= 0)
            {
                Debug.LogWarning("PullByAdminGrade exceeded maximum attempts, possible configuration issue.");
            }

            return pulledPolicies;
        }

        public PolicySettings CommonSettings => _commonSettings;
        [SerializeField] private PolicySettings _commonSettings = new();

        public PolicySettings RareSettings => _rareSettings;
        [SerializeField] private PolicySettings _rareSettings = new();

        public PolicySettings SpecialSettings => _specialSettings;
        [SerializeField] private PolicySettings _specialSettings = new();

        public PolicySettings EpicSettings => _epicSettings;
        [SerializeField] private PolicySettings _epicSettings = new();

        public PolicySettings LegendarySettings => _legendarySettings;
        [SerializeField] private PolicySettings _legendarySettings = new();
    }

    public static class PolicyExtensions
    {
        public static void SetField(this Policy policy, string fieldName, object value)
        {
            var field = policy.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null) field.SetValue(policy, value);
            else Debug.LogWarning($"Field {fieldName} not found in {policy.GetType().Name}");
        }

        public static void SetGrade(this Policy policy, Grade grade)
        {
            var field = typeof(Gacha.Base.Loot).GetProperty("Grade", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (field != null) field.SetValue(policy, grade);
        }
    }
}