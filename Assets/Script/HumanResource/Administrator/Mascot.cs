using System;
using System.Collections;
using System.Collections.Generic;
using Script.Gacha.Base;
using UnityEngine;

namespace Script.HumanResource.Administrator {
    public abstract class Mascot : Gacha.Base.Loot {
        public EmployeeName Name;
        public Sprite Portrait;
        public List<Policy> Policies;
        public void SetGrade(Grade grade) => this.Grade = grade;
        public bool IsActive { get; private set; } = false;


        public virtual void OnAssign() {
            if (IsActive) return;
            IsActive = true;
            Policies.ForEach(p => p.OnAssign());
        }

        public virtual void OnDismiss() {
            if (!IsActive) return;
            IsActive = false;
            Policies.ForEach(p => p.OnDismiss());
        }

        public virtual void OnUpdate(float deltaTime) {
            if (!IsActive) return;
            Policies.ForEach(p => p.OnUpdate(deltaTime));
        }
    }
}