using System;
using System.Collections.Generic;
using Script.Gacha.Machine;
using UnityEngine;

namespace Script.HumanResource.Administrator {
    [Serializable]
    public class PolicySettings : ILootboxSettings<Policy> {
        public float Rate { get => _rate; }
        [SerializeField] [Range(0f, 100f)] private float _rate;
        public IEnumerable<Policy> ItemPool { get => _itemPool; }
        [SerializeField] private List<Policy> _itemPool;

        [SerializeField] public int GradeLevel;
        [SerializeField] public int MinimumTotalLevel;
        [SerializeField] public int MaximumTotalLevel;
        [SerializeField] public int MinimumPolicies;
        [SerializeField] public int MaximumPolicies;
        [SerializeReference, SubclassSelector] public List<IItemRequirement<Policy>> policyRequirements;
    }
}