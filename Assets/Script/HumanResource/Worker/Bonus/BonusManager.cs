using System;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using MyBox;
using UnityEngine;

namespace Script.HumanResource.Worker {
    [CreateAssetMenu(fileName = "Bonus Manager", menuName = "HumanResource/Bonus Manager")]
    public class BonusManager : ScriptableObject {
        public Dictionary<Bonus, BonusCondition> BonusConditions {
            get => _bonusConditions;
        }

        [SerializeField] private SerializedDictionary<Bonus, BonusCondition> _bonusConditions;

        public void RecalculateBonuses(IWorker worker) {
            var bonuses = GetApplicableBonuses(worker);
            var currentBonuses = worker.Bonuses.ToList();
            currentBonuses.Where(bonus => !bonuses.Contains(bonus)).ForEach(worker.RemoveBonus);
            currentBonuses = worker.Bonuses.ToList();
            bonuses.Where(bonus => !currentBonuses.Contains(bonus)).ForEach(worker.AddBonus);
        }

        public List<Bonus> GetApplicableBonuses(IWorker worker) =>
            GetApplicableBonuses(worker.CurrentCores);

        public List<Bonus> GetApplicableBonuses(Dictionary<CoreType, float> cores) {
            return _bonusConditions
                .Where(b => _bonusConditions
                    .TryGetValue(b.Key, out BonusCondition bonusCondition) && bonusCondition.IsApplicable(cores))
                .Select(b => b.Key)
                .ToList();
        }
        
        private void OnValidate() { }


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
}