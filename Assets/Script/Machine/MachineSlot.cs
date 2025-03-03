// #nullable enable
using System.Linq;
using JetBrains.Annotations;
using MyBox;
using Script.Controller;
using Script.HumanResource.Worker;
using UnityEngine;
using UnityEngine.Serialization;

namespace Script.Machine {
    [DisallowMultipleComponent]
    public class MachineSlot : MonoBehaviour {
        [CanBeNull] public IWorker CurrentWorker {get; private set;}
        [CanBeNull] public IWorker WishListWorker  {get; private set;}
        private CountdownTimer _wishlistTimer;

        [SerializeField] private float _wishlistTravelTimer;
        [SerializeField] public bool _forAll;
        [ConditionalField("_forAll", true)]
        [SerializeField] private CollectionWrapperList<WorkerType> _forWorker;
        public MachineBase Machine { get; private set; }

        public bool SetCurrentWorker([CanBeNull] IWorker worker = null) {
            if (worker != null) {
                if (CurrentWorker == worker) return true;
                if (CurrentWorker == null) {
                    Debug.LogError($"{this.name} slot is occupied!");
                    return false;
                }
                if (!CanAddWorker(worker)) {
                    var type = worker is Worker monoWorker ? $"({monoWorker.name})" : "";
                    Debug.LogError($"{worker.Name}{type} cannot be added to this slot!");
                    return false;
                }

                if (WishListWorker != worker) {
                    Debug.LogError($"{this.name} slot is wish listed!");
                    return false;
                }
            }
            CurrentWorker?.StopWorking();
            CurrentWorker = worker;
            CurrentWorker?.StartWorking(this);
            return true;
        }

        public bool CanAddWorker(IWorker worker) {
            if (Machine.IsClosed) return false;
            
            if (_forAll) return true;
            return _forWorker.Value.Any(w => w == IWorker.ToWorkerType(worker));
        }
        
        public bool SetWishlist([CanBeNull] IWorker worker = null) {
            if (WishListWorker == null && worker != null) {
                Debug.LogError($"{this.name} slot is wish listed!");
                return false;
            }

            if (worker != null && !CanAddWorker(worker)) {
                var type = worker is Worker monoWorker ? $"({monoWorker.name})" : "";
                Debug.LogError($"{worker.Name}{type} cannot be added to this slot!");
                return false;
            }
            WishListWorker = worker;
            if (WishListWorker != null) {
                _wishlistTimer.Start();
            }
            else {
                _wishlistTimer.Stop();
            }

            return true;
        }

        private void Update() {
                _wishlistTimer.Tick(Time.deltaTime);
        }

        private void Awake() {
            Machine = GetComponentInParent<MachineBase>();
            _wishlistTimer = new CountdownTimer(_wishlistTravelTimer);
        }

        private void Start() {
            _wishlistTimer.OnTimerStop += () => SetWishlist();
            Machine.onMachineCloseStatusChanged += OnMachineCloseStatusChanged;
        }

        private void OnMachineCloseStatusChanged(bool status) {
            if (status) return;
            SetCurrentWorker();
            SetWishlist();
        }
    }
}