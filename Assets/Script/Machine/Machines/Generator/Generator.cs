using System;
using UnityEngine;

namespace Script.Machine.Machines.Generator {
    public class Generator : MachineBase {
        public int Power { get => _power; }
        [SerializeField]private int _power = 0;
    }
}