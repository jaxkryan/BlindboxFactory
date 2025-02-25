using System;
using System.Collections.Generic;
using Script.HumanResource.Worker;

namespace Script.Machine {
    public interface IMachine {
        float ProgressionPerSec { get; }
        float EstimateCompletionTime { get; }
        bool IsClosed { get; }
        IEnumerable<MachineSlot> Slots { get; }
        float CurrentProgress { get; set; }
        float MaxProgress { get; }
        IEnumerable<IWorker> Workers { get; }
        void AddWorker(IWorker worker);
        void AddWorker(IWorker worker, MachineSlot slot);
        void RemoveWorker(IWorker worker);
        IEnumerable<WorkDetail> WorkDetails { get; }
        IProduct Product { get; }
        IProduct CreateProduct();
        void IncreaseProgress(float progress);
        event Action<float> onProgress;
        event Action onWorkerChanged;
        event Action<IProduct> onCreateProduct;
    }
}