using Script.Controller;
using System;
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
            if (base.Save() is not GeneratorData saveData) return base.Save();
            
            saveData.Power = _power;
            return saveData;
        }

        public class GeneratorData : MachineBaseData {
            public int Power;
        }

        public Generator.GeneratorData GetGeneratorData()
        {
            return new GeneratorData { Power = _power };
        }
    }
}