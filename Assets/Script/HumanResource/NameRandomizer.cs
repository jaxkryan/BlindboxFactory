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
        public override EmployeeName? Pull(IEnumerable<EmployeeName> itemPool) {
            var pool = Requirement.ProcessItemPool(this, itemPool).ToList();

            if (pool.Any()) {
                Debug.LogWarning("Pull cannot be completed due to acting requirement(s).");
                return null;
            }

            EmployeeName? pulled = null;
            int pulledCount = 0;
            while (pulled == null) {
                pulledCount++;
                pulled = Requirement.ProcessPulledItem(this, pool.ToArray().PickRandom());
                if (pulledCount > _requirementFailPullsCount) {
                    return null;
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