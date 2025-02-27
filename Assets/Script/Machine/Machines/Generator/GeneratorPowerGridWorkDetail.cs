using System;
using Script.Controller;
using Script.Resources;
using UnityEngine;

namespace Script.Machine.Machines.Generator {
    [Serializable]
    public class GeneratorPowerGridWorkDetail : WorkDetail {
        [SerializeField] int _energy;

        protected override bool IsSetUp() {
            if (!base.IsSetUp()) return false;
            if (Machine is not Generator) {
                Debug.LogError("Machine is not generator");
                return false;
            }

            return true;
        }

        protected override void OnStart() {
            base.OnStart();

            var controller = GameController.Instance.ResourceController;
            if (controller.TryGetAmount(Resource.Energy, out var currentEnergy)) {
                controller.TrySetAmount(Resource.Energy, currentEnergy + _energy);
            }
        }

        protected override void OnStop() { 
            base.OnStop();

            var controller = GameController.Instance.ResourceController;
            if (controller.TryGetAmount(Resource.Energy, out var currentEnergy)) {
                controller.TrySetAmount(Resource.Energy, currentEnergy - _energy);
            }
        }
    }
}