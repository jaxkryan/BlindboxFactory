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
        public Quest.Quest Quest { get; set; }
        
        public void UpdateQuestData() {
            if (Quest == null) {
                _name.text = "";
                _description.text = "";
                _progress.text = "";
                _tick.enabled = false;
                Debug.LogWarning($"No mission assigned!");
                return;
            }
            
            Quest.Evaluate();
            bool isCompleted = Quest.State == QuestState.Complete; 
            
            _name.text = Quest.Name;
            _description.text = Quest.Description;
            _progress.text = isCompleted ? "" : string.Join("\n", Quest.Preconditions.Select(p => p.Progress(Quest)));
            _tick.enabled = isCompleted;
        }
    }
}