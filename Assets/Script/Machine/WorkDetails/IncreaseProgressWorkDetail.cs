using System;
using UnityEngine;

namespace Script.Machine.WorkDetails {
    [Serializable]
    public class IncreaseProgressWorkDetail : WorkDetail {
        [SerializeField] public float ProgressionPerInterval;
        [SerializeField] public float IntervalSeconds;
        protected override void OnStart() {
            Debug.Log("Starting Work Detail");
            base.OnStart();
            
            _timer = new CountdownTimer(IntervalSeconds);
            _timer.OnTimerStop += () => {
                Machine.IncreaseProgress(ProgressionPerInterval);
                _timer.Start();
            };
            _timer.Start();
        }


        protected override void OnUpdate(float deltaTime) {
            base.OnStop();
            _timer.Tick(deltaTime);
        }

        protected override void OnStop() {
            base.OnStop();
            _timer.Reset();
            _timer.Pause();
        }
    }
}