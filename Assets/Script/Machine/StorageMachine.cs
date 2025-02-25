using UnityEngine;

namespace Script.Machine {
    public class StorageMachine : MachineBase, IStorage {
        public int MaxCapacity {
            get => _maxCapacity;
            set => _maxCapacity = value;
        }
        [SerializeField] private int _maxCapacity;
    }
}