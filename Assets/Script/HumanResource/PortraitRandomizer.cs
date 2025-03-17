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
        public override Sprite? Pull(IEnumerable<Sprite> itemPool)
        {
            var pool = Requirement.ProcessItemPool(this, itemPool).ToList();
            if (!pool.Any()) // Fixed: Return null if pool is empty
            {
                Debug.LogWarning("Pull cannot be completed: No items in pool after requirements.");
                return null;
            }
            Sprite? pulled = null;
            int pulledCount = 0;
            while (pulled == null)
            {
                pulledCount++;
                pulled = Requirement.ProcessPulledItem(this, pool.ToArray().PickRandom());
                if (pulledCount > _requirementFailPullsCount)
                {
                    Debug.LogWarning("Pull failed after max attempts due to requirements.");
                    return null; // Or return a default Sprite
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