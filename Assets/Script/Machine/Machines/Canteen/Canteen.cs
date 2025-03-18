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
    }
}