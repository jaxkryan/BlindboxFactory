using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ZLinq;
using AYellowpaper.SerializedCollections;
using BuildingSystem;
using Script.Controller.SaveLoad;
using Script.HumanResource.Worker;
using Script.Machine;
using Script.Utils;
using UnityEngine;

namespace Script.Controller {
    public enum WorkerType {
        Worker,
        FactoryWorker
    }

    [Serializable]
    public class WorkerController : ControllerBase {
        [SerializeField] public List<Sprite> PortraitSprites;
        [SerializeField] public SerializedDictionary<WorkerType, Worker> WorkerPrefabs;

        public ReadOnlyDictionary<WorkerType, List<Worker>> WorkerList {
            get => new ReadOnlyDictionary<WorkerType, List<Worker>>(_workerList);
        }

        private Dictionary<WorkerType, List<Worker>> _workerList;

        public SerializedDictionary<WorkerType, SerializedDictionary<CoreType, int>> WorkerNeedsList {
            get =>
                _workerNeedsList;
        }

        [SerializeField] private SerializedDictionary<WorkerType, SerializedDictionary<CoreType, int>> _workerNeedsList;

        public WorkerController(Dictionary<WorkerType, List<Worker>> workerList,
            Dictionary<WorkerType, Dictionary<CoreType, int>> workerNeedsList) {
            _workerList = workerList;
            _workerNeedsList = new(
                workerNeedsList
                    .AsValueEnumerable()
                    .Select(pair =>
                        new KeyValuePair<WorkerType, SerializedDictionary<CoreType, int>>
                            (pair.Key, new(pair.Value))).ToDictionary());
        }

        public WorkerController() : this(new Dictionary<WorkerType, List<Worker>>(),
            new Dictionary<WorkerType, Dictionary<CoreType, int>>()) { }

        public override void OnEnable() {
            base.OnEnable();
            Subscribe();
        }

        public override void OnDisable() {
            Unsubscribe();
            base.OnDisable();
        }

        private void Subscribe() {
            if (!Application.isPlaying) return;
            if (GameController.Instance.ConstructionLayer
                .TryGetComponent<ConstructionLayer>(out var constructionLayer)) {
                constructionLayer.onItemBuilt += OnMachineOnItemBuilt;
            }
            else Debug.LogWarning("Can't find construction layer");
        }

        private void Unsubscribe() {
            if (_isQuiting) return;
            try {
                if (GameController.Instance.ConstructionLayer is not null
                    && GameController.Instance.ConstructionLayer
                        .TryGetComponent<ConstructionLayer>(out var constructionLayer)) {
                    constructionLayer.onItemBuilt -= OnMachineOnItemBuilt;
                }
            }
            catch (System.Exception e) {
                Debug.LogWarning(e.Message);
                e.RaiseException();
            }
        }

        private bool _isQuiting = false;
        public override void OnApplicationQuit() => _isQuiting = true;

        private void OnMachineOnItemBuilt(GameObject obj) {
            if (!obj.TryGetComponent<MachineBase>(out var machine)) return;
            for (int i = 0; i < machine.SpawnWorkers; i++) {
                GameController.Instance.WorkerSpawner.Spawn(machine.SpawnWorkerType, out _);
            }
        }

        public event Action<Worker> onWorkerAdded = delegate { };
        public event Action<Worker> onWorkerRemoved = delegate { };

        public void SetNeeds(WorkerType type, params (CoreType core, int need)[] needs) {
            if (!_workerNeedsList.ContainsKey(type)) _workerNeedsList.Add(type, ToDictionary(needs));
            else _workerNeedsList[type] = ToDictionary(needs);

            SerializedDictionary<CoreType, int> ToDictionary((CoreType core, int need)[] needs) {
                var dicts = Enum.GetValues(typeof(CoreType)).Cast<CoreType>().ToDictionary(c => c, c => 0);

                needs.ForEach(n => { dicts[n.core] += n.need; });

                return new(dicts);
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

        public override void Load(SaveManager saveManager) {
            try {
                if (!saveManager.TryGetValue(this.GetType().Name, out var saveData)
                    || SaveManager.Deserialize<SaveData>(saveData) is not SaveData data) return;

                foreach (var key in data.WorkerData.Keys) {
                    Debug.LogWarning($"Loading worker key: {key}");
                    if (!data.WorkerData.TryGetValue(key, out var list)
                        || !WorkerPrefabs.TryGetValue(key, out _)) continue;
                    if (_workerList.TryGetValue(key, out var workerList) && workerList.Count > 0) {
                        for (var i = 0; i < (data.WorkerData[key]?.Count ?? 0); i++) {
                            var w = data.WorkerData[key]?.AsValueEnumerable().ElementAtOrDefault(i);
                            var worker = workerList?.AsValueEnumerable().ElementAtOrDefault(i);
                            if (worker is null || w is null) break;
                            worker.Load(w);
                        }
                    }
                }
            }
            catch (System.Exception ex) {
                Debug.LogError($"Cannot load {GetType()}");
                Debug.LogException(ex);
                ex.RaiseException();
                return;
            }
        }

        public override void Save(SaveManager saveManager) {
            try {
                var newSave = new SaveData() { WorkerData = new() };
                foreach (var w in WorkerList) {
                    if (w.Value.IsNullOrEmpty()) continue;
                    try {
                        if (newSave.WorkerData.ContainsKey(w.Key)) {
                            Debug.LogError("Duplicate worker type: " + w.Key);
                            continue;
                        }

                        var list = w.Value.AsValueEnumerable().Select(worker => worker.Save()).ToList();
                        newSave.WorkerData.Add(w.Key, list);
                    }
                    catch (System.Exception e) {
                        Debug.LogWarning(new System.Exception("Cannot save worker", e).RaiseException());
                    }
                }

                var serialized = SaveManager.Serialize(newSave);
                saveManager.AddOrUpdate(this.GetType().Name, serialized);
            }
            catch (System.Exception ex) {
                Debug.LogError($"Cannot save {GetType()}");
                Debug.LogException(ex);
                ex.RaiseException();
            }
        }

        private class SaveData {
            public Dictionary<WorkerType, List<Worker.SaveData>> WorkerData;
        }
    }
}