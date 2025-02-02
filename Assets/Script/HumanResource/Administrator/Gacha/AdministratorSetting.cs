using System;
using System.Collections.Generic;
using Script.Gacha.Machine;
using UnityEngine;

namespace Script.HumanResource.Administrator {
    [Serializable]
    public class AdministratorSetting : ILootboxSettings<Administrator> {
        public float Rate { get => _rate; }
        [SerializeField] [Range(0,100)] public float _rate;
        public IEnumerable<Administrator> ItemPool { get => _itemPool; }
        [SerializeField] private List<Administrator> _itemPool;
        [SerializeField] public float hrWeight = 1f;
        [SerializeField] public float facilityWeight = 1f;
        [SerializeField] public float supplyWeight = 1f;
        [SerializeField] public float financeWeight = 1f;

        [SerializeReference, SubclassSelector] public List<IItemRequirement<EmployeeName>> nameRequirements;
        [SerializeReference, SubclassSelector] public List<IItemRequirement<Sprite>> portraitRequirements;
        

    }
}