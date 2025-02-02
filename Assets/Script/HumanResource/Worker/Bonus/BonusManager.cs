using System;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using MyBox;
using UnityEngine;

namespace Script.HumanResource.Worker {
    [CreateAssetMenu(fileName = "Bonus Manager", menuName = "HumanResource/Bonus Manager")]
    public class BonusManager : ScriptableObject {
        [SerializeField] private SerializedDictionary<Bonus, BonusCondition> _bonusConditions;

        public void RecalculateBonuses(IWorker worker){
            var bonuses = GetApplicableBonuses();
            var currentBonuses = worker.Bonuses.ToList();
            currentBonuses.Where(bonus => !bonuses.Contains(bonus)).ForEach(worker.RemoveBonus);
            currentBonuses = worker.Bonuses.ToList();
            bonuses.Where(bonus => !currentBonuses.Contains(bonus)).ForEach(worker.AddBonus);
            
            List<Bonus> GetApplicableBonuses() {
                var applicableBonuses = new List<Bonus>();
                var hunger = worker.CurrentHunger;
                var happiness = worker.CurrentHappiness;

                //Check for all that satisfied first
                foreach (var bonus in _bonusConditions.Keys) {
                    var condition = _bonusConditions[bonus];
                    if (condition.UseHungerCore) {
                        if (hunger >= condition.HungerCoreMinimum && hunger <= condition.HungerCoreMaximum)
                            applicableBonuses.AddIfNew(bonus);
                    }

                    if (condition.UseHappinessCore) {
                        if (happiness >= condition.HappinessCoreMinimum && happiness <= condition.HappinessCoreMaximum)
                            applicableBonuses.AddIfNew(bonus);
                    }

                    if (condition.UseBothCores) {
                        if (hunger >= condition.HungerCoreMinimum && hunger <= condition.HungerCoreMaximum
                                                                  && happiness >= condition.HappinessCoreMinimum &&
                                                                  happiness <= condition.HappinessCoreMaximum)
                            applicableBonuses.AddIfNew(bonus);
                    }
                }

                //Then remove those that aren't qualified
                var removeUnmet = new List<Bonus>();
                foreach (var bonus in applicableBonuses) {
                    var condition = _bonusConditions[bonus];
                    if (condition.UseHungerCore) {
                        if (!(hunger >= condition.HungerCoreMinimum && hunger <= condition.HungerCoreMaximum))
                            removeUnmet.AddIfNew(bonus);
                    }

                    if (condition.UseHappinessCore) {
                        if (!(happiness >= condition.HappinessCoreMinimum &&
                              happiness <= condition.HappinessCoreMaximum))
                            removeUnmet.AddIfNew(bonus);
                    }

                    if (condition.UseBothCores) {
                        if (!(hunger >= condition.HungerCoreMinimum && hunger <= condition.HungerCoreMaximum
                                                                    && happiness >= condition.HappinessCoreMinimum &&
                                                                    happiness <= condition.HappinessCoreMaximum))
                            removeUnmet.AddIfNew(bonus);
                    }
                }

                removeUnmet.ForEach(bonus => applicableBonuses.Remove(bonus));
                return applicableBonuses;
            }
        }

        private void OnValidate() {
            foreach (var bonus in _bonusConditions.Keys) {
                var bonusConditions = new List<string>();
                if (_bonusConditions[bonus].UseHungerCore) bonusConditions.Add("Hunger");
                if (_bonusConditions[bonus].UseHappinessCore) bonusConditions.Add("Happiness");
                if (_bonusConditions[bonus].UseBothCores) bonusConditions.Add("Both Cores");

                if (bonusConditions.Count > 1) {
                    Debug.LogWarning($"A bonus({bonus.GetType()}) has more than 1 bonus condition: {string.Join(" and ", bonusConditions)}");
                }
            }
        }


        [Serializable]
        public class BonusCondition {
            [SerializeField] public bool UseHungerCore;

            [ConditionalField("UseHungerCore", inverse: false, true)] [SerializeField]
            public float HungerCoreMinimum;

            [ConditionalField("UseHungerCore", inverse: false, true)] [SerializeField]
            public float HungerCoreMaximum;

            [SerializeField] public bool UseHappinessCore;

            [ConditionalField("UseHappinessCore", inverse: false, true)] [SerializeField]
            public float HappinessCoreMinimum;

            [ConditionalField("UseHappinessCore", inverse: false, true)] [SerializeField]
            public float HappinessCoreMaximum;

            [SerializeField] public bool UseBothCores;

            [ConditionalField("UseBothCores", inverse: false, true)] [SerializeField]
            public float BothCoresMinimum;

            [ConditionalField("UseBothCores", inverse: false, true)] [SerializeField]
            public float BothCoresMaximum;
        }
    }
}