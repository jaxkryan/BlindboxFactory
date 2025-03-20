using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using Script.Controller;
using UnityEngine;

namespace Script.Quest {
    [CreateAssetMenu(menuName = "Quest/Condition/Previous Quest Condition", fileName = "Previous Quest Condition")]
    public class PreviousQuestCondition : QuestCondition {
        [SerializeField] public SerializedDictionary<string, QuestType> QuestNames;

        private List<string> _questQuestName =>
            QuestNames
                .Where(q => q.Value == QuestType.Quest)
                .Select(q => q.Key.Trim()).ToList();
        private List<string> _dailyQuestName =>
            QuestNames
                .Where(q => q.Value == QuestType.Quest)
                .Select(q => q.Key).ToList();
        
        private List<Quest> _quests => GameController.Instance.QuestController.Quests
            .Where(q => _questQuestName.Contains(q.Name))
            .Union(GameController.Instance.QuestController.Quests
                .Where(q => _dailyQuestName.Contains(q.Name)))
                .ToList();
        protected override string OnProgressCheck(Quest quest) {
            var list = _quests.Select(q => $"{q.Name} {q.State}").ToList();

            return string.Join("\n", list);
        }
        public override bool Evaluate(Quest quest) => _quests.All(q => q.State == QuestState.Complete);

        public enum QuestType {
            Quest,
            DailyMission
        }
    }
}