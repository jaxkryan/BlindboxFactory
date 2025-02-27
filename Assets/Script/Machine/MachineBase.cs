using System;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using MyBox;
using Script.HumanResource.Worker;
using Script.Machine.Products;
using Script.Resources;
using UnityEngine;

namespace Script.Machine {
    [DisallowMultipleComponent]
    public abstract class MachineBase : MonoBehaviour, IMachine, IBuilding {
        public int PowerUse { get => _powerUse; set => _powerUse = value; }
        [SerializeField] private int _powerUse = 0;
        public List<ResourceManager.ResourceUse> ResourceUse {
            get => _product.ResourceUse;
        }
        [Obsolete]
        // private ResourceManager.ResourceManager _resourceManager;
        
        public float ProgressionPerSec {
            get {
                var avg = 0f;
                _progressQueue.ForEach(p => avg = (p + avg) / 2);
                return avg;
            }
            set => _progressQueue = new Queue<float>(new []{value}) ;
        }

        private Timer _progressPerSecTimer;
        private Queue<float> _progressQueue = new();

        public float EstimateCompletionTime {
            get => (MaxProgress - CurrentProgress) / ProgressionPerSec;
        }

        public bool IsClosed {
            get => _isClosed;
            set {
                _isClosed = value;
                onMachineCloseStatusChanged?.Invoke(value);
            }
        }

        public event Action<bool> onMachineCloseStatusChanged = delegate { };
        [SerializeField] private bool _isClosed;

        public IEnumerable<MachineSlot> Slots {
            get => _slot;
        }

        [SerializeField] private List<MachineSlot> _slot;

        public float CurrentProgress {
            get => _currentProgress;
            set {
                _currentProgress = value;
                if (!(CurrentProgress >= MaxProgress)) return;
                CurrentProgress -= MaxProgress;
                CreateProduct();
            }
        }
        

        private float _currentProgress;

        public float MaxProgress {
            get => Product.MaxProgress;
        }

        [SerializeField] float _maxProgress;

        public IEnumerable<IWorker> Workers {
            get => _slot.Select(s => s.CurrentWorker).Where(w => w != null);
        }

        public virtual void AddWorker(IWorker worker, MachineSlot slot) {
            if (IsClosed) {
                Debug.LogWarning($"Machine({name}) is closed.");
                return;
            }
            
            if (Workers.Count() >= Slots.Count()) {
                Debug.LogWarning($"Machine({name}) is full.");
                return;
            }

            if (Workers.Contains(worker)) {
                string str = "";
                if (worker is MonoBehaviour monoWorker) str = $"({monoWorker.name})";
                Debug.LogWarning($"Worker{str} is already working on machine({str}).");
                return;
            }

            if (IsClosed) {
                Debug.LogWarning($"Machine({name}) is closed.");
                return;
            }

            if (Slots.All(s => s != slot)) { Debug.LogWarning($"Slots don't belong to machine({name})."); }

            slot.SetCurrentWorker(worker);
            WorkDetails.Where(d => d.CanExecute()).ForEach(d => d.Start());
            onWorkerChanged?.Invoke();
        }

        public virtual void AddWorker(IWorker worker) => AddWorker(worker, Slots.First(s => s.CurrentWorker == null));

        public virtual void RemoveWorker(IWorker worker) {
            if (!Workers.Contains(worker)) {
                string str = "";
                if (worker is MonoBehaviour monoWorker) str = $"({monoWorker.name})";
                Debug.LogWarning($"Worker{str} is not working on machine({str}).");
                return;
            }

            Slots.Where(s => s.CurrentWorker?.Equals(worker) ?? false).ForEach(s => s.SetCurrentWorker());
            WorkDetails.Where(d => !d.CanExecute()).ForEach(d => d.Stop());
            onWorkerChanged?.Invoke();
        }

        public IEnumerable<WorkDetail> WorkDetails {
            get => _workDetails;
        }

        [SerializeReference, SubclassSelector] private List<WorkDetail> _workDetails;

        public virtual ProductBase Product {
            get => _product;
            set {
                onProductChanged?.Invoke(value);
                _product = value;
            }
        }
        public event Action<ProductBase> onProductChanged = delegate { };
    
        [SerializeReference, SubclassSelector] private ProductBase _product;
        public event Action<ProductBase> onCreateProduct = delegate { };
        public DateTimeOffset PlacedTime { get => _placedTime;  }
        private DateTimeOffset _placedTime = DateTimeOffset.Now;
        
        public void SetMachinePlacedTime(DateTimeOffset time) => _placedTime = time;
        
        public virtual ProductBase CreateProduct() {
            _product?.OnProductCreated();
            onCreateProduct?.Invoke(_product);
            return _product;
        }

        public void IncreaseProgress(float progress) {
            CurrentProgress += progress;
            onProgress?.Invoke(progress);
            _progressQueue.Enqueue(progress);
        }

        public event Action<float> onProgress = delegate { };

        public event Action onWorkerChanged = delegate { };

        protected virtual void Awake() {
            WorkDetails.ForEach(d => d.Machine = this);
            _progressPerSecTimer = new CountdownTimer(1);
            // _resourceManager = new();
        }

        protected virtual void Start() {
            #region Progression
            _progressPerSecTimer.OnTimerStop += () => {
                var diff = 0f;
                if (_progressQueue.Any()) {
                    var last = _progressQueue.Last();
                    if (CurrentProgress < last)
                        diff = CurrentProgress + (MaxProgress - last);
                    else diff = CurrentProgress - last;
                }

                _progressQueue.Enqueue(diff);
                if (_progressQueue.Count > 10) _progressQueue.Dequeue();
                _progressPerSecTimer.Start();
            };

            _progressPerSecTimer.Start();
            #endregion

            #region Resource
            // _resourceManager.SetResourceUses(ResourceUse.ToArray());

            #endregion
        }

        protected virtual void Update() {
            _progressPerSecTimer.Tick(Time.deltaTime);
            WorkDetails.ForEach(d => d.Update(Time.deltaTime));
        }
    }
}