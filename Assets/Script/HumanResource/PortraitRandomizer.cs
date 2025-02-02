#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Script.Gacha.Machine;
using Script.Utils;
using UnityEngine;

namespace Script.HumanResource {
    [CreateAssetMenu(fileName = "PortraitRandomizer", menuName = "HumanResource/Portrait Randomizer")]
    public class PortraitRandomizer : ScriptableGachaMachineBase<Sprite> {
        public override Sprite? Pull(IEnumerable<Sprite> itemPool) {
            var pool = Requirement.ProcessItemPool(this, itemPool).ToList();

            if (pool.Any()) {
                Debug.LogWarning("Pull cannot be completed due to acting requirement(s).");
                return null;
            }

            Sprite? pulled = null;
            int pulledCount = 0;
            while (pulled == null) {
                pulledCount++;
                pulled = Requirement.ProcessPulledItem(this, pool.ToArray().PickRandom());
                if (pulledCount > _requirementFailPullsCount) {
                    Debug.LogWarning("Pull cannot be completed due to acting requirement(s).");
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