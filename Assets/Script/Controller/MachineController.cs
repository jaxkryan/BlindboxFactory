using System;
using System.Collections.Generic;
using System.Linq;
using Script.HumanResource.Worker;
using Script.Machine;

namespace Script.Controller {
    public class MachineController : ControllerBase {
        public HashSet<MachineBase> Machines { get; private set; }
        
        public MachineController(List<MachineBase> machines) => Machines = machines.ToHashSet();
        public MachineController() : this (new List<MachineBase>()) { }
        
        public event Action<IMachine> onMachineAdded = delegate { };
        public event Action<IMachine> onMachineRemoved = delegate { };

        public void AddMachine(MachineBase machine) {
            Machines.Add(machine);
            onMachineAdded?.Invoke(machine);
        }

        public void RemoveMachine(MachineBase machine) {
            Machines.Remove(machine);
            onMachineRemoved?.Invoke(machine);
        }

        public IEnumerable<MachineBase> FindWorkableMachines() => Machines.Where(m => m.Slots.Count() > m.Workers.Count());

        public IEnumerable<MachineBase> FindWorkableMachines(IWorker worker) => FindWorkableMachines()
            .Where(m => m.Slots.Any(s => s.CanAddWorker(worker)));

    }
}