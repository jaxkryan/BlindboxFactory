using System;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using Script.Machine;
using Unity.VisualScripting;
using UnityEngine;

namespace Script.HumanResource.Worker {
    [Serializable]
    public abstract class Bonus {
        [SerializeField] public BonusCondition Condition;
        public virtual string Name { get => GetType().Name.ToNormalString(StringExtension.StringCapitalizationSetting.CapitalizeEachWords); }
        public abstract string Description { get; }
        public IWorker Worker { get; set; }
        public virtual void OnUpdate(float deltaTime){}
        public virtual void OnStart(){}
        public virtual void OnStop(){}
        
        
        public static void RecalculateBonuses(IWorker worker) {
            var bonuses = GetApplicableBonuses(worker);
            var currentBonuses = worker.Bonuses.ToList();
            currentBonuses.Where(bonus => !bonuses.Contains(bonus)).ForEach(worker.RemoveBonus);
            currentBonuses = worker.Bonuses.ToList();
            bonuses.Where(bonus => !currentBonuses.Contains(bonus)).ForEach(worker.AddBonus);
        }

        public static List<Bonus> GetApplicableBonuses(IWorker worker) =>
            GetApplicableBonuses(worker.CurrentCores, worker.Bonuses);

        public static List<Bonus> GetApplicableBonuses(Dictionary<CoreType, float> cores, IEnumerable<Bonus> bonuses) {
            return bonuses
                .Where(b => b.Condition.IsApplicable(cores))
                .ToList();
        }
    }
}