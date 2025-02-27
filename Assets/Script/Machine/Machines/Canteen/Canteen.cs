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
        
    }
}