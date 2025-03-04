using System;
using System.Collections.Generic;
using Script.Gacha.Machine;
using UnityEngine;

namespace Script.HumanResource.Administrator {
    [Serializable]
    public class AdministratorSetting : ILootboxSettings<Mascot> {
        public float Rate { get => _rate; }
        [SerializeField] [Range(0,100)] public float _rate;
        public IEnumerable<Mascot> ItemPool { get => _itemPool; }
        [SerializeField] private List<Mascot> _itemPool;

        [SerializeReference, SubclassSelector] public List<IItemRequirement<EmployeeName>> nameRequirements;
        [SerializeReference, SubclassSelector] public List<IItemRequirement<Sprite>> portraitRequirements;
        

    }
}