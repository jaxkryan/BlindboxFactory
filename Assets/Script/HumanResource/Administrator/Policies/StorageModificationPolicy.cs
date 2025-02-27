using System.Collections.Generic;
using System.Linq;
using MyBox;
using Script.Controller;
using Script.Machine;
using Script.Machine.Machines;
using UnityEngine;

namespace Script.HumanResource.Administrator.Policies {
    [CreateAssetMenu(menuName = "HumanResource/Policies/StorageModificationPolicy")]
    public class StorageModificationPolicy : Policy {
        [SerializeField] private int amount;
        [SerializeField] private bool _forAllStorages;
        [ConditionalField(nameof(_forAllStorages), true)] [SerializeField]
        private CollectionWrapperList<StorageMachine> _storages;

        
        public override void OnAssign() {
            var controller = GameController.Instance.MachineController;
            var list = controller.Machines
                .Where(m => m is StorageMachine)
                .Cast<StorageMachine>();
            if (!_forAllStorages) list = list.Where(l => _storages.Value.Any(s => s.GetType() == l.GetType()));
            list.ForEach(ApplyBonus);

            controller.onMachineAdded += ApplyBonus;
        }

        private List<StorageMachine> _appliedMachine = new(); 
        public void ApplyBonus(IMachine machine) {
            if (machine is not StorageMachine storageMachine) return;
            
            _appliedMachine.Add(storageMachine);
            UpdateCapacity(storageMachine, amount);
        }

        public void UnapplyBonus(IMachine machine) {
            if (machine is not StorageMachine storageMachine) return;
            
            _appliedMachine.Remove(storageMachine);
            UpdateCapacity(storageMachine, -amount);
        }
        

        private void UpdateCapacity(StorageMachine storageMachine, int amount) {
            storageMachine.MaxCapacity += amount;
        }

        protected override void ResetValues() {
            _appliedMachine.Clone().ForEach(UnapplyBonus);
            
            var controller = GameController.Instance.MachineController;
        }
    }
}