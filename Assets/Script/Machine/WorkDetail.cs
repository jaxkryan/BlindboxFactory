using System;
using System.Linq;
using Script.Machine;
using Unity.VisualScripting;
using UnityEngine;
using IMachine = Script.Machine.IMachine;

namespace Script.Machine {
    [Serializable]
    public abstract class WorkDetail {
        [SerializeField] public int Slot;
        protected CountdownTimer _timer;
        public IMachine Machine { get; set; }
        public bool IsRunning { get; private set; }

        public void Start() {
            if (!IsSetUp()) {
                Debug.LogError("Work detail is not setup.");
                return;
            }
            if (!CanExecute()) {
                //Debug.LogError("Work detail cannot be started.");
                return;
            }
            Debug.LogWarning($"{this.GetType()} start");
            IsRunning = true;
            OnStart();
        }

        protected  virtual void OnStart() { }

        public void Stop() {
            if (!IsSetUp()) {
                Debug.LogError("Work detail is not setup.");
                return;
            }
            if (CanExecute()) {
                Debug.LogError("Work detail cannot be stopped.");
                return;
            }
            IsRunning = false;
            OnStop();
        }

        protected  virtual void OnStop() { }

        public void Update(float deltaTime) {
            if (!IsSetUp()) {
                Debug.LogError("Work detail is not setup.");
                return;
            }
            if (IsRunning) OnUpdate(deltaTime);
        } 
        protected virtual void OnUpdate(float deltaTime) { }

        public virtual bool CanExecute() {
            if (Machine is null) return false;
            if (Machine.Slots.Count() < Slot) return false;
            if (IsRunning) return false;
            
            return true;
        }

        protected virtual bool IsSetUp() {
            if (Machine is null) {
                Debug.LogError("Machine is not assigned.");
                return false;
            }
            return true;
        }
    }
}