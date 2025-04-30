using System.Collections.Generic;
using System.Linq;
using MyBox;
using Script.Controller;
using Script.Machine;
using Script.Resources;
using Script.Utils;
using UnityEngine;

namespace Script.HumanResource.Administrator.Policies
{
    [CreateAssetMenu(menuName = "HumanResource/Policies/StorageModificationPolicy")]
    public class StorageModificationPolicy : Policy
    {
        [SerializeField] public Vector2 Multiplier = Vector2.one; 
        [SerializeField] public Vector2 Additives = Vector2.zero; 
        [SerializeField] private bool _forAllStorages;
        [ConditionalField(nameof(_forAllStorages), true)]
        [SerializeField]
        private CollectionWrapperList<StoreHouse> _storages;

        private List<StoreHouse> _appliedMachines = new();

        public override void OnAssign()
        {
            var controller = GameController.Instance.MachineController;
            var list = controller.Machines
                .Where(m => m is StoreHouse)
                .Cast<StoreHouse>();

            if (!_forAllStorages)
            {
                list = list.Where(l => _storages.Value.Any(s => s.GetType() == l.GetType()));
            }

            list.ForEach(ApplyBonus);

            controller.onMachineAdded += ApplyBonus;
        }

        public void ApplyBonus(IMachine machine)
        {
            if (machine is not StoreHouse storeHouse) return;

            if (!_forAllStorages && !_storages.Value.Any(s => s.GetType() == storeHouse.GetType())) return;

            _appliedMachines.Add(storeHouse);
            UpdateCapacity(storeHouse, Multiplier, Additives, apply: true);
        }

        public void UnapplyBonus(IMachine machine)
        {
            if (machine is not StoreHouse storeHouse) return;

            if (_appliedMachines.Remove(storeHouse))
            {
                UpdateCapacity(storeHouse, new Vector2(1f / Multiplier.x, 1f / Multiplier.y), -Additives, apply: false);
            }
        }

        private void UpdateCapacity(StoreHouse storeHouse, Vector2 multiplier, Vector2 additives, bool apply)
        {
            var boxController = GameController.Instance.BoxController;
            var resourceController = GameController.Instance.ResourceController;

            // Lấy giá trị gốc từ StoreHouse
            long originalBoxAmount = storeHouse.boxamount;
            long originalResourceAmount = storeHouse.resorceamount;

            // Tính toán giá trị mới dựa trên multiplier và additives
            long newBoxAmount = apply
                ? (long)(originalBoxAmount * multiplier.x + additives.x)
                : (long)(originalBoxAmount / multiplier.x - additives.x);
            long newResourceAmount = apply
                ? (long)(originalResourceAmount * multiplier.y + additives.y)
                : (long)(originalResourceAmount / multiplier.y - additives.y);

            newBoxAmount = (long) Mathf.Max(0, newBoxAmount);
            newResourceAmount = (long) Mathf.Max(0, newResourceAmount);

            // Cập nhật giá trị trong StoreHouse
            storeHouse.boxamount = newBoxAmount;
            storeHouse.resorceamount = newResourceAmount;

            // Cập nhật BoxController
            boxController.TryGetWarehouseMaxAmount(out var maxBoxAmount);
            long boxDelta = newBoxAmount - originalBoxAmount;
            boxController.TrySetWarehouseMaxAmount(maxBoxAmount + boxDelta);

            // Cập nhật ResourceController
            resourceController.TryGetAllResourceAmounts(out var materials);
            foreach (var res in materials)
            {
                resourceController.TryGetData(res.Key, out var oldResData, out var curAmount);
                long resourceDelta = newResourceAmount - originalResourceAmount;
                ResourceData resourceData = new ResourceData
                {
                    MaxAmount = oldResData.MaxAmount + resourceDelta
                };
                resourceController.TryUpdateData(res.Key, resourceData);
            }

            Debug.Log($"Updated StoreHouse {storeHouse.name}: boxamount={storeHouse.boxamount}, resorceamount={storeHouse.resorceamount}");
        }

        protected override void ResetValues()
        {
            // Hủy bonus cho tất cả StoreHouse đã áp dụng
            _appliedMachines.ToList().ForEach(UnapplyBonus);
            _appliedMachines.Clear();

            var controller = GameController.Instance.MachineController;
            controller.onMachineAdded -= ApplyBonus; // Hủy đăng ký sự kiện
        }

        public override SaveData Save() {
            var data = base.Save().CastToSubclass<StorageModificationSaveData, SaveData>();
            if (data is null) return base.Save();

            data.Additives = new(Additives);
            data.Multiplier = new (Multiplier);
            return data;
        }

        public override void Load(SaveData data) {
            base.Load(data);
            
            if (data is not StorageModificationSaveData coreData) return;

            Additives = coreData.Additives;
            Multiplier = coreData.Multiplier;
        }

        public class StorageModificationSaveData : SaveData {
            public V2 Additives;
            public V2 Multiplier;
        }
    }
}