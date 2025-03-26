using System;
using System.Collections.Generic;
using System.Linq;
using MyBox;
using Script.Controller;
using UnityEngine;

namespace Script.Quest {
    [CreateAssetMenu(menuName = "Quest/Quest", fileName = "New Quest")]
    public class Quest : ScriptableObject{
        [SerializeField] public string Name;
        [TextArea]
        [SerializeField] public string Description;
        
        [HideInInspector]public QuestState State;


        public bool TryGetQuestData<T>(string keyName, out T data) {
            var controller = GameController.Instance.QuestController;
            return controller.TryGetValue(Key(keyName), out data);
        }

        public void ClearQuestData() {
            var controller = GameController.Instance.QuestController;
            var keys = controller.Keys().Where(k => k.StartsWith(Key(""))); 
            keys.Where(k => controller.ContainsKey(k)).ForEach(k => controller.RemoveData(k));
        }

        public void AddData<T>(string keyName, T data) {
            var controller = GameController.Instance.QuestController;
            controller.AddData(Key(keyName), data);
        }
        
        public void SetData<T>(string keyName, T data) => GameController.Instance.QuestController.SetData(Key(keyName), data);

        private string Key(string keyName) => $"{Name}: {keyName}";
        
        public void Evaluate() {
            QuestState oriState;
            do {
                oriState = State;
                switch (oriState) {
                    case QuestState.Locked:
                        if (Preconditions.All(c => c.Evaluate(this)))
                            State = QuestState.InProgress;
                        break;
                    case QuestState.InProgress:
                        if (Objectives.All(o => o.Evaluate(this)))
                            State = QuestState.Complete;
                        OnComplete();
                        break;
                    case QuestState.Complete:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                if (oriState != State) onQuestStateChanged?.Invoke(this, State); 
            }
            while (oriState != State);
            
        }

        public event Action<Quest, QuestState> onQuestStateChanged = delegate { };
        
        [SerializeField] public List<QuestCondition> Preconditions;
        [SerializeField] public List<QuestCondition> Objectives;

        [SerializeReference, SubclassSelector] public List<QuestReward> Rewards; 
        
        private void OnComplete() {
            Rewards.ForEach(r => r.Grant());
        }
    }
}