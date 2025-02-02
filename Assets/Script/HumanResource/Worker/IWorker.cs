using System;
using System.Collections.Generic;
using Script.Machine;
using UnityEngine;

namespace Script.HumanResource.Worker {
    public interface IWorker {
        float CurrentHunger { get; }
        float CurrentHappiness { get; }
        CoreDrain HungerDrain { get; }
        CoreDrain HappinessDrain { get; }
        void DrainCores(DrainType drainType);
        event Action onCoreDrained;
        void RefillHunger(float amount);
        void RefillHappiness(float amount);
        event Action<float> onHungerChanged;
        event Action<float> onHappinessChanged;
        IMachine Machine{ get; }
        void DoWork();
        void StopWorking();
        event Action onWorking;
        event Action onStopWorking;
        IEnumerable<Bonus> Bonuses { get; }
        void AddBonus(Bonus bonus);
        void RemoveBonus(Bonus bonus);
        string Id { get; }
        string Name { get; }
        string Description { get; }
        Sprite Portrait { get; }
        WorkerDirector Director { get; }
    }
}