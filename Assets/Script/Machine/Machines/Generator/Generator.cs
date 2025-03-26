using System;
using UnityEngine;

namespace Script.Machine.Machines.Generator {
    public class Generator : MachineBase {
        public int Power { get => _power; set => _power = value; }
        [SerializeField]private int _power = 0;

        private void Start()
        {
            base.Start();
        }
    }
}