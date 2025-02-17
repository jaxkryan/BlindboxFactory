using System;
using Script.Machine;
using Unity.VisualScripting;
using UnityEngine;

namespace Script.HumanResource.Worker {
    [Serializable]
    public abstract class Bonus{
        public abstract string Name { get; }
        public abstract string Description { get; }
        public virtual void OnUpdate(float deltaTime){}
        public virtual void OnStart(){}
        public virtual void OnStop(){}
    }
}