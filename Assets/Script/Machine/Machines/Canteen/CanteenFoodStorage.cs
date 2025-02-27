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
            var newAmount = amount + AvailableMeals;

            if (newAmount > MaxCapacity) {
                if (AvailableMeals >= MaxCapacity) {
                    Debug.LogError("Meal amount exceeds max capacity");
                    return false;
                }
                
                amount = MaxCapacity - AvailableMeals;
                newAmount = amount + AvailableMeals;
            }

            if (newAmount < 0) {
                Debug.LogError("Meal amount cannot be negative");
                return false;
            }

            AvailableMeals = newAmount;
            onMealAmountChanged?.Invoke(amount);
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
    }
}