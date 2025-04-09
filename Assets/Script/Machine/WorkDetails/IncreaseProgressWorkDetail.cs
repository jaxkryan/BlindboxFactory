using System;
using Script.Utils;
using UnityEngine;

namespace Script.Machine.WorkDetails {
    [Serializable]
    public class IncreaseProgressWorkDetail : WorkDetail {
        [SerializeField] public float ProgressionPerInterval;
        [SerializeField] public float IntervalSeconds;
        protected override void OnStart() {
            base.OnStart();
            
            _timer = new CountdownTimer(IntervalSeconds);
            _timer.OnTimerStop += OnTimerStop;
            _timer.Start();
        }

        private void OnTimerStop() {
            Machine.IncreaseProgress(ProgressionPerInterval);
            _timer.Start();
        }

        protected override void OnUpdate(float deltaTime) {
            base.OnUpdate(deltaTime);
            _timer.Tick(deltaTime);
        }

        protected override void OnStop() {
            base.OnStop();
            _timer.Pause();
            _timer.Reset();
        }

        public override SaveData Save() {
            var data = base.Save().CastToSubclass<IncreaseProgressSaveData, SaveData>();
            if (data is null) return base.Save();
            
            data.ProgressionPerInterval = ProgressionPerInterval;
            data.IntervalSeconds = IntervalSeconds;
            return data;
        }

        public override void Load(SaveData saveData) {
            base.Load(saveData);
            if (saveData is not IncreaseProgressSaveData data) return;
            
            ProgressionPerInterval = data.ProgressionPerInterval;
            IntervalSeconds = data.IntervalSeconds;
        }


        public class IncreaseProgressSaveData : SaveData {
            public float ProgressionPerInterval;
            public float IntervalSeconds;
        }
    }
}