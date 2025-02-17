using System;
using System.Collections.Generic;
using System.Linq;
using Script.HumanResource.Worker;
using UnityEngine;

namespace Script.Machine {
    [Serializable]
    public abstract class MachineBase : MonoBehaviour, IMachine{
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
            get => _maxProgress;
        }
        [SerializeField] float _maxProgress;
        public IEnumerable<IWorker> Workers { get => _slot.Select(s => s.Worker).Where(w => w != null); }

        private void Awake() {
            WorkDetails.ForEach(d => d.Machine = this);
        }

        public void AddWorker(IWorker worker) {
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
            Slots.First(s => s.Worker == null).Worker = worker;
            WorkDetails.Where(d => d.Slot >= Workers.Count() && !d.IsRunning).ForEach(d => d.Start());
            onWorkerChanged?.Invoke();
        }

        public void RemoveWorker(IWorker worker) {
            if (!Workers.Contains(worker)) {
                string str = "";
                if (worker is MonoBehaviour monoWorker) str = $"({monoWorker.name})";
                Debug.LogWarning($"Worker{str} is not working on machine({str}).");
                return;
            }
            Slots.Where(s => s.Worker?.Equals(worker) ?? false).ForEach(s => s.Worker = default);
            WorkDetails.Where(d => d.Slot < Workers.Count() && d.IsRunning).ForEach(d => d.Stop());
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
        }

        public event Action<float> onProgress = delegate { };

        public event Action onWorkerChanged = delegate { };

        protected virtual void Update() {
            WorkDetails.ForEach(d => d.Update(Time.deltaTime));
        }
    }
}