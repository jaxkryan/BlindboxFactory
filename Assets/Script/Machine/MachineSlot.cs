// #nullable enable
using ZLinq;
using JetBrains.Annotations;
using MyBox;
using Script.Controller;
using Script.HumanResource.Worker;
using Script.Utils;
using UnityEngine;

namespace Script.Machine {
    [DisallowMultipleComponent]
    public class MachineSlot : MonoBehaviour {
        [CanBeNull] public Worker CurrentWorker { get => _currentWorker; }
        [SerializeField] [CanBeNull] private Worker _currentWorker;
        [CanBeNull] public Worker WishListWorker  {get => _wishListWorker; private set => _wishListWorker = value; }
        [SerializeField] [CanBeNull] private Worker _wishListWorker;
        public bool FlipWorker  {get => _flipWorker; set => _flipWorker = value; }
        [SerializeField] private bool _flipWorker; 
        
        private CountdownTimer _wishlistTimer;

        [SerializeField] private float _wishlistTravelTimer = 3f;
        [SerializeField] public bool _forAll;
        [ConditionalField("_forAll", true)]
        [SerializeField] private CollectionWrapperList<WorkerType> _forWorker;
        public MachineBase Machine { get; private set; }

        public bool SetCurrentWorker([CanBeNull] Worker worker = null) {
            // Debug.Log($"Try setting {((Worker)worker)?.name ?? "null"} as the current worker");
            if (worker is not null) {
                if (CurrentWorker == worker) return true;
                if (CurrentWorker is not null) {
                    Debug.LogWarning($"{this.name} slot is occupied!");
                    return false;
                }
                if (!CanAddWorker(worker)) {
                    var type = worker is Worker monoWorker ? $"({monoWorker.name})" : "";
                    Debug.LogWarning($"{worker.Name}{type} cannot be added to this slot!");
                    return false;
                }

                if (WishListWorker != worker && WishListWorker is not null) {
                    Debug.LogWarning($"{this.name} slot is wish listed by {((Worker)WishListWorker).name}!");
                    return false;
                }
            }
            _currentWorker?.StopWorking();
            _currentWorker = worker;
            if (worker is null) _currentWorker = null;
            _currentWorker?.StartWorking(this);
            if (worker is not null) {
                if (WishListWorker is not null) WishListWorker = null;
            }
            return true;
        }

        public bool CanAddWorker(Worker worker) {
            if (!Machine.IsWorkable) return false;
            
            return _forAll || _forWorker.Value.AsValueEnumerable().Count(w => w == worker.ToWorkerType()) > 0;
        }
        
        public bool SetWishlist([CanBeNull] Worker worker = null) {
            if (WishListWorker is not null) {
                WishListWorker.Director.TargetSlot = null;
            }
            if (worker != null) {
                if (WishListWorker != null) {
                    Debug.LogError($"{this.name} slot is wish listed!");
                    return false;
                }

                if (!CanAddWorker(worker)) {
                    var type = worker is Worker monoWorker ? $"({monoWorker.name})" : "";
                    Debug.LogError($"{worker.Name}{type} cannot be added to this slot!");
                    return false;
                }
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