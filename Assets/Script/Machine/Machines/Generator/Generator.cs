using Script.Controller;
using System;
using Script.Utils;
using UnityEngine;

namespace Script.Machine.Machines.Generator {
    public class Generator : MachineBase {
        public int Power { get => _power; set => _power = value; }
        [SerializeField]private int _power = 0;

        public override void Load(MachineBaseData data) {
            base.Load(data);
            if (data is not GeneratorData saveData) return;
            _power = saveData.Power;
        }

        public override MachineBaseData Save() {
            var data = base.Save().CastToSubclass<GeneratorData, MachineBaseData>();
            if (data is null) return base.Save();
            
            data.Power = _power;
            return data;
        }

        public class GeneratorData : MachineBaseData {
            public int Power;
        }
    }
}