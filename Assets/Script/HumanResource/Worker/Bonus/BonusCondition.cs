using System;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace Script.HumanResource.Worker {
    [Serializable]
    public class BonusCondition {
        //     [SerializeField] public bool UseHungerCore;
        //
        //     [ConditionalField("UseHungerCore", inverse: false, true)] [SerializeField]
        //     public float HungerCoreMinimum;
        //
        //     [ConditionalField("UseHungerCore", inverse: false, true)] [SerializeField]
        //     public float HungerCoreMaximum;
        //
        //     [SerializeField] public bool UseHappinessCore;
        //
        //     [ConditionalField("UseHappinessCore", inverse: false, true)] [SerializeField]
        //     public float HappinessCoreMinimum;
        //
        //     [ConditionalField("UseHappinessCore", inverse: false, true)] [SerializeField]
        //     public float HappinessCoreMaximum;
        //
        //     [SerializeField] public bool UseBothCores;
        //
        //     [ConditionalField("UseBothCores", inverse: false, true)] [SerializeField]
        //     public float BothCoresMinimum;
        //
        //     [ConditionalField("UseBothCores", inverse: false, true)] [SerializeField]
        //     public float BothCoresMaximum;
    
        [SerializeField] public SerializedDictionary<CoreType, float> Conditions;

        public bool IsApplicable(Dictionary<CoreType, float> currentCores) {
            foreach (var core in Conditions.Keys) {
                var value = Conditions.GetValueOrDefault(core);
                var currentCoreValue = currentCores.FirstOrDefault(c => c.Key == core).Value;
                if (value == 0f) continue;
                if (value > currentCoreValue) return false;
            }
            return true;
        }

        public bool IsApplicable(IWorker worker) =>
            IsApplicable(worker.CurrentCores);
    }
}