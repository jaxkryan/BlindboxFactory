using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AYellowpaper.SerializedCollections;
using BuildingSystem;
using BuildingSystem.Models;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Script.Controller.SaveLoad;
using Script.HumanResource.Worker;
using Script.Machine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Tilemaps;

namespace Script.Controller {
    [Serializable]
    public class MachineController : ControllerBase {
        [SerializeField] public List<BuildableCategory> Categories = new();

        public ReadOnlyDictionary<string, bool> UnlockMachines => new(_unlockMachines);
        [SerializeField] private SerializedDictionary<string, bool> _unlockMachines = new();

        public ReadOnlyCollection<MachineBase> Machines {
            get => _machines.AsReadOnly();
        }

        private List<MachineBase> _machines;

        public ReadOnlyDictionary<MachineBase, List<MachineCoreRecovery>> RecoveryMachines =>
            new(_recoverMachines);

        [SerializeField] private SerializedDictionary<MachineBase, List<MachineCoreRecovery>> _recoverMachines = new();

        public List<MachineBase> FindMachinesOfType(Type type) {
            if (!type.IsSubclassOf(typeof(MachineBase))) return new();

            return Machines.Where(m => m.GetType() == type).ToList();
        }

        public List<BuildableItem> Buildables {
            get {
                var cat = Categories;
                var ret = new List<BuildableItem>();
                cat.ForEach(c => ret.AddRange(c.buildables));

                return ret;
            }
        }

        public MachineController(List<MachineBase> machines) => _machines = machines.ToList();

        public MachineController() : this(new List<MachineBase>()) { }

        public override void OnAwake() {
            base.OnAwake();

            _constructionLayer = GameController.Instance.ConstructionLayer;
            _constructionLayer.TryGetComponent<ConstructionLayer>(out _constructionLayerScript);
        }

        private Tilemap _constructionLayer;
        private ConstructionLayer _constructionLayerScript;

        public event Action<MachineBase> onMachineAdded = delegate { };
        public event Action<MachineBase> onMachineRemoved = delegate { };
        public event Action<string> onMachineUnlocked = delegate { };

        public void UnlockMachine(string name) {
            if (!_unlockMachines.TryGetValue(name, out var isUnlocked)) return;
            if (isUnlocked) return;
            _unlockMachines[name] = true;
            onMachineUnlocked?.Invoke(name);
        }

        public bool IsRecoveryMachine(MachineBase machine, out List<MachineCoreRecovery> recoveries) { 
            recoveries = default;
            if (machine.PrefabName == null) return false;

            var prefab = Buildables.FirstOrDefault(b => b.Name == machine.PrefabName)?.gameObject;
            if (prefab is null || prefab.TryGetComponent<MachineBase>(out var machinePrefab)) return false;

            var keys = RecoveryMachines.Keys?.Where(r => ReferenceEquals(r,machinePrefab))?.ToList() ?? new ();

            recoveries = RecoveryMachines
                .Where(r => keys.Contains(r.Key))
                .Select(r => r.Value)
                .Aggregate(new List<MachineCoreRecovery>(), (x, y) => x.Concat(y).ToList());
            return recoveries.Any();
        }

        public void AddMachine(MachineBase machine) {
            _machines.Add(machine);
            onMachineAdded?.Invoke(machine);
        }

        public void RemoveMachine(MachineBase machine) {
            _machines.Remove(machine);
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

        public IEnumerable<MachineBase> FindWorkableMachines(Worker worker,
            [CanBeNull] IEnumerable<MachineBase> machines = null) =>
            FindWorkableMachines(machines)
                .Where(m => m.Slots.Any(s =>
                    s.CanAddWorker(worker) && worker.Agent.isOnNavMesh == true && worker.Agent.CalculatePath(GetNavMeshHit(worker), new())));

        private Vector3 GetNavMeshHit(Worker worker) {
            NavMeshHit hit;
            if (!NavMesh.SamplePosition(worker.transform.position, out hit, Single.MaxValue, 1)) return Vector3.zero;
            return hit.position;
        }

        [Serializable]
        public struct MachineCoreRecovery {
            public CoreType Core;
            public WorkerType Worker;
        }

        public override void OnValidate() {
            base.OnValidate();

            if (Buildables.Select(b => b.Name).GroupBy(n => n).Any(n => n.Count() > 1)) {
                Debug.LogError("Buildable names conflict");
            }

            foreach (var machine in Buildables) {
                if (UnlockMachines.ContainsKey(machine.Name)) continue;
                else _unlockMachines.Add(machine.Name, false);
            }

            var redundantKeys = UnlockMachines.Where(m => Buildables.All(b => b.Name != m.Key)).Select(m => m.Key);
            redundantKeys.ForEach(k => _unlockMachines.Remove(k));
        }

        public override void Load(SaveManager saveManager) {
            try {
                if (!saveManager.SaveData.TryGetValue(this.GetType().Name, out var saveData)
                    || SaveManager.Deserialize<SaveData>(saveData) is not SaveData data)
                    return;


                _unlockMachines = new(data.UnlockMachines);
                if (_constructionLayerScript == null || _constructionLayerScript == default) { return; }

                Debug.LogWarning($"Machine count: {data.Machines.Count}");
                Debug.LogWarning($"Buildable prefab list: {string.Join(", ", Buildables.Select(b => b.Name))}");

                UnityMainThreadDispatcher.Instance().Enqueue(() => {

                    Debug.Log($"Machine count: {data.Machines.Count}");
                    Debug.Log($"Buildable prefab list: {string.Join(", ", Buildables.Select(b => b.Name))}");

                    foreach (var m in data.Machines) {
                        Debug.Log($"Building prefab: {m.PrefabName}");
                        var prefab = Buildables.FirstOrDefault(b => b.Name == m.PrefabName);
                        if (prefab == default) continue;

                        var worldPos = _constructionLayer.CellToWorld(m.Position.ToVector3Int());
                        Debug.Log($"Building machine: {prefab.Name} at {worldPos}");
                        var constructedGameObject = _constructionLayerScript.Build(worldPos, prefab);

                        if (constructedGameObject is null
                            || !constructedGameObject.TryGetComponent<MachineBase>(out var machine)) continue;
                        machine.Load(m);
                    }
                });
            }
            catch (System.Exception e) {
                Debug.LogError($"Cannot load {GetType()}");
                Debug.LogException(e);
                return;
            }
        }

        public override void Save(SaveManager saveManager) {
            var newSave = new SaveData() {
                Machines = new(),
                UnlockMachines = _unlockMachines
            };
            Machines.ForEach(m => {
                var machine = m.Save();
                newSave.Machines.Add(machine);
            });
            // Debug.LogWarning(SaveManager.Serialize(newSave));


            try {
                if (!saveManager.SaveData.TryGetValue(this.GetType().Name, out var saveData)
                    || SaveManager.Deserialize<SaveData>(saveData) is SaveData data)
                    saveManager.SaveData.TryAdd(this.GetType().Name,
                        SaveManager.Serialize(newSave));
                else
                    saveManager.SaveData[this.GetType().Name]
                        = SaveManager.Serialize(newSave);
            }
            catch (System.Exception ex) {
                Debug.LogError($"Cannot save {GetType()}");
                Debug.LogException(ex);
            }
        }

        private class SaveData {
            public List<MachineBase.MachineBaseData> Machines;
            public Dictionary<string, bool> UnlockMachines;
        }
    }
}