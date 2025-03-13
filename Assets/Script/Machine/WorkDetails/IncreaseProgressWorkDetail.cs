using System;
using UnityEngine;

namespace Script.Machine.WorkDetails {
    [Serializable]
    public class IncreaseProgressWorkDetail : WorkDetail {
        [SerializeField] public float ProgressionPerInterval;
        [SerializeField] public float IntervalSeconds;
        [SerializeField] public float _timerTime;
        [SerializeField] public bool _timerStart;
        protected override void OnStart() {
            base.OnStart();
            
            _timer = new CountdownTimer(IntervalSeconds);
            _timer.OnTimerStop += () => {
                Machine.IncreaseProgress(ProgressionPerInterval);
                _timer.Start();
            };
            _timer.Start();
        }


        protected override void OnUpdate(float deltaTime) {
            base.OnUpdate(deltaTime);
            _timer.Tick(deltaTime);
            _timerTime = _timer.Time;
            _timerStart = _timer.IsRunning;
        }

        protected override void OnStop() {
            base.OnStop();
            _timer.Pause();
            _timer.Reset();
        }
    }
}