using System;
using UnityEngine;

namespace Script.HumanResource.Administrator {
    [CreateAssetMenu(menuName = "HumanResource/Administrator/Policy")]
    public abstract class Policy : Gacha.Base.Loot {
        public string Name;
        public string Description;
        public abstract void OnAssign();
        public virtual void OnDismiss() { ResetValues();}
        public abstract void ResetValues();
    }
}