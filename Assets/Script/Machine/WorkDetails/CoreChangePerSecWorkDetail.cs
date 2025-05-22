using Script.HumanResource.Worker;
using Script.Utils;

namespace Script.Machine.WorkDetails {
    public class CoreChangePerSecWorkDetail : WorkDetail {
        public CoreType Core;
        public float IncreaseBy;
        public float IntervalSeconds;

        protected override void OnStart() {
            base.OnStart();
            
            _timer = new CountdownTimer(IntervalSeconds);
            _timer.OnTimerStop += OnTimerStop;
            _timer.Start();
        }

        protected override void OnStop() {
            base.OnStop();

            Machine.onProgress -= UpdateCore;
        }

        private void OnTimerStop() {
            UpdateCore(IncreaseBy);
            _timer.Start();
        }
        
        private void UpdateCore(float progress) {            
            Machine.Workers.ForEach(w => w.UpdateCore(Core, IncreaseBy));
        }

        public override SaveData Save() {
            var data = base.Save().CastToSubclass<CoreChangePerSecSaveData, SaveData>();
            if (data is null) return base.Save();
            
            data.Core = Core;
            data.IncreaseBy = IncreaseBy;
            data.IntervalSeconds = IntervalSeconds;
            return data;
        }

        public override void Load(SaveData data) {
            base.Load(data);
            if (data is not CoreChangePerSecSaveData saveData) return;
            
            Core = saveData.Core;
            IncreaseBy = saveData.IncreaseBy;
            IntervalSeconds = saveData.IntervalSeconds;
        }

        public class CoreChangePerSecSaveData : WorkDetail.SaveData {
            public CoreType Core;
            public float IncreaseBy;
            public float IntervalSeconds;
        }
    }
}