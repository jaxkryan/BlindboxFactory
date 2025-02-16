using System;
using Script.Machine;
using Unity.VisualScripting;
using UnityEngine;
using IMachine = Script.Machine.IMachine;

namespace Script.HumanResource.Worker {
    [Serializable]
    public abstract class WorkDetail {
        [SerializeField] public int Slot;
        protected CountdownTimer _timer;
        public IMachine Machine { get; set; }
        public bool IsRunning { get; private set; }

        public void Start() {
            IsRunning = true;
            OnStart();
        }

        protected  virtual void OnStart() { }

        public void Stop() {
            IsRunning = false;
            OnStop();
        }

        protected  virtual void OnStop() { }

        public void Update(float deltaTime) {
            if (IsRunning) OnUpdate(deltaTime);
        } 
        protected virtual void OnUpdate(float deltaTime) { }
    }
}