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
        protected abstract void ResetValues();
    }
}