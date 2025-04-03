using System;
using Script.HumanResource.Worker;
using UnityEngine;
using UnityEngine.Serialization;

namespace Script.Machine.WorkDetails {
    [Serializable]
    public class CoreChangeWorkDetail : WorkDetail {
        public CoreType Core;
        [FormerlySerializedAs("Amount")] public float IncreaseBy;

        protected override void OnStart() {
            base.OnStart();

            Machine.onProgress += UpdateCore;
        }

        protected override void OnStop() {
            base.OnStop();

            Machine.onProgress -= UpdateCore;
        }

        private void UpdateCore(float progress) {            
            Machine.Workers.ForEach(w => w.UpdateCore(Core, IncreaseBy));
        }

        public override SaveData Save() {
            if (base.Save() is not CoreChangeSaveData data) return base.Save();

            data.Core = Core;
            data.IncreaseBy = IncreaseBy;
            return data;
        }

        public override void Load(SaveData data) {
            base.Load(data);
            if (data is not CoreChangeSaveData saveData) return;
            
            Core = saveData.Core;
            IncreaseBy = saveData.IncreaseBy;
        }

        public class CoreChangeSaveData : WorkDetail.SaveData {
            public CoreType Core;
            public float IncreaseBy;
        }
    }
}