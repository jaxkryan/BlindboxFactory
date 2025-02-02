using System.Collections;
using System.Collections.Generic;
using Script.Gacha.Base;
using UnityEngine;

namespace Script.HumanResource.Administrator {
    public abstract class Administrator : Gacha.Base.Loot {
        public EmployeeName Name;
        public Sprite Portrait;
        public IEnumerable<Policy> Policies;
        public void SetGrade(Grade grade) => this.Grade = grade;
        
        public virtual void OnAssignManager() => Policies.ForEach(p => p.OnAssign());
        public virtual void OnDismissManager() => Policies.ForEach(p => p.OnDismiss());
        
    }
}