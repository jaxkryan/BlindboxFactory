using System;
using Script.HumanResource.Worker;
using UnityEngine;

namespace Script.Machine.WorkDetails {
    [Serializable]
    public class CoreChangeWorkDetail : WorkDetail {
        public CoreType Core;
        public float Amount;

        protected override void OnStart() {
            base.OnStart();

            Machine.onProgress += UpdateCore;
        }

        protected override void OnStop() {
            base.OnStop();
            
            Machine.onProgress -= UpdateCore;
        }

        private void UpdateCore(float progress) {
            Machine.Workers.ForEach(w => w.UpdateCore(Core, Amount));
        }
    }
}