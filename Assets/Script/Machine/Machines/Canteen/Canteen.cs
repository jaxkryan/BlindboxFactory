using System.Linq;
using Script.Machine.MachineDataGetter;
using Script.Utils;
using UnityEngine;
using static Script.Machine.Machines.Canteen.Canteen.CanteenData;

namespace Script.Machine.Machines.Canteen {
    [RequireComponent(typeof(CanteenFoodStorage))]
    public class Canteen : MachineBase {
        private CanteenFoodStorage _storage;
        private CanteenKitchen _kitchen;
        protected override void Awake() {
            base.Awake();
            _storage = GetComponent<CanteenFoodStorage>();
            if (_storage is null) Debug.LogError("Cannot find Food storage"); 
            
            _kitchen = GetComponentInChildren<CanteenKitchen>();
            if (_kitchen is null) Debug.LogError("Cannot find Kitchen"); 
        }

        public override bool HasResourceForWork { get => base.HasResourceForWork && _storage.AvailableMeals > 0; }

        //
        // [SerializeField]private int _lockedMeals = 0;
        //
        // private void LockMeal() {
        //     if (_lockedMeals < 0) Debug.LogError("Number of locked meals cannot be less than 0!");
        //     var count = Workers.Count() + 1;
        //     var needMeals = _lockedMeals - count;
        //     if (needMeals >= 0) return; 
        //     if (_storage.AvailableMeals < needMeals) return;
        //     _storage.TryChangeMealAmount(-needMeals);
        //     
        //     _lockedMeals = count;
        // }
        //
        // public override bool HasResourceForWork => base.HasResourceForWork && HasMeal();
        //
        // private bool HasMeal() {
        //     if (_lockedMeals <= Workers.Count()) {
        //         LockMeal();
        //     }
        //     return _lockedMeals > Workers.Count();
        // }
        //
        // protected override void TryPullResource() {
        //     base.TryPullResource();
        //     LockMeal();
        // }
        //
        // protected override void UnlockResource() {
        //     base.UnlockResource();
        //     UnlockMeals();
        // }
        //
        // private void UnlockMeals() {
        //     _storage.TryChangeMealAmount(_lockedMeals);
        //     _lockedMeals = 0;
        // }
        //
        // protected override void OnEnable() {
        //     base.OnEnable();
        //     _storage.onMealAmountChanged += OnMealAmountChanged;
        // }
        //
        // protected override void OnDisable() {
        //     base.OnDisable();
        //     _storage.onMealAmountChanged -= OnMealAmountChanged;
        // }
        //
        // private void OnMealAmountChanged(int amount) => LockMeal();

        public override void Load(MachineBaseData data) {
            base.Load(data);
            if (data is not CanteenData saveData) return;
            _storage.Load(saveData.Storage);
        }

        public override MachineBaseData Save() {
            var data = base.Save().CastToSubclass<CanteenData, MachineBaseData>();
            if (data is null) return base.Save();
            
            data.Storage = _storage.Save();
            return data;
        }

        public class CanteenData : MachineBase.MachineBaseData {
            public FoodStorageData Storage;

            public class FoodStorageData {
                public long MaxCapacity;
                public long AvailableMeals;
            }
        }

        public Canteen.CanteenData GetCanteenData()
        {
            var data = new Canteen.CanteenData
            {
                Storage = _storage.Save(),
        };
            return data;
        }
    }
}