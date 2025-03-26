using Script.Controller;
using Script.Controller.Commission;
using UnityEngine;

namespace Script.Quest {
    [CreateAssetMenu(menuName = "Quest/Condition/Commission Completed Condition")]
    public class CommissionCompletedCondition : QuestCondition {
        [SerializeField] public int Amount;
        private const string subbedKeyName = "CommissionsSubbed";
        private const string keyName = "Commissions";
        protected override string OnProgressCheck(Quest quest) {
            bool isSubbed = quest.TryGetQuestData(subbedKeyName, out bool subbed) && subbed == true;
            if (!isSubbed) {
                GameController.Instance.CommissionController.onCommissionCompleted += OnCommissionCompleted;
                quest.AddData(subbedKeyName, true);
            }

            if (!quest.TryGetQuestData(keyName, out int amount)) amount = 0;
            return $"{amount}/{Amount}";

            void OnCommissionCompleted(Commission commission) {
                if (!quest.TryGetQuestData(keyName, out amount)) amount = 0;
                if (amount < Amount) amount++;
                quest.SetData(keyName, amount);
            }
        }
        
        public override bool Evaluate(Quest quest) { throw new System.NotImplementedException(); }
    }
}