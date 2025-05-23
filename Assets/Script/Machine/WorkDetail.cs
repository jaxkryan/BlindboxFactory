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
        public MachineBase Machine { get; set; }
        public bool IsRunning { get; private set; }

        public void Start() {
            if (!IsSetUp()) {
                Debug.LogError("Work detail is not setup.");
                return;
            }
            if (!CanExecute()) return;

            if (IsRunning) return;
            IsRunning = true;
            OnStart();
        }

        protected  virtual void OnStart() { }

        public void Stop() {
            if (!IsSetUp()) {
                Debug.LogError("Work detail is not setup.");
                return;
            }
            if (CanExecute()) return;

            if (!IsRunning) return;
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
            if (!Machine.IsWorkable) return false;
            if (Machine.Workers.Count() < Slot) return false;
            return true;
        }

        protected virtual bool IsSetUp() {
            if (Machine is null) {
                Debug.LogError("Machine is not assigned.");
                return false;
            }
            return true;
        }

        public virtual SaveData Save() =>
            new SaveData() {
                Type = this.GetType(),
                Slot = Slot,
                HasTimer = _timer is not null,
                TimerCurrentTime = _timer?.Time ?? 0f,
                TimerTime = _timer?.Time ?? 0f / _timer?.Progress ?? 1f,
            };

        public virtual void Load(SaveData data) {
            Slot = data.Slot;
            if (data.HasTimer) {
                _timer = new CountdownTimer(data.TimerTime);
                _timer.Time = data.TimerCurrentTime;
            }
        }
        public class SaveData {
            public Type Type;
            public int Slot;
            public bool HasTimer;
            public float TimerTime;
            public float TimerCurrentTime;
        }
    }
}