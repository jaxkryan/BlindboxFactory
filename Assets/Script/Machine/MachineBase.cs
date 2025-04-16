using System;
using System.Collections.Generic;
using System.Linq;
using Script.Controller;
using Script.Controller.SaveLoad;
using Script.HumanResource.Worker;
using Script.Machine.Products;
using Script.Machine.ResourceManager;
using UnityEngine;
using UnityEngine.Serialization;

namespace Script.Machine {
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PolygonCollider2D))]
    public abstract class MachineBase : MonoBehaviour, IMachine, IBuilding {
        public virtual Vector3 Position { get => _position; set => _position = value; }
        [SerializeField] private Vector3 _position;
        public virtual string PrefabName { get; set; }
        

        public int PowerUse {
            get => _powerUse;
            set => _powerUse = value;
        }

        [SerializeField] private int _powerUse = 0;

        public List<ResourceManager.ResourceUse> ResourceUse {
            get => _product?.ResourceUse;
        }

        private ResourceManager.ResourceManager _resourceManager;
        public virtual bool HasResourceForWork {
            get {
                if (_resourceManager == null) return false;
                if (!_resourceManager.HasResourcesForWork(out _)) {
                    _resourceManager.UnlockResource();
                    _resourceManager.TryPullResource(1, out _);
                }
                
                return _resourceManager.HasResourcesForWork(out _);
            }
        }

            public bool CanCreateProduct {
            get => _product.CanCreateProduct;
        }

        public virtual bool IsWorkable {
            get => !_isClosed && HasResourceForWork && HasEnergyForWork && CanCreateProduct;
        }

        public bool HasEnergyForWork { get; private set; }

        public void SetMachineHasEnergyForWork(bool hasEnergy) => HasEnergyForWork = hasEnergy;

        public float ProgressionPerSec {
            get {
                _progressPerSec = _progressQueue.Average();
                return _progressPerSec;
            }
            set => _progressQueue = new Queue<float>(new[] { value });
        }

        private float _progressPerSec;


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
        public event Action onMachineDisabled = delegate { };
        [SerializeField] private bool _isClosed;

        public IEnumerable<MachineSlot> Slots {
            get => _slots;
        }

        [FormerlySerializedAs("_slot")] [SerializeField]
        private List<MachineSlot> _slots;

        public float CurrentProgress {
            get => _currentProgress;
            set {
                _currentProgress = value;
                if (!(CurrentProgress >= MaxProgress && MaxProgress > 0f)) return;
                if (CurrentProgress > MaxProgress) {
                    CurrentProgress -= MaxProgress;
                    CreateProduct();
                }
            }
        }

        private float _lastProgress = 0f;
        [SerializeField]private float _currentProgress;

        public float MaxProgress {
            get => Product.MaxProgress;
        }

        public int SpawnWorkers {
            get => _spawnWorkers;
            set => _spawnWorkers = value;
        }

        [SerializeField] private int _spawnWorkers;

        public WorkerType SpawnWorkerType {
            get => _spawnWorkerType;
            set => _spawnWorkerType = value;
        }

        [SerializeField] WorkerType _spawnWorkerType;

        public IEnumerable<Worker> Workers {
            get => _slots.Select(s => s.CurrentWorker).Where(w => w is not null);
        }

        public virtual void AddWorker(Worker worker, MachineSlot slot) {
            if (!IsWorkable) {
                Debug.LogWarning($"Machine({name}) is not workable.");
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

            if (Slots.All(s => s != slot)) { Debug.LogWarning($"Slots don't belong to machine({name})."); }

            slot.SetCurrentWorker(worker);
            onWorkerChanged?.Invoke();
        }

        public virtual void AddWorker(Worker worker) => AddWorker(worker, Slots.First(s => s.CurrentWorker == null));

        public virtual void RemoveWorker(Worker worker) {
            if (!Workers.Contains(worker)) {
                string str = "";
                if (worker is MonoBehaviour monoWorker) str = $"({monoWorker.name})";
                Debug.LogWarning($"Worker{str} is not working on machine({str}).");
                return;
            }

            Slots.Where(s => s.CurrentWorker?.Equals(worker) ?? false).ForEach(s => s.SetCurrentWorker());
            onWorkerChanged?.Invoke();
        }

        public IEnumerable<WorkDetail> WorkDetails {
            get => _workDetails;
        }

        [SerializeReference, SubclassSelector] private List<WorkDetail> _workDetails = new();

        public virtual ProductBase Product {
            get => _product;
            set {
                ResourceUse.ForEach(r => r.Stop());
                _product = value;
                _product.SetParent(this);
                ResourceUse.ForEach(r => r.Start(this, _resourceManager));
                onProductChanged?.Invoke(value);
            }
        }

        public event Action<ProductBase> onProductChanged = delegate { };

        [SerializeReference, SubclassSelector] private ProductBase _product = new NullProduct();
        public event Action<ProductBase> onCreateProduct = delegate { };

        public DateTimeOffset PlacedTime {
            get => _placedTime;
            set => _placedTime = value;
        }

        private DateTimeOffset _placedTime = DateTimeOffset.Now;

        public void SetMachinePlacedTime(DateTimeOffset time) => _placedTime = time;

        public virtual ProductBase CreateProduct() {
            _product?.OnProductCreated();
            onCreateProduct?.Invoke(_product);
            return _product;
        }

        public void IncreaseProgress(float progress) {
            if (_resourceManager is not null && !HasResourceForWork) {
                UnlockResource();
                _resourceManager.SetResourceUses(ResourceUse.ToArray());
                TryPullResource();
                if (!HasResourceForWork) {
                    Debug.LogWarning($"Machine {name} does not have enough resource to work. {Product.GetType()}");
                    return;
                }
            }

            CurrentProgress += progress;
            onProgress?.Invoke(progress);
        }

        protected virtual void UnlockResource() { _resourceManager.UnlockResource(); }

        protected virtual void TryPullResource() { _resourceManager.TryPullResource(1, out _); }

        public event Action<float> onProgress = delegate { };

        public event Action onWorkerChanged = delegate { };

        private void UpdateWorkDetails() {
            WorkDetails.Where(d => d.CanExecute()).ForEach(d => d.Start());
            WorkDetails.Where(d => !d.CanExecute()).ForEach(d => d.Stop());
        }

        private void UpdateWorkDetails(ProductBase value) => UpdateWorkDetails();
        private void UpdateWorkDetails(bool value) => UpdateWorkDetails();
        private void UpdateWorkDetails(float value) => UpdateWorkDetails();

        private void SubscribeWorkDetails() {
            this.onWorkerChanged += UpdateWorkDetails;
            this.onProductChanged += UpdateWorkDetails;
            this.onCreateProduct += UpdateWorkDetails;
            this.onMachineCloseStatusChanged += UpdateWorkDetails;
            onProgress += UpdateWorkDetails;
        }

        private void UnsubscribeWorkDetails() {
            this.onWorkerChanged -= UpdateWorkDetails;
            this.onProductChanged -= UpdateWorkDetails;
            this.onCreateProduct -= UpdateWorkDetails;
            this.onMachineCloseStatusChanged -= UpdateWorkDetails;
            onProgress -= UpdateWorkDetails;
        }

        protected virtual void Awake() {
            WorkDetails.ForEach(d => d.Machine = this);
            _progressPerSecTimer = new CountdownTimer(1);
            _resourceManager = new(this);
            _product?.SetParent(this);
        }

        protected virtual  void OnEnable() {
            GameController.Instance.PowerGridController.RegisterMachine(this);
            ResourceUse?.ForEach(r => r.Start(this, _resourceManager));
            UpdateWorkDetails();
            SubscribeWorkDetails();
            WorkDetails.ForEach(d => d.Start());
        }

        private void OnValidate() { WorkDetails.ForEach(d => d.Machine = this); }

        protected virtual void OnDisable() {
            GameController.Instance.PowerGridController.UnregisterMachine(this);
            ResourceUse?.ForEach(r => r.Stop());
            UpdateWorkDetails();
            UnsubscribeWorkDetails();
            WorkDetails.ForEach(d => d.Stop());
            onMachineDisabled?.Invoke();
        }


        protected virtual void Start() {
            #region Progression

            _progressPerSecTimer.OnTimerStop += () => {
                var diff = 0f;
                if (_progressQueue.Any()) {
                    var last = _progressQueue.Last();
                    if (CurrentProgress < _lastProgress)
                        diff = CurrentProgress + (MaxProgress - _lastProgress);
                    else diff = CurrentProgress - _lastProgress;
                    //Debug.Log($"Diff: {diff}. Queue: [{ string.Join(", ", _progressQueue)}]");
                }

                if (_progressQueue.All(p => p == 0f)) _progressQueue.Clear();
                _progressQueue.Enqueue(diff);
                _lastProgress = _currentProgress;
                if (_progressQueue.Count > 50) _progressQueue.Dequeue();
                _progressPerSecTimer.Start();
            };

            _progressPerSecTimer.Start();

            #endregion

            #region Resource

            _resourceManager.SetResourceUses(ResourceUse.ToArray());
            onProductChanged += product => {
                //Debug.Log("Product changed to " + nameof(product));
                UnlockResource();
                _resourceManager.SetResourceUses(product.ResourceUse.ToArray());
                TryPullResource();
            };

            ResourceUse?.ForEach(r => r.Start(this, _resourceManager));

            #endregion

            GameController.Instance.BuildNavMesh();
        }

        protected virtual void Update() {
            _progressPerSecTimer?.Tick(Time.deltaTime);
            foreach (var d in WorkDetails) d.Update(Time.deltaTime);
            UpdateWorkDetails();
        }

        public virtual MachineBaseData Save() => new MachineBaseData() {
                PrefabName = PrefabName,
                Position = new(Position),
                PowerUse = _powerUse,
                ResourceManager = _resourceManager.ToSaveData(),
                HasEnergyForWork = HasEnergyForWork,
                HasTimer = _progressPerSecTimer is not null && _progressPerSecTimer != default,
                TimerTime = _progressPerSecTimer?.Time ?? 0f * _progressPerSecTimer?.Progress ?? 1f,
                TimerCurrentTime = _progressPerSecTimer?.Time ?? 0f,
                ProgressQueue = _progressQueue,
                IsClosed = _isClosed,
                CurrentProgress = _currentProgress,
                LastProgress = _lastProgress,
                WorkDetails = _workDetails.Select(w => w.Save()).ToList(),
                Product = _product.Save(),
                PlacedTime = _placedTime,
                SpawnWorkers = _spawnWorkers,
                SpawnWorkerType = _spawnWorkerType,
            };

        public virtual void Load(MachineBaseData data) {
            _workDetails.Clear();
            foreach (var w in data.WorkDetails) {
                var workDetail = (WorkDetail)Activator.CreateInstance(w.Type);
                workDetail.Load(w);
                workDetail.Machine = this;
                _workDetails.Add(workDetail);
            }

            PrefabName = data.PrefabName;
            Position = data.Position;
            PowerUse = data.PowerUse;
            _resourceManager = data.ResourceManager.ToResourceManager(this);
            HasEnergyForWork = data.HasEnergyForWork;
            if (data.HasTimer) {
                _progressPerSecTimer = new CountdownTimer(data.TimerTime);
                _progressPerSecTimer.Time = data.TimerCurrentTime;
            }

            _progressQueue = data.ProgressQueue;
            IsClosed = data.IsClosed;
            CurrentProgress = data.CurrentProgress;
            _lastProgress = data.LastProgress;
            _placedTime = data.PlacedTime;
            _spawnWorkers = data.SpawnWorkers;
            _spawnWorkerType = data.SpawnWorkerType;
            var pType = Type.GetType(data.Product.Type);
            if (pType != null
                && pType.IsSubclassOf(typeof(ProductBase))
                && Activator.CreateInstance(pType) is ProductBase product) {
                if (Product.GetType() == product.GetType()) {
                    Product.Load(data.Product);
                }
                else {
                    product.Load(data.Product);
                    Product = product;
                }
            }

            UpdateWorkDetails();
            OnDisable();
            OnEnable();
        }

        public class MachineBaseData {
            public string PrefabName;
            public V3 Position;
            public int PowerUse;
            public ResourceManager.ResourceManager.SaveData ResourceManager;
            public bool HasEnergyForWork;
            public bool HasTimer;
            public float TimerTime;
            public float TimerCurrentTime;
            public Queue<float> ProgressQueue;
            public bool IsClosed;
            public float CurrentProgress;
            public float LastProgress;
            public List<WorkDetail.SaveData> WorkDetails;
            public IProduct.SaveData Product;
            public DateTimeOffset PlacedTime;
            public int SpawnWorkers;
            public WorkerType SpawnWorkerType;
        }
    }
}