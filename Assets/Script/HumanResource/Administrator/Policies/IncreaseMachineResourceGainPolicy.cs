using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using Script.Controller;
using Script.HumanResource.Worker;
using Script.Machine;
using Script.Machine.Products;
using Script.Resources;
using UnityEngine;

namespace Script.HumanResource.Administrator.Policies
{
    [CreateAssetMenu(menuName = "HumanResource/Policies/MachineResourceGainPolicy")]
    public class IncreaseMachineResourceGainPolicy : Policy
    {
        [SerializedDictionary("Core Type", "Min | Max")]
        [SerializeField] public SerializedDictionary<Resource, Vector2> Additives;
        [SerializedDictionary("Core Type", "Min | Max")]
        [SerializeField] public SerializedDictionary<Resource, Vector2> Multiplier;

        public override void OnAssign()
        {
            var controller = GameController.Instance.MachineController;
            foreach (var machine in controller.Machines)
            {
                machine.onCreateProduct += OnProductCreated;
            }

            controller.onMachineAdded += OnMachineAdded;
            controller.onMachineRemoved += OnMachineRemoved;
        }

        private void OnMachineRemoved(MachineBase machine)
        {
            machine.onCreateProduct -= OnProductCreated;
        }

        private void OnMachineAdded(MachineBase machine)
        {
            machine.onCreateProduct += OnProductCreated;
        }

        List<Resource> Resources => Additives.Select(a => a.Key).Union(Multiplier.Select(a => a.Key)).ToList();

        private void OnProductCreated(ProductBase product)
        {
            if (product is not AddResourceToStorageProduct addResourceToStorageProduct)
            {
                return;
            }

            // Use SelectedMaterial if available, otherwise fall back to Resource
            Resource targetResource = addResourceToStorageProduct.SelectedMaterial.HasValue
                ? addResourceToStorageProduct.SelectedMaterial.Value
                : addResourceToStorageProduct.Resource;

            if (!Resources.Contains(targetResource))
            {
                return;
            }

            var controller = GameController.Instance.ResourceController;
            if (!controller.TryGetAmount(targetResource, out var current))
            {
                return;
            }

            // Use SelectedQuantity if available, otherwise fall back to Amount
            int baseAmount = addResourceToStorageProduct.SelectedMaterial.HasValue
                ? addResourceToStorageProduct.SelectedQuantity
                : addResourceToStorageProduct.Amount;

            var newAmount = baseAmount;
            if (Additives.TryGetValue(targetResource, out var add))
            {
                newAmount += (int)PickRandom(add);
            }
            if (Multiplier.TryGetValue(targetResource, out var mul))
            {
                newAmount = (int)(newAmount * PickRandom(mul));
            }

            controller.TrySetAmount(targetResource, current + newAmount);
            Debug.Log($"IncreaseMachineResourceGainPolicy: Adjusted {targetResource} by {newAmount - baseAmount} (total: {newAmount})");
        }

        protected override void ResetValues()
        {
            var controller = GameController.Instance.MachineController;

            foreach (var machine in controller.Machines)
            {
                OnMachineRemoved(machine);
            }

            controller.onMachineAdded -= OnMachineAdded;
            controller.onMachineRemoved -= OnMachineRemoved;
        }

        private float PickRandom(Vector2 range) => new Unity.Mathematics.Random().NextFloat(range.x, range.y);

        public override SaveData Save()
        {
            var data = (IncreaseMachineResourceGainData)base.Save();

            data.Additives = Additives;
            data.Multiplier = Multiplier;

            return data;
        }

        public override void Load(SaveData data)
        {
            if (data is IncreaseMachineResourceGainData increaseData)
            {
                Additives = new(increaseData.Additives);
                Multiplier = new(increaseData.Multiplier);
            }
            base.Load(data);
        }

        public class IncreaseMachineResourceGainData : SaveData
        {
            public Dictionary<Resource, Vector2> Additives;
            public Dictionary<Resource, Vector2> Multiplier;
        }
    }
}