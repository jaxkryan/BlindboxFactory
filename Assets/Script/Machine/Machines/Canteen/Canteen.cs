using System.Linq;
using Script.Machine.MachineDataGetter;
using Script.Utils;
using UnityEngine;

namespace Script.Machine.Machines.Canteen {
    [RequireComponent(typeof(CanteenFoodStorage))]
    public class Canteen : MachineBase {
        private CanteenFoodStorage _storage;
        private CanteenKitchen _kitchen;
        protected override void Awake() {
            base.Awake();
            _storage = GetComponent<CanteenFoodStorage>();
            _storage.onMealAmountChanged += (amount) => {
                IsClosed = _storage.AvailableMeals <= 0;
            };
            if (_storage is null) Debug.LogError("Cannot find Food storage"); 
            
            _kitchen = GetComponentInChildren<CanteenKitchen>();
            if (_kitchen is null) Debug.LogError("Cannot find Kitchen"); 
        }

        private int _lockedMeals = 0;

        private void LockMeal() {
            if (_lockedMeals < 0) Debug.LogError("Number of locked meals cannot be less than 0!");
            var count = Workers.Count();
            var needMeals = _lockedMeals - count;
            _storage.TryChangeMealAmount(-needMeals);
            
            _lockedMeals = count;
        }
        public override bool HasResourceForWork => base.HasResourceForWork && _lockedMeals > Workers.Count();

        protected override void TryPullResource() {
            base.TryPullResource();
            LockMeal();
        }

        protected override void UnlockResource() {
            base.UnlockResource();
            UnlockMeals();
        }

        private void UnlockMeals() {
            _storage.TryChangeMealAmount(_lockedMeals);
            _lockedMeals = 0;
        }

        public override void Load(MachineBaseData data) {
            base.Load(data);
            if (data is not CanteenData saveData) return;
            _storage.Load(saveData.Storage);
            _kitchen.Load(saveData.Kitchen);
        }

        public override MachineBaseData Save() {
            var data = base.Save().CastToSubclass<CanteenData, MachineBaseData>();
            if (data is null) return base.Save();
            
            data.Storage = _storage.Save();
            data.Kitchen = (CanteenData.KitchenData) _kitchen.Save();
            return data;
        }

        public class CanteenData : MachineBase.MachineBaseData {
            public FoodStorageData Storage;
            public KitchenData Kitchen;

            public class FoodStorageData {
                public int MaxCapacity;
                public int AvailableMeals;
            }

            public class KitchenData : MachineBase.MachineBaseData {
                
            }
        }
    }
}