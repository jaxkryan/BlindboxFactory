using System;
using System.Collections.Generic;
using System.Linq;
using Script.Quest;
using UnityEngine;

namespace Script.Controller {
    [Serializable]
    public class QuestController : ControllerBase {
        public override void Load() { throw new System.NotImplementedException(); }
        public override void Save() { throw new System.NotImplementedException(); }

        [SerializeField] public List<Quest.Quest> Quests;
        private Dictionary<string, object> QuestData { get; set; } = new();
        
        public bool ContainsKey(string key) => QuestData.ContainsKey(key);
        public HashSet<string> Keys() => QuestData.Keys.ToHashSet();

        public bool TryGetValue<T>(string key, out T value) {
            if (QuestData.TryGetValue(key, out var entry) && entry is BlackboardEntry<T> castedEntry) {
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
            ActiveQuests(func).ForEach(q => q.Objectives.ForEach(o => o.Condition(q)));

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
            
            Quests.ForEach(q => {
                if (quests.Any(regQuest => q.Name == regQuest.Name)) duplicatedQuests.Add(q);
                quests.Add(q);
            });
            
            if (duplicatedQuests.Any())
                Debug.LogError("Quests can't be sharing names: " + string.Join(", ", duplicatedQuests.Select(q => q.Name)));
        }

        public event Action<Quest.Quest> onQuestStateChanged = delegate { };
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