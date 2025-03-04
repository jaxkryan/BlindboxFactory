using System.Collections.Generic;
using System.Linq;
using Script.Machine;
using Script.Machine.Machines.Generator;
using Script.Resources;
using UnityEngine;

namespace Script.Controller {
    public class PowerGridController : ControllerBase{
        public override void Load() { throw new System.NotImplementedException(); }
        public override void Save() { throw new System.NotImplementedException(); }

        public override void OnStart() {
            base.OnStart();
            
            if (GameController.Instance.MachineController == null) return;
            GameController.Instance.MachineController.onMachineAdded += onMachineAdded;
            GameController.Instance.MachineController.onMachineRemoved += onMachineRemoved;
        }
        public override void OnEnable() {
            base.OnStart();
            
            if (GameController.Instance.MachineController == null) return;
            GameController.Instance.MachineController.onMachineAdded += onMachineAdded;
            GameController.Instance.MachineController.onMachineRemoved += onMachineRemoved;
        }

        public override void OnDisable() {
            base.OnDisable();
            
            if (GameController.Instance.MachineController == null) return;
            GameController.Instance.MachineController.onMachineAdded -= onMachineAdded;
            GameController.Instance.MachineController.onMachineRemoved -= onMachineRemoved;
        }

        private IEnumerable<Generator> _generators =>
            GameController.Instance.MachineController.FindMachinesOfType(typeof(Generator)).Cast<Generator>();

        public int GridCapacity => _generators.Select(g => g.Power).Sum();

        public int EnergyUsage =>
            GameController.Instance.MachineController
                .Machines.SelectMany(m => m.ResourceUse).Where(r => r.Resource == Resource.Energy)
                .Select(r => r.Amount).Sum();
        
        private void onMachineAdded(MachineBase machine) {
            ResetMachineEnergyUsage();
        }

        private void onMachineRemoved(MachineBase machine) {
            ResetMachineEnergyUsage();
            
        }
        
        private void ResetMachineEnergyUsage(){
            var list = new List<MachineBase>();
            list.AddRange(GameController.Instance.MachineController.Machines.OrderByDescending(m => m.PlacedTime));
            foreach (var m in list) {
                m.SetMachineHasEnergyForWork(true);
            }
            while (GridCapacity < EnergyUsage) {
                if (!list.Any()) break;
                var m = list.First();
                list.Remove(m);

                m.SetMachineHasEnergyForWork(false);
            }}

    }
}