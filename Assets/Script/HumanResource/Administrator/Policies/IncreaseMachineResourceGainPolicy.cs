using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using Script.Controller;
using Script.HumanResource.Worker;
using Script.Machine;
using Script.Machine.Products;
using Script.Resources;
using UnityEngine;

namespace Script.HumanResource.Administrator.Policies {
    public class IncreaseMachineResourceGainPolicy : Policy {
        [SerializedDictionary("Core Type", "Min | Max")]
        [SerializeField] public SerializedDictionary<Resource, Vector2> Additives; 
        [SerializedDictionary("Core Type", "Min | Max")]
        [SerializeField] public SerializedDictionary<Resource, Vector2> Multiplier; 
        public override void OnAssign() {
            var controller = GameController.Instance.MachineController;
            foreach (var machine in controller.Machines) {
                machine.onCreateProduct += OnProductCreated;
            }
            
            controller.onMachineAdded += OnMachineAdded;
            controller.onMachineRemoved += OnMachineRemoved;
        }

        private void OnMachineRemoved(MachineBase machine) {
            machine.onCreateProduct -= OnProductCreated;
        }

        private void OnMachineAdded(MachineBase machine) {
            machine.onCreateProduct += OnProductCreated;
        } 
        
        List<Resource> Resources => Additives.Select(a => a.Key).Union(Multiplier.Select(a => a.Key)).ToList();
        
        private void OnProductCreated(ProductBase product) {
            if (product is not AddResourceToStorageProduct addResourceToStorageProduct
                || !Resources.Contains(addResourceToStorageProduct.Resource)) return;

            var controller = GameController.Instance.ResourceController;
            if (!controller.TryGetAmount(addResourceToStorageProduct.Resource, out var current)) return;

            var newAmount = addResourceToStorageProduct.Amount;
            if (Additives.TryGetValue(addResourceToStorageProduct.Resource, out var add)) newAmount += (int)PickRandom(add);
            if (Multiplier.TryGetValue(addResourceToStorageProduct.Resource, out var mul)) newAmount *= (int)PickRandom(mul);
            
            controller.TrySetAmount(addResourceToStorageProduct.Resource, current + newAmount);
        }

        protected override void ResetValues() {
            var controller = GameController.Instance.MachineController;

            foreach (var machine in controller.Machines) {
                OnMachineRemoved(machine);
            }
            
            controller.onMachineAdded -= OnMachineAdded;
            controller.onMachineRemoved -= OnMachineRemoved;
        }

        private float PickRandom(Vector2 range) => new Unity.Mathematics.Random().NextFloat(range.x, range.y);
    }
}