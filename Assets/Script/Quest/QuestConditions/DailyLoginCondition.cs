using UnityEngine;

namespace Script.Quest {
    [CreateAssetMenu(menuName = "Quest/Condition/Daily Login Condition")]
    public class DailyLoginCondition : QuestCondition {
        protected override string OnProgressCheck(Quest quest) => "Completed";

        public override bool Evaluate(Quest quest) => true;
    }
}