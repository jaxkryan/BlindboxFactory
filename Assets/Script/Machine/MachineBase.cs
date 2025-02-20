using System;
using System.Collections.Generic;
using System.Linq;
using Script.HumanResource.Worker;
using UnityEngine;

namespace Script.Machine {
    [Serializable]
    public abstract class MachineBase : MonoBehaviour, IMachine{
        public float ProgressionPerSec {
            get {
                var avg = 0f;
                _progressQueue.ForEach(p => avg = (p + avg ) / 2);
                return avg;
            }
        }

        private Queue<float> _progressQueue = new ();
        public float EstimateCompletionTime { get => (MaxProgress - CurrentProgress)/ProgressionPerSec; }
        public bool IsClosed { get => _isClosed; set => _isClosed = value; }
        [SerializeField] private bool _isClosed;
        public IEnumerable<MachineSlot> Slots { get => _slot; set => _slot = value.ToList(); }
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
        public IEnumerable<IWorker> Workers { get => _slot.Select(s => s.CurrentWorker).Where(w => w != null); }

        private void Awake() {
            WorkDetails.ForEach(d => d.Machine = this);
        }

        public void AddWorker(IWorker worker, MachineSlot slot) {
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

            if (Slots.All(s => s != slot)) {
                Debug.LogWarning($"Slots don't belong to machine({name}).");
            }
            slot.SetCurrentWorker(worker);
            WorkDetails.Where(d => d.CanStart()).ForEach(d => d.Start());
            onWorkerChanged?.Invoke();
        }

        public void AddWorker(IWorker worker) => AddWorker(worker, Slots.First(s => s.CurrentWorker == null));

        public void RemoveWorker(IWorker worker) {
            if (!Workers.Contains(worker)) {
                string str = "";
                if (worker is MonoBehaviour monoWorker) str = $"({monoWorker.name})";
                Debug.LogWarning($"Worker{str} is not working on machine({str}).");
                return;
            }
            Slots.Where(s => s.CurrentWorker?.Equals(worker) ?? false).ForEach(s => s.SetCurrentWorker());
            WorkDetails.Where(d => d.CanStop()).ForEach(d => d.Stop());
            onWorkerChanged?.Invoke();
        }

        public IEnumerable<WorkDetail> WorkDetails {
            get => _workDetails;
        }
        [SerializeReference, SubclassSelector] private List<WorkDetail> _workDetails;
        public IProduct Product { get => _product; }
        [SerializeField] private IProduct _product;
        public event Action<IProduct> onCreateProduct = delegate { };
        public virtual IProduct CreateProduct() {
            onCreateProduct?.Invoke(_product);
            return _product;
        }

        public void IncreaseProgress(float progress) {
            CurrentProgress += progress;
            onProgress?.Invoke(progress);
            _progressQueue.Enqueue(progress);
            if (_progressQueue.Count() > 10) _progressQueue.Dequeue();
        }

        public event Action<float> onProgress = delegate { };

        public event Action onWorkerChanged = delegate { };

        protected virtual void Update() {
            WorkDetails.ForEach(d => d.Update(Time.deltaTime));
        }
    }
}