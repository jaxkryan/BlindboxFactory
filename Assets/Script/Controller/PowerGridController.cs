using System;
using System.Collections.Generic;
using System.Linq;
using Script.Controller.SaveLoad;
using Script.Machine;
using Script.Machine.Machines.Generator;
using Script.Resources;
using UnityEngine;

namespace Script.Controller {
    [Serializable]
    public class PowerGridController : ControllerBase {
        [SerializeField]private List<MachineBase> _registeredMachines = new();

        public void RegisterMachine(MachineBase machine) {
            if (_registeredMachines.Contains(machine)) return;
            _registeredMachines.Add(machine);
            ResetMachineEnergyUsage();
        }

        public void UnregisterMachine(MachineBase machine) {
            if (!_registeredMachines.Contains(machine)) return;
            _registeredMachines.Remove(machine);
            ResetMachineEnergyUsage();
        }

        private IEnumerable<Generator> _generators =>
            _registeredMachines.FindMachinesOfType(typeof(Generator)).Cast<Generator>();
        
        public int GridCapacity => _generators.Select(g => g.Power).Sum();

        public int EnergyUsage =>
            _registeredMachines.Select(m => m.PowerUse).Sum();
        
        private void ResetMachineEnergyUsage(){
            var list = new List<MachineBase>();
            list.AddRange(_registeredMachines.OrderByDescending(m => m.PlacedTime));
            foreach (var m in list) {
                m.SetMachineHasEnergyForWork(true);
            }
            while (GridCapacity < EnergyUsage) {
                if (!list.Any()) break;
                var m = list.First();
                list.Remove(m);

                m.SetMachineHasEnergyForWork(false);
            }}

        public override void Load(SaveManager saveManager) { 
            //throw new System.NotImplementedException(); 
        }
        public override void Save(SaveManager saveManager) { 
            //throw new System.NotImplementedException(); 
        }
    }
}