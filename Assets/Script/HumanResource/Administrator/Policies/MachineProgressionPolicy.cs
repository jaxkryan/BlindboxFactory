using System;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using MyBox;
using Script.Controller;
using Script.HumanResource.Worker;
using Script.Machine;
using UnityEngine;

namespace Script.HumanResource.Administrator.Policies {
    [CreateAssetMenu(menuName = "HumanResource/Policies/MachineProgressionPolicy")]
    public class MachineProgressionPolicy : Policy {
        [SerializeField] private bool _forAllMachines;
        [ConditionalField(nameof(_forAllMachines), true)] [SerializeField] private CollectionWrapperList<MachineBase> _machineType;
        [SerializeField] public Vector2 Additives; 
        [SerializeField] public Vector2 Multiplier; 
        public override void OnAssign() {
            var controller = GameController.Instance.MachineController;
            List<MachineBase> machines = _forAllMachines 
                ? controller.Machines.ToList() 
                : controller.Machines.Where(m => _machineType.Value.Any(type => type.GetType() == m.GetType())).ToList();
            machines.ForEach(ApplyBonus);
            
            controller.onMachineAdded += ApplyBonus;
            controller.onMachineRemoved += UnapplyBonus;
        }

        protected override void ResetValues() {
            var controller = GameController.Instance.MachineController;
            _appliedMachines.Clone().ForEach(UnapplyBonus);
        }

        private List<IMachine> _appliedMachines = new();
        
        private void ApplyBonus(IMachine machine) {
            _appliedMachines.Add(machine);
            machine.onProgress += Evt;

            void Evt(float progress) {
                if (_appliedMachines.Contains(machine)) OnProgress(machine, progress);
                else machine.onProgress -= Evt;
            }
        }

        private void UnapplyBonus(IMachine machine) => _appliedMachines.Remove(machine);

        private void OnProgress(IMachine machine, float progress) {
            if (progress <= 0) return;
            
            var changeAmount = 0f;
            changeAmount += PickRandom(Additives);
            changeAmount += PickRandom(Multiplier) * (progress + changeAmount);
            
            machine.CurrentProgress += changeAmount;
        }

        private float PickRandom(Vector2 range) => new Unity.Mathematics.Random().NextFloat(range.x, range.y);
    }
}