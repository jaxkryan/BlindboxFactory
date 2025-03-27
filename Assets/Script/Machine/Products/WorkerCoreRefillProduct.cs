using System;
using System.Collections.Generic;
using Script.HumanResource.Worker;
using Script.Machine.ResourceManager;
using UnityEngine;

namespace Script.Machine.Products {
    [Serializable]
    public class WorkerCoreRefillProduct : SingleProductBase {
        [SerializeField] private CoreType _core;
        [SerializeField] private float _amount;
        public override void OnProductCreated() {
            _machine.Workers.ForEach(w => w.UpdateCore(_core, _amount));
        }

        public override IProduct.SaveData Save() {
            if (base.Save() is not WorkerCoreRefillData data) return base.Save();
            
            data.Core = _core;
            data.Amount = _amount;
            return data;
        }

        public override void Load(IProduct.SaveData saveData) {
            BaseLoad(saveData);

            if (saveData is not WorkerCoreRefillData data) return;
            
            _core = data.Core;
            _amount = data.Amount;
        }

        public class WorkerCoreRefillData : IProduct.SaveData {
            public CoreType Core;
            public float Amount;
        }
    }
}