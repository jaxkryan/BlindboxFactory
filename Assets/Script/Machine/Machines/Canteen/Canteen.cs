using System.Linq;
using UnityEngine;

namespace Script.Machine.Machines.Canteen {
    [RequireComponent(typeof(CanteenFoodStorage))]
    public class Canteen : MachineBase {
        private CanteenFoodStorage _storage;
        protected override void Awake() {
            base.Awake();
            _storage = GetComponent<CanteenFoodStorage>();
            _storage.onMealAmountChanged += (amount) => {
                IsClosed = _storage.AvailableMeals <= 0;
            };
        }

        private int _lockedMeals = 0;

        private void LockMeal() {
            if (_lockedMeals < 0) Debug.LogError("Number of locked meals cannot be less than 0!");
            var count = Workers.Count();
            var needMeals = _lockedMeals - count;

            if (_storage.AvailableMeals < needMeals) {
                Debug.LogError("Kitchen runs out of meals");
                _hasMeals = false;
            }
            else _hasMeals = true;
            _storage.TryChangeMealAmount(-needMeals);
            

            _lockedMeals = count;
        }

        private bool _hasMeals = true;
        public override bool HasResourceForWork => base.HasResourceForWork && _hasMeals;

        private void UnlockMeals() {
            _storage.TryChangeMealAmount(_lockedMeals);
            _lockedMeals = 0;
        }
    }
}