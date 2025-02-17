using System;
using System.Collections.Generic;
using System.Linq;
using Script.Gacha.Machine;
using Script.HumanResource.Administrator;
using UnityEngine;

namespace Script.HumanResource {
    [Serializable]
    public class UseGachaMachineNameListRequirement : ItemRequirementBase<EmployeeName> {
        [SerializeField] AdministratorGacha administratorGacha;

        protected override void OnProcessItemPool(IGachaMachine<EmployeeName> machine,
            ref IEnumerable<EmployeeName> items) {
            base.OnProcessItemPool(machine, ref items);
            var itemPool = items.ToList();
            administratorGacha.Names.ForEach(name => {
                if (!itemPool.Contains(name)) itemPool.Add(name);
            });
            items = itemPool;
        }
    }
}