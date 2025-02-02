using System;
using System.Collections.Generic;
using System.Linq;
using Script.HumanResource.Worker;
using UnityEngine;

namespace Script.Machine {
    public abstract class MachineBase : MonoBehaviour, IMachine{
        public bool IsClosed { get => _isClosed; set => _isClosed = value; }
        [SerializeField] private bool _isClosed;
        public IEnumerable<MachineSlot> Slots { get => _slot; set => _slot = value.ToList(); }
        [SerializeField] private List<MachineSlot> _slot; 
        public float Progress { get; set; }
        public IEnumerable<IWorker> Workers { get => _slot.Select(s => s.Worker).Where(w => w != null); }
        
        
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
            Slots.Where(s => s.Worker.Equals(worker)).ForEach(s => s.Worker = default);
            WorkDetails.Where(d => d.Slot < Workers.Count() && d.IsRunning).ForEach(d => d.Stop());
            onWorkerChanged?.Invoke();
        }

        public IEnumerable<WorkDetail> WorkDetails { get; }
        [SerializeReference, SubclassSelector] private List<WorkDetail> _workDetails;
        public IProduct Product { get => _product; }
        [SerializeField] private IProduct _product;
        public event Action<IProduct> onCreateProduct;
        public virtual IProduct CreateProduct() {
            IProduct ret;
            if (Product is ScriptableObject scriptableProduct) ret = (IProduct)Instantiate(scriptableProduct);
            else if (Product is MonoBehaviour monoProduct) ret = (IProduct)Instantiate(monoProduct);
            else ret = Product;
            
            onCreateProduct?.Invoke(ret);
            return ret;
        }

        public event Action onWorkerChanged;

        protected virtual void Update() {
            WorkDetails.ForEach(d => d.Update(Time.deltaTime));
        }
    }
}