using System;
using Script.Gacha.Base;
using UnityEngine;

namespace Script.Gacha.Machine {
    [Serializable]
    [Tooltip("Guaranteed a specific item after X amount of pulls")]
    public class HardPityGachaRequirement<TItem> : ItemRequirementBase<TItem> where TItem : Loot {
        [Tooltip("Pulls before a pity is achieved")]
        [SerializeField] private int _pityCount = 1;
        [Tooltip("The pity item")]
        [SerializeField] private TItem _pity;

        protected override void OnProcessPulledItem(IGachaMachine<TItem> machine, ref TItem item) {
            base.OnProcessPulledItem(machine, ref item);
            if (machine.Pulls % _pityCount != 0) return;
            item = _pity;
        }
    }
}