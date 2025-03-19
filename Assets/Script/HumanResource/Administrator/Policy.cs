using System;
using Unity.Cinemachine;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Script.HumanResource.Administrator {
    [CreateAssetMenu(menuName = "HumanResource/Administrator/Policy")]
    public abstract class Policy : Gacha.Base.Loot {
        public string Name {get => FormatName();}

        protected virtual string FormatName() => $"{Grade} {_name}";

        [SerializeField] protected string _name;
        public string Description {get => FormatDescription();}

        protected virtual string FormatDescription() => _description;

        [TextArea]
        [SerializeField] protected string _description;
        public abstract void OnAssign();
        public virtual void OnDismiss() { ResetValues();}
        public virtual void OnUpdate(float deltaTime) {}
        protected abstract void ResetValues();

        public virtual SaveData Save() =>
            new SaveData() {
                Type = this.GetType().Name,
                Name = _name,
                Description = _description
            };

        public virtual void Load(SaveData data) {
            _name = data.Name;
            _description = data.Description;
        }
        public class SaveData {
            public string Type;
            public string Name;
            public string Description;
        }
    }
}