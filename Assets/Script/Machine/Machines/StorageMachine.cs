using UnityEngine;

namespace Script.Machine.Machines {
    public class StorageMachine : MachineBase, IStorage {
        public long MaxCapacity {
            get => _maxCapacity;
            set => _maxCapacity = value;
        }
        [SerializeField] private long _maxCapacity;
    }
}