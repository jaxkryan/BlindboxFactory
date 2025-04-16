using System;
using MyBox;
using UnityEngine;

namespace Script.Machine.Machines.Canteen {
    [DisallowMultipleComponent]
    public class CanteenFoodStorage : MonoBehaviour, IStorage {
        public int MaxCapacity { get => _maxCapacity; }
        [SerializeField] int _maxCapacity;
        public int AvailableMeals { get; private set; }

        public event Action<int> onMealAmountChanged = delegate { };

        public bool TryChangeMealAmount(int amount) {
            if (amount > _maxCapacity || amount < 0) return false;

            AvailableMeals = amount;
            onMealAmountChanged?.Invoke(amount);
            return true;

            return true;
        }

        public bool TrySetCapacity(int capacity, bool forceDumpExcessAvailableMeals = false) {
            if (capacity < 0) {
                Debug.LogError("Capacity cannot be negative");
                return false;
            }

            if (forceDumpExcessAvailableMeals && AvailableMeals > capacity) { TryChangeMealAmount(capacity); }

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