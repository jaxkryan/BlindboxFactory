using System.Linq;
using Script.Quest;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Script.UI.Mission {
    public class DailyItemUI : MonoBehaviour {
        [SerializeField] TextMeshProUGUI _name;
        [SerializeField] TextMeshProUGUI _description;
        [SerializeField] TextMeshProUGUI _progress;
        [SerializeField] Image _tick;
        public Quest.Quest DailyMission { get; set; }
        
        public void UpdateQuestData() {
            if (DailyMission == null) {
                _name.text = "";
                _description.text = "";
                _progress.text = "";
                _tick.enabled = false;
                Debug.LogWarning($"No mission assigned!");
                return;
            }
            
            DailyMission.Evaluate();
            bool isCompleted = DailyMission.State is QuestState.Complete; 
            
            _name.text = DailyMission.Name;
            _description.text = DailyMission.Description;
            _progress.text = isCompleted ? "" : string.Join("\n", DailyMission.Objectives.Select(p => p.Progress(DailyMission)));
            _tick.enabled = isCompleted;
            DailyMission.onQuestStateChanged += OnQuestStateChanged;
        }

        private void OnQuestStateChanged(Quest.Quest quest, QuestState state) {
            if (state == QuestState.Complete) {
                Debug.Log($"Daily mission {DailyMission.Name} completed! Removing item.");
                Destroy(gameObject);
            }
        }
    }
}