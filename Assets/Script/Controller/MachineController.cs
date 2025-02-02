using System.Collections.Generic;
using System.Linq;
using Script.HumanResource.Worker;
using Script.Machine;

namespace Script.Controller {
    public class MachineController {
        public HashSet<IMachine> Machines { get; } = new();

        public IEnumerable<IMachine> FindWorkableMachines() => Machines.Where(m => m.Slots.Count() > m.Workers.Count());

        public IEnumerable<IMachine> FindWorkableMachines(IWorker worker) => FindWorkableMachines()
            .Where(m => m.Slots.Any(s => s.CanAddWorker(worker)));

    }
}