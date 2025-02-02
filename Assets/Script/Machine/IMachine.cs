using System;
using System.Collections.Generic;
using Script.HumanResource.Worker;

namespace Script.Machine {
    public interface IMachine {
        bool IsClosed { get; }
        IEnumerable<MachineSlot> Slots { get; }
        float Progress { get; }
        IEnumerable<IWorker> Workers { get; }
        void AddWorker(IWorker worker);
        void RemoveWorker(IWorker worker);
        IEnumerable<WorkDetail> WorkDetails { get; }
        IProduct Product { get; }
        IProduct CreateProduct();
        event Action onWorkerChanged;
        event Action<IProduct> onCreateProduct;
    }
}