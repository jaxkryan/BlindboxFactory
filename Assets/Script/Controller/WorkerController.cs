using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Script.HumanResource.Worker;

namespace Script.Controller {
    public enum WorkerType {
        Worker
    }

    public class WorkerController : ControllerBase {
        public ReadOnlyDictionary<WorkerType, HashSet<Worker>> WorkerList {
            get => new ReadOnlyDictionary<WorkerType, HashSet<Worker>>(_workerList);
        }

        private Dictionary<WorkerType, HashSet<Worker>> _workerList;

        public ReadOnlyDictionary<WorkerType, Dictionary<CoreType, int>> WorkerNeedsList {
            get => new(_workerNeedsList);
        }

        private Dictionary<WorkerType, Dictionary<CoreType, int>> _workerNeedsList;

        public WorkerController(Dictionary<WorkerType, HashSet<Worker>> workerList,
            Dictionary<WorkerType, Dictionary<CoreType, int>> workerNeedsList) {
            _workerList = workerList;
            _workerNeedsList = workerNeedsList;
        }

        public WorkerController() : this(new Dictionary<WorkerType, HashSet<Worker>>(),
            new Dictionary<WorkerType, Dictionary<CoreType, int>>()) { }

        public event Action<Worker> onWorkerAdded = delegate { };
        public event Action<Worker> onWorkerRemoved = delegate { };

        public void SetNeeds(WorkerType type, params (CoreType core, int need)[] needs) {
            if (!_workerNeedsList.ContainsKey(type)) _workerNeedsList.Add(type, ToDictionary(needs));
            else _workerNeedsList[type] = ToDictionary(needs);

            Dictionary<CoreType, int> ToDictionary((CoreType core, int need)[] needs) {
                var dicts = Enum.GetValues(typeof(CoreType)).Cast<CoreType>().ToDictionary(c => c, c => 0);

                needs.ForEach(n => {
                    dicts[n.core] += n.need;
                });

                return dicts;
            }
        }

        public void AddWorker<TWorker>(TWorker worker) where TWorker : Worker {
            if (!TryAddWorker(IWorker.ToWorkerType(worker), worker)) throw new System.Exception("Cannot add worker");

            onWorkerAdded?.Invoke(worker);

            bool TryAddWorker(WorkerType type, Worker worker) {
                HashSet<Worker> workerList;
                if (!_workerList.TryGetValue(type, out workerList)) workerList = new HashSet<Worker>();
                if (workerList.Contains(worker)) return false;
                workerList.Add(worker);
                _workerList[type] = workerList;
                return true;
            }
        }

        public void RemoveWorker<TWorker>(TWorker worker) where TWorker : Worker {
            if (!TryRemoveWorker(IWorker.ToWorkerType(worker), worker)) throw new System.Exception("Worker not found");
            onWorkerRemoved?.Invoke(worker);

            bool TryRemoveWorker(WorkerType type, Worker worker) {
                HashSet<Worker> workerList;
                if (!_workerList.TryGetValue(type, out workerList)) workerList = new HashSet<Worker>();
                if (!workerList.Contains(worker)) return false;
                workerList.Remove(worker);
                _workerList[type] = workerList;
                return true;
            }
        }

        public override void Load() { throw new NotImplementedException(); }
        public override void Save() { throw new NotImplementedException(); }
    }
}