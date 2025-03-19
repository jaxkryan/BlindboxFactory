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
        public virtual Vector2Int Position { get; set; }
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
        public virtual bool HasResourceForWork => _resourceManager.HasResourcesForWork(out _);

        public bool CanCreateProduct {
            get => _product.CanCreateProduct;
        }

        public bool IsWorkable {
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
                if (!(CurrentProgress >= MaxProgress)) return;
                CurrentProgress -= MaxProgress;
                CreateProduct();
            }
        }

        private float _lastProgress = 0f;
        private float _currentProgress;

        public float MaxProgress {
            get => Product.MaxProgress;
        }

        public IEnumerable<IWorker> Workers {
            get => _slots.Select(s => s.CurrentWorker).Where(w => w != null);
        }

        public virtual void AddWorker(IWorker worker, MachineSlot slot) {
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

        public virtual void AddWorker(IWorker worker) => AddWorker(worker, Slots.First(s => s.CurrentWorker == null));

        public virtual void RemoveWorker(IWorker worker) {
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

        [SerializeReference, SubclassSelector] private List<WorkDetail> _workDetails;

        public virtual ProductBase Product {
            get => _product;
            set {
                ResourceUse.ForEach(r => r.Stop());
                _product = value;
                ResourceUse.ForEach(r => r.Start(this, _resourceManager));
                onProductChanged?.Invoke(value);
            }
        }

        public event Action<ProductBase> onProductChanged = delegate { };

        [SerializeReference, SubclassSelector] private ProductBase _product;
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
                    Debug.LogError($"Machine {name} does not have enough resource to work. {Product.GetType()}");
                    return;
                }
            }

            CurrentProgress += progress;
            onProgress?.Invoke(progress);
        }

        protected virtual void UnlockResource() {
            _resourceManager.UnlockResource();
        }

        protected virtual void TryPullResource() {
            _resourceManager.TryPullResource(1, out _);
        }

        public event Action<float> onProgress = delegate { };

        public event Action onWorkerChanged = delegate { };

        private void UpdateWorkDetails() {
            WorkDetails.Where(d => d.CanExecute()).ForEach(d => d.Start());
            WorkDetails.Where(d => !d.CanExecute()).ForEach(d => d.Stop());
        }

        private void UpdateWorkDetails(ProductBase value) => UpdateWorkDetails();
        private void UpdateWorkDetails(bool value) => UpdateWorkDetails();

        private void SubscribeWorkDetails() {
            this.onWorkerChanged += UpdateWorkDetails;
            this.onProductChanged += UpdateWorkDetails;
            this.onCreateProduct += UpdateWorkDetails;
            this.onMachineCloseStatusChanged += UpdateWorkDetails;
        }

        private void UnsubscribeWorkDetails() {
            this.onWorkerChanged -= UpdateWorkDetails;
            this.onProductChanged -= UpdateWorkDetails;
            this.onCreateProduct -= UpdateWorkDetails;
            this.onMachineCloseStatusChanged -= UpdateWorkDetails;
        }

        protected virtual void Awake() {
            WorkDetails.ForEach(d => d.Machine = this);
            _progressPerSecTimer = new CountdownTimer(1);
            _resourceManager = new();
        }

        private void OnEnable() {
            ResourceUse?.ForEach(r => r.Start(this, _resourceManager));
            WorkDetails.ForEach(d => d.Start());
            SubscribeWorkDetails();
        }

        private void OnValidate() {
            WorkDetails.ForEach(d => d.Machine = this);
        }

        private void OnDisable() {
            ResourceUse?.ForEach(r => r.Stop());
            WorkDetails.ForEach(d => d.Stop());
            UnsubscribeWorkDetails();
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

            GameController.Instance.MachineController.AddMachine(this);
        }

        protected virtual void Update() {
            _progressPerSecTimer?.Tick(Time.deltaTime);
            WorkDetails.ForEach(d => d.Update(Time.deltaTime));
        }

        public virtual MachineBaseData Save() => new MachineBaseData(){
            PrefabName = PrefabName,
            Position = Position,
            PowerUse = _powerUse,
            ResourceManager = _resourceManager.ToSaveData(),
            HasEnergyForWork = HasEnergyForWork,
            HasTimer = _progressPerSecTimer is not null && _progressPerSecTimer != default,
            TimerTime = _progressPerSecTimer?.Time ?? 0f / _progressPerSecTimer?.Progress ?? 1f,
            TimerCurrentTime = _progressPerSecTimer?.Time ?? 0f,
            ProgressQueue = _progressQueue,
            IsClosed = _isClosed,
            CurrentProgress = _currentProgress,
            LastProgress = _lastProgress,
            WorkDetails = _workDetails,
            Product = _product,
            PlacedTime = _placedTime
        };

        public virtual void Load(MachineBaseData data) {
            PrefabName = data.PrefabName;
            Position = data.Position;
            PowerUse = data.PowerUse;
            _resourceManager = data.ResourceManager.ToResourceManager();
            HasEnergyForWork = data.HasEnergyForWork;
            if (data.HasTimer) {
                _progressPerSecTimer = new CountdownTimer(data.TimerTime);
                _progressPerSecTimer.Time = data.TimerCurrentTime;
            }

            _progressQueue = data.ProgressQueue;
            IsClosed = data.IsClosed;
            CurrentProgress = data.CurrentProgress;
            _lastProgress = data.LastProgress;
            _workDetails = data.WorkDetails;
            Product = data.Product;
            _placedTime = data.PlacedTime;
            
            OnDisable();
            OnEnable();
        }

        public class MachineBaseData {
            public string PrefabName;
            public Vector2Int Position;
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
            public List<WorkDetail> WorkDetails;
            public ProductBase Product;
            public DateTimeOffset PlacedTime;
        }
    }
}