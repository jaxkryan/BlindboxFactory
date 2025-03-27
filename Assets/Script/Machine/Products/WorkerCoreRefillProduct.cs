using System;
using System.Collections.Generic;
using Script.HumanResource.Worker;
using Script.Machine.ResourceManager;
using UnityEngine;

namespace Script.Machine.Products {
    [Serializable]
    public class WorkerCoreRefillProduct : SingleProductBase {
        [SerializeField] private MachineBase _machine;
        [SerializeField] private CoreType _core;
        [SerializeField] private float _amount;
        public override void OnProductCreated() {
            _machine.Workers.ForEach(w => w.UpdateCore(_core, _amount));
        }
    }
}