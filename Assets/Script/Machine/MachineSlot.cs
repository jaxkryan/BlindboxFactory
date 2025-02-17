#nullable enable
using System;
using MyBox;
using Script.HumanResource.Worker;
using UnityEngine;

namespace Script.Machine {
    public class MachineSlot : MonoBehaviour {
        [HideInInspector] public IWorker? Worker;
        [HideInInspector] public Vector3 Position;

        [SerializeField] private bool _forAll;
        [ConditionalField("_forAll", true)]
        [SerializeReference] private Worker _forWorker;
        MachineBase _machine;

        private void Awake() {
            _machine = GetComponentInParent<MachineBase>();
            Position = _machine.transform.position;
        }

        public bool CanAddWorker(IWorker worker) {
            if (_forAll) return true;
            return worker.GetType() == _forWorker.GetType();
        }

        public bool TryAddWorker(IWorker worker) {
            if (!CanAddWorker(worker)) return false;
            Worker = worker;
            return true;
        }
    }
}