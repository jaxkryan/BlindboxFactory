using System;
using System.Collections.Generic;
using Script.Controller;
using Script.HumanResource.Worker.Workers;
using Script.Machine;
using UnityEngine;

namespace Script.HumanResource.Worker {
    public interface IWorker {
        Dictionary<CoreType, float> CurrentCores { get; }

        // float CurrentHunger { get; }
        // float CurrentHappiness { get; }
        // Dictionary<CoreType, CoreDrain> CoreDrains { get; }
        // CoreDrain HungerDrain { get; }
        // CoreDrain HappinessDrain { get; }
        // void DrainCores(DrainType drainType);
        // void RefillCore(CoreType core, float amount);
        // void RefillHunger(float amount);
        // void RefillHappiness(float amount);
        Dictionary<CoreType, float> MaximumCore { get; }
        void UpdateCore(CoreType core, float amount, bool trigger = true);

        event Action<CoreType, float> onCoreChanged;

        // event Action<float> onHungerChanged;
        // event Action<float> onHappinessChanged;
        IMachine Machine { get; }
        MachineSlot WorkingSlot { get; }
        void StartWorking(MachineSlot slot);
        void StopWorking();
        event Action onWorking;
        event Action onStopWorking;
        List<Bonus> Bonuses { get; }
        void AddBonus(Bonus bonus);
        void RemoveBonus(Bonus bonus);
        string Name { get; }
        string Description { get; }
        Sprite Portrait { get; }
        WorkerDirector Director { get; }

        public static WorkerType ToWorkerType<TWorker>(TWorker worker) where TWorker : Worker {
            return worker switch {
                FactoryWorker factoryWorker => WorkerType.FactoryWorker,
                Worker _worker => WorkerType.Worker,
                _ => throw new ArgumentOutOfRangeException(nameof(worker), worker, null)
            };
        }
    }
}