using UnityEngine;

namespace Script.Quest {
    public abstract class QuestCondition : ScriptableObject {
        public virtual string Description { get => _description; }
        [SerializeField] protected string _description;

        public string Progress(Quest quest) {
            Evaluate(quest);
            return OnProgressCheck(quest);
        }

        protected abstract string OnProgressCheck(Quest quest);
        public abstract bool Evaluate(Quest quest);
    }
}