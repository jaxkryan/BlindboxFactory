using System;
using System.Collections.Generic;
using Script.Controller;
using Script.HumanResource.Worker;
using Script.Machine.Products;
using UnityEngine;

namespace Script.Machine {
    public interface IMachine {
        float ProgressionPerSec { get; }
        float EstimateCompletionTime { get; }
        bool IsClosed { get; }
        bool HasEnergyForWork { get; }
        bool HasResourceForWork { get; }
        bool CanCreateProduct { get; }
        bool IsWorkable { get; }
        IEnumerable<MachineSlot> Slots { get; }
        float CurrentProgress { get; set; }
        float MaxProgress { get; }
        int SpawnWorkers { get; }
        WorkerType SpawnWorkerType { get; }
        IEnumerable<Worker> Workers { get; }
        void AddWorker(Worker worker);
        void AddWorker(Worker worker, MachineSlot slot);
        void RemoveWorker(Worker worker);
        IEnumerable<WorkDetail> WorkDetails { get; }
        ProductBase Product { get; }
        ProductBase CreateProduct();
        void IncreaseProgress(float progress);
        event Action<float> onProgress;
        event Action onWorkerChanged;
        event Action<ProductBase> onCreateProduct;
        event Action<bool> onMachineCloseStatusChanged;
        event Action onMachineDestroyed;
        DateTimeOffset PlacedTime { get; } 
        Vector3 Position { get; }
    }
}