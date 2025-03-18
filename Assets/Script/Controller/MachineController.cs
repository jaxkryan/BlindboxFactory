using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AYellowpaper.SerializedCollections;
using JetBrains.Annotations;
using Script.HumanResource.Worker;
using Script.Machine;
using UnityEngine;

namespace Script.Controller {
    [Serializable]
    public class MachineController : ControllerBase {
        public HashSet<MachineBase> Machines { get; private set; }

        public ReadOnlyDictionary<MachineBase, List<MachineCoreRecovery>> RecoveryMachines =>
            new((Dictionary<MachineBase, List<MachineCoreRecovery>>)_recoverMachines);

        [SerializeField] SerializedDictionary<MachineBase, List<MachineCoreRecovery>> _recoverMachines;

        public HashSet<MachineBase> FindMachinesOfType(Type type) {
            if (!type.IsSubclassOf(typeof(MachineBase))) return new();
            
            return Machines.Where(m => m.GetType() == type).ToHashSet();
        }
        
        public MachineController(List<MachineBase> machines) => Machines = machines.ToHashSet();
        public MachineController() : this(new List<MachineBase>()) { }

        public event Action<MachineBase> onMachineAdded = delegate { };
        public event Action<MachineBase> onMachineRemoved = delegate { };

        public void AddMachine(MachineBase machine) {
            Machines.Add(machine);
            onMachineAdded?.Invoke(machine);
        }

        public void RemoveMachine(MachineBase machine) {
            Machines.Remove(machine);
            onMachineRemoved?.Invoke(machine);
        }

        public IEnumerable<MachineBase> FindRecoveryMachine<TWorker>(CoreType core, TWorker worker = null)
            where TWorker : Worker {
            var workableMachines = worker == null ? FindWorkableMachines() : FindWorkableMachines(worker);
            var list = new List<MachineBase>();

            foreach (var machine in workableMachines) {
                var key = _recoverMachines.Keys.FirstOrDefault(k => k.GetType() == machine.GetType());
                if (key == null || !_recoverMachines.TryGetValue(key, out var recoveryInfo)) continue;
                if (recoveryInfo.All(r => r.Core != core)) continue;
                if (worker != null && recoveryInfo.All(r => r.Worker != IWorker.ToWorkerType(worker))) continue;

                list.Add(machine);
            }

            return list;
        }

        public IEnumerable<MachineBase> FindWorkableMachines([CanBeNull] IEnumerable<MachineBase> machines = null) {
            if (machines == null) machines = Machines;
            return machines.Where(m => m.IsWorkable && m.Slots.Count() > m.Workers.Count());
        }

        public IEnumerable<MachineBase> FindWorkableMachines(IWorker worker, [CanBeNull] IEnumerable<MachineBase> machines = null) =>
            FindWorkableMachines(machines)
                .Where(m => m.Slots.Any(s => s.CanAddWorker(worker)));

        [Serializable]
        public struct MachineCoreRecovery {
            public CoreType Core;
            public WorkerType Worker;
        }

        public override void Load() { 
            //throw new NotImplementedException(); 
        }
        public override void Save() { 
            //throw new NotImplementedException(); 
        }
    }
}