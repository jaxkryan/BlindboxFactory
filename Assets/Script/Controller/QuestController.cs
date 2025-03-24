using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Script.Quest;
using UnityEngine;

namespace Script.Controller {
    [Serializable]
    public class QuestController : ControllerBase {

        [SerializeField] private List<Quest.Quest> _quest;
        public List<Quest.Quest> Quests {
            get {
                return _quest ?? NewSetAndLog();

                List<Quest.Quest> NewSetAndLog() {
                    Debug.LogError("Quest list is empty!");
                    return new ();
                }
            }
        }

        private Dictionary<string, object> QuestData { get; set; } = new();
        
        public bool ContainsKey(string key) => QuestData.ContainsKey(key);
        public HashSet<string> Keys() => QuestData.Keys.ToHashSet();

        public bool TryGetValue<T>(string key, out T value) {
            if (QuestData.TryGetValue(key, out var entry) && entry is Entry<T> castedEntry) {
                value = castedEntry.Value;
                return true;
            }

            value = default;
            return false;
        }

        public override void OnStart() {
            base.OnStart();
            RegisterQuestsToEvents();
        }

        public void AddData<T>(string key, T value) => QuestData.TryAdd(key, new Entry<T> (key, value));
        public void RemoveData(string key) => QuestData.Remove(key);
        public void SetData<T>(string key, T value) => QuestData[key] = new Entry<T>(key, value);

        public void ReevaluateActiveQuests(Func<Quest.Quest, bool> func) =>
            ActiveQuests(func).ForEach(q => q.Objectives.ForEach(o => o.Evaluate(q)));

        public IEnumerable<Quest.Quest> ActiveQuests(Func<Quest.Quest, bool> func) => Quests.Where(q => q.State is QuestState.InProgress).Where(func);
        private void RegisterQuestsToEvents() {
            var machines = GameController.Instance.MachineController.Machines;
            machines.ForEach(m => {
                m.onProductChanged += (_) => ReevaluateActiveQuests(q => q.Objectives.Any(o => o is ResourceConsumedQuestCondition));
                m.onCreateProduct += (_) => ReevaluateActiveQuests(q => q.Objectives.Any(o => o is ResourceConsumedQuestCondition));
            });
            GameController.Instance.ResourceController.onResourceAmountChanged += (_,_,_) => ReevaluateActiveQuests(q => true);
            GameController.Instance.QuestController.onQuestStateChanged += (_) => ReevaluateActiveQuests(q => true);
        }

        public override void OnValidate() {
            base.OnValidate();
            List<Quest.Quest> quests = new();
            List<Quest.Quest> duplicatedQuests = new();
            
            Quests?.ForEach(q => {
                if (quests.Any(regQuest => q.Name == regQuest.Name)) duplicatedQuests.Add(q);
                quests.Add(q);
            });
            
            if (duplicatedQuests.Any())
                Debug.LogError("Quests can't be sharing names: " + string.Join(", ", duplicatedQuests.Select(q => q.Name)));
        }

        public event Action<Quest.Quest> onQuestStateChanged = delegate { };
        public override void Load() {
            try {
                if (!GameController.Instance.SaveManager.SaveData.TryGetValue(this.GetType().Name, out var saveData)
                      || JsonConvert.DeserializeObject<SaveData>(saveData) is not SaveData data) return;

                _quest.ForEach(q => q.State = QuestState.Locked);
                for (var i = 0; i < data.QuestStates.Count; i++) {
                    _quest[i].State = data.QuestStates[i];
                }
                QuestData = data.QuestData;
            }
            catch {
                Debug.LogError($"Cannot load {GetType()}");
                return;
            }
        }
        public override void Save() {
            var newSave = new SaveData() { QuestData = QuestData, QuestStates =  new()};

            var list = _quest.Clone();

            while (list.Any(q => q.State != QuestState.Locked)) {
                var quest = list[0];
                newSave.QuestStates.Add(quest.State);
                list.RemoveAt(0);
            }
            
            
            try {
                if (!GameController.Instance.SaveManager.SaveData.TryGetValue(this.GetType().Name, out var saveData)
                    || JsonConvert.DeserializeObject<SaveData>(saveData) is SaveData data)
                    GameController.Instance.SaveManager.SaveData.TryAdd(this.GetType().Name,
                        JsonConvert.SerializeObject(newSave));
                else
                    GameController.Instance.SaveManager.SaveData[this.GetType().Name]
                        = JsonConvert.SerializeObject(newSave);
            }
            catch {
                Debug.LogError($"Cannot save {GetType()}");
            }
        }

        public class SaveData {
            public List<QuestState> QuestStates;
            public Dictionary<string, object> QuestData;
        }
    }

    public class Entry<T> {
        public string Key { get; }
        public T Value { get; }
        public Type ValueType { get; }

        public Entry(string key, T value) {
            Key = key;
            Value = value;
            ValueType = typeof(T);
        }

        public override bool Equals(object obj) => obj is Entry<T> entry && entry.Key == Key;
        public override int GetHashCode() => Key.GetHashCode();
    }
}