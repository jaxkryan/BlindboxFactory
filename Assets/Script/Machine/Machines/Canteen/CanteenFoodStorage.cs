using System;
using MyBox;
using UnityEngine;

namespace Script.Machine.Machines.Canteen {
    [DisallowMultipleComponent]
    public class CanteenFoodStorage : MonoBehaviour, IStorage {
        public long MaxCapacity { get => _maxCapacity; }
        [Min(1)][SerializeField] long _maxCapacity;
        public long AvailableMeals { get; private set; }

        public event Action<long> onMealAmountChanged = delegate { };

        public bool TrySetMealAmount(long amount) {
            amount = amount > MaxCapacity ? MaxCapacity : amount;
            amount = amount < 0 ? 0 : amount;
            
            AvailableMeals = amount;
            onMealAmountChanged?.Invoke(amount);
            return true;
        }

        public bool TrySetCapacity(long capacity, bool forceDumpExcessAvailableMeals = false) {
            if (capacity < 0) {
                Debug.LogError("Capacity cannot be negative");
                return false;
            }

            if (forceDumpExcessAvailableMeals && AvailableMeals > capacity) { TrySetMealAmount(capacity); }

            _maxCapacity = capacity;
            return true;
        }

        public Canteen.CanteenData.FoodStorageData Save() =>
            new() {
                MaxCapacity = MaxCapacity,
                AvailableMeals = AvailableMeals
            };

        public void Load(Canteen.CanteenData.FoodStorageData data) {
            _maxCapacity = data.MaxCapacity;
            AvailableMeals = data.AvailableMeals;
        }
    }
}