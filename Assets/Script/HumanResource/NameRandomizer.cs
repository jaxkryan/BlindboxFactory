#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Script.Gacha.Machine;
using Script.Utils;
using UnityEngine;
using UnityEngine.Serialization;

namespace Script.HumanResource {
    [CreateAssetMenu(fileName = "NameRandomizer", menuName = "HumanResource/Name Randomizer")]
    public class NameRandomizer : ScriptableGachaMachineBase<EmployeeName> {
        public override EmployeeName? Pull(IEnumerable<EmployeeName> itemPool)
        {
            var pool = Requirement.ProcessItemPool(this, itemPool).ToList();
            if (!pool.Any()) // Fixed: Return null if pool is empty
            {
                Debug.LogWarning("Pull cannot be completed: No items in pool after requirements.");
                return null;
            }
            EmployeeName? pulled = null;
            int pulledCount = 0;
            while (pulled == null)
            {
                pulledCount++;
                pulled = Requirement.ProcessPulledItem(this, pool.ToArray().PickRandom());
                if (pulledCount > _requirementFailPullsCount)
                {
                    Debug.LogWarning("Pull failed after max attempts due to requirements.");
                    return null; // Or return a default EmployeeName
                }
            }
            Pulls++;
            PullHistory.Add(pulled);
            return pulled;
        }

        public bool UseGachaRequirements {
            get => _useGachaRequirements;
            set => _useGachaRequirements = value;
        }

        [SerializeField] private bool _useGachaRequirements;
    }
}