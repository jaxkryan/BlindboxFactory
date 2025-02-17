using System;
using System.Collections.Generic;
using System.Linq;
using Script.Gacha.Base;
using UnityEngine;

namespace Script.Gacha.Machine {
    [Serializable]
    [Tooltip("Guaranteed a Legendary grade item after X amount of pulls")]
    public class PityGachaRequirement<TItem> : ItemRequirementBase<TItem> where TItem : Loot {
        [Tooltip("Pulls before a pity is achieved")]
        [SerializeField] private int _pityCount = 1;

        protected override void OnProcessItemPool(IGachaMachine<TItem> machine, ref IEnumerable<TItem> items) {
            if (machine.Pulls % _pityCount != 0) return;
            List<TItem> removePool = new(); 
            var itemPool = items.ToList();
            itemPool.ForEach(i => {
                if (i.Grade != Grade.Legendary) removePool.Add(i);
            });
            removePool.ForEach(i => itemPool.Remove(i));
            items = itemPool;
        }
    }
}