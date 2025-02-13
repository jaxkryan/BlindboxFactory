using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Script.HumanResource.Worker;

namespace Script.Controller {
    public enum WorkerType {
        Worker
    }

    public class WorkerController {
        public ReadOnlyDictionary<WorkerType, HashSet<Worker>> WorkerList {
            get => new ReadOnlyDictionary<WorkerType, HashSet<Worker>>(_workerList);
        }

        private Dictionary<WorkerType, HashSet<Worker>> _workerList;

        public ReadOnlyDictionary<WorkerType, (int HappinessNeeds, int HungerNeeds)> WorkerNeedsList {
            get => new ReadOnlyDictionary<WorkerType, (int HappinessNeeds, int HungerNeeds)>(_workerNeedsList);
        }

        private Dictionary<WorkerType, (int HappinessNeeds, int HungerNeeds)> _workerNeedsList;

        public WorkerController(Dictionary<WorkerType, HashSet<Worker>> workerList,
            Dictionary<WorkerType, (int HappinessNeeds, int HungerNeeds)> workerNeedsList) {
            _workerList = workerList;
            _workerNeedsList = workerNeedsList;
        }

        public WorkerController() : this(new Dictionary<WorkerType, HashSet<Worker>>(),
            new Dictionary<WorkerType, (int HappinessNeeds, int HungerNeeds)>()) { }

        public void SetNeeds(WorkerType type, int happinessNeeds, int hungerNeeds) {
            if (!_workerNeedsList.ContainsKey(type)) _workerNeedsList.Add(type, (happinessNeeds, hungerNeeds));
            else _workerNeedsList[type] = (happinessNeeds, hungerNeeds);
        }
        
        public void AddWorker<TWorker>(TWorker worker) where TWorker : Worker {
            switch (typeof(TWorker)) {
                case Type @base when @base == typeof(Worker):
                    if (!TryAddWorker(WorkerType.Worker, worker)) throw new Exception("Cannot add worker");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

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
            switch (typeof(TWorker)) {
                case Type @base when @base == typeof(Worker):
                    if (!TryRemoveWorker(WorkerType.Worker, worker)) throw new Exception("Worker not found");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            bool TryRemoveWorker(WorkerType type, Worker worker) {
                HashSet<Worker> workerList;
                if (!_workerList.TryGetValue(type, out workerList)) workerList = new HashSet<Worker>();
                if (!workerList.Contains(worker)) return false;
                workerList.Remove(worker);
                _workerList[type] = workerList;
                return true;
            }
        }
    }
}