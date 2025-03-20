using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AYellowpaper.SerializedCollections;
using Newtonsoft.Json;
using Script.HumanResource.Worker;
using UnityEngine;

namespace Script.Controller {
    public enum WorkerType {
        Worker,
        FactoryWorker
    }

    [Serializable]
    public class WorkerController : ControllerBase {
        [SerializeField] public List<Sprite> PortraitSprites;
        [SerializeReference] public SerializedDictionary<WorkerType, Worker> WorkerPrefabs; 
        public ReadOnlyDictionary<WorkerType, List<Worker>> WorkerList {
            get => new ReadOnlyDictionary<WorkerType, List<Worker>>(_workerList);
        }

        private Dictionary<WorkerType, List<Worker>> _workerList;

        public ReadOnlyDictionary<WorkerType, Dictionary<CoreType, int>> WorkerNeedsList {
            get => new(_workerNeedsList);
        }

        private Dictionary<WorkerType, Dictionary<CoreType, int>> _workerNeedsList;

        public WorkerController(Dictionary<WorkerType, List<Worker>> workerList,
            Dictionary<WorkerType, Dictionary<CoreType, int>> workerNeedsList) {
            _workerList = workerList;
            _workerNeedsList = workerNeedsList;
        }

        public WorkerController() : this(new Dictionary<WorkerType, List<Worker>>(),
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
                List<Worker> workerList;
                if (!_workerList.TryGetValue(type, out workerList)) workerList = new List<Worker>();
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
                List<Worker> workerList;
                if (!_workerList.TryGetValue(type, out workerList)) workerList = new List<Worker>();
                if (!workerList.Contains(worker)) return false;
                workerList.Remove(worker);
                _workerList[type] = workerList;
                return true;
            }
        }

        public override void Load() {
            if (!GameController.Instance.SaveManager.SaveData.TryGetValue(this.GetType().Name, out var saveData)
                || JsonConvert.DeserializeObject<SaveData>(saveData) is not SaveData data) return;

            foreach (var key in data.WorkerData.Keys) {
                if (!data.WorkerData.TryGetValue(key, out var list) || !WorkerPrefabs.TryGetValue(key, out var prefab)) continue;
                if (_workerList.TryGetValue(key, out var workerList) && workerList.Count > 0) {
                    var count = workerList.Count;
                    while (list.Count > 0 && count-- > 0) {
                        list.RemoveAt(0);
                    }
                }
                foreach (var w in list) {
                    if (!GameController.Instance.WorkerSpawner.SpawnAtPosition(key, w.Position, out var worker)) {
                        Debug.LogError($"Spawning {key} at {w.Position}");
                        continue;
                    }
                    
                    worker.Load(w);
                }
            }
        }
        public override void Save() {
            var newSave = new SaveData() { WorkerData = new() };
            foreach (var w in WorkerList) {
                if (newSave.WorkerData.ContainsKey(w.Key)) {
                    Debug.LogError("Duplicate worker type: " + w.Key);
                    continue;
                }
                var list = w.Value.Select(worker => worker.Save()).ToList();
                newSave.WorkerData.Add(w.Key, list);
            }

            if (!GameController.Instance.SaveManager.SaveData.TryGetValue(this.GetType().Name, out var saveData)
                || JsonConvert.DeserializeObject<SaveData>(saveData) is SaveData data) 
                GameController.Instance.SaveManager.SaveData.TryAdd(this.GetType().Name, JsonConvert.SerializeObject(newSave));
            else GameController.Instance.SaveManager.SaveData[this.GetType().Name] = JsonConvert.SerializeObject(newSave);
        }

        private class SaveData {
            public Dictionary<WorkerType, List<Worker.SaveData>> WorkerData;
        }
    }
}