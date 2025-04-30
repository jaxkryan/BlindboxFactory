#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Script.Gacha.Machine;
using Script.Utils;
using UnityEngine;

namespace Script.HumanResource
{
    [CreateAssetMenu(fileName = "PortraitRandomizer", menuName = "HumanResource/Portrait Randomizer")]
    public class PortraitRandomizer : ScriptableGachaMachineBase<Sprite>
    {
        public override Sprite? Pull(IEnumerable<Sprite> itemPool)
        {
            var pool = Requirement.ProcessItemPool(this, itemPool).ToList();
            if (!pool.Any())
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
                    return null;
                }
            }
            Pulls++;
            PullHistory.Add(pulled);
            return pulled;
        }

        public Sprite? PullByMascotName(string firstName) => PullByMascotName(firstName, ItemPool);
        public Sprite? PullByMascotName(string firstName, IEnumerable<Sprite> itemPool)
        {
            var pool = itemPool.ToList();
            if (pool.Count < 8)
            {
                //Debug.LogWarning($"PullByMascotName failed: Item pool must have at least 8 items, but only {pool.Count} items provided.");
                return null;
            }

            // Determine the range of indices based on the first name
            List<Sprite> nameSpecificPool = firstName switch
            {
                "Skidibi" => pool.GetRange(0, 2), // Items 0 and 1 (2 items)
                "Bunny" => pool.GetRange(2, 3),   // Items 2, 3, 4 (3 items)
                "Lababa" => pool.GetRange(5, 3),  // Items 5, 6, 7 (3 items)
                _ => new List<Sprite>()
            };

            if (!nameSpecificPool.Any())
            {
                Debug.LogWarning($"PullByMascotName failed: Unknown first name '{firstName}'. Expected 'Skidibi', 'Bunny', or 'Lababa'.");
                return null;
            }

            // Apply requirements if enabled
            var filteredPool = Requirement.ProcessItemPool(this, nameSpecificPool).ToList();
            if (!filteredPool.Any())
            {
                Debug.LogWarning($"PullByMascotName failed: No items in pool after applying requirements for '{firstName}'.");
                return null;
            }

            Sprite? pulled = null;
            int pulledCount = 0;
            while (pulled == null)
            {
                pulledCount++;
                pulled = Requirement.ProcessPulledItem(this, filteredPool.ToArray().PickRandom());
                if (pulledCount > _requirementFailPullsCount)
                {
                    //Debug.LogWarning($"PullByMascotName failed after max attempts due to requirements for '{firstName}'.");
                    return null;
                }
            }

            Pulls++;
            PullHistory.Add(pulled);
            return pulled;
        }

        public bool UseGachaRequirements
        {
            get => _useGachaRequirements;
            set => _useGachaRequirements = value;
        }

        [SerializeField] private bool _useGachaRequirements;
    }
}