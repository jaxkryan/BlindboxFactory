using System;
using AYellowpaper.SerializedCollections;
using MyBox;
using Script.Machine.Machines.Canteen;
using Script.Resources;
using Script.Utils;
using UnityEngine;

namespace Script.Machine.Products {
    [Serializable]
    public abstract class AddToStorageProduct : SingleProductBase {
        public int Amount { get => _amount; set => _amount = value; }
        [SerializeField] private int _amount;

        public override IProduct.SaveData Save() {
            var data = base.Save().CastToSubclass<AddToStorageSaveData, IProduct.SaveData>();
            if (data is null) return base.Save();
            
            data.Amount = _amount;
            return data;
        }
        
        public override void Load(IProduct.SaveData data) {
            BaseLoad(data);
            if (data is not AddToStorageSaveData saveData) return;
            
            _amount = saveData.Amount;
        }

        public class AddToStorageSaveData : IProduct.SaveData {
            public int Amount;
        }
    }
    
    [Serializable]
    public class KitchenMealProduct : AddToStorageProduct {
        public override bool CanCreateProduct { get => _storage && _storage.MaxCapacity > _storage.AvailableMeals; }
        CanteenFoodStorage _storage => _machine.GetComponentInParent<CanteenFoodStorage>();
        public override void OnProductCreated() {
            if (_storage is null) {
                Debug.LogError($"Cannot find food storage: {_machine.name}");
                return;
            }
            _storage.TryAddMealAmount(Amount);
        }
    }
}