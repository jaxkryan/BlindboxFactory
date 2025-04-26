using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ZLinq;
using AYellowpaper.SerializedCollections;
using BuildingSystem;
using BuildingSystem.Models;
using JetBrains.Annotations;
using Script.Controller.SaveLoad;
using Script.HumanResource.Worker;
using Script.Machine;
using Script.Machine.Products;
using Script.Utils;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Tilemaps;
using ZLinq.Linq;

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

        public ReadOnlyDictionary<RecoveryMachineKey, List<CoreType>> RecoveryMachines =>
            new(_recoverMachines);

        [SerializeField] private SerializedDictionary<RecoveryMachineKey, List<CoreType>> _recoverMachines = new();

        private bool _log => GameController.Instance.Log;

        public IEnumerable<MachineBase> FindMachinesOfType(Type type)
            => Machines.FindMachinesOfType(type);

        public List<BuildableItem> Buildables {
            get {
                var list = new List<BuildableItem>();
                list.Capacity = Categories.AsValueEnumerable().Select(c => c.buildables.Count).Sum();
                foreach (var bi in Categories.AsValueEnumerable().Select(c => c.buildables.AsValueEnumerable())) {
                    foreach (var b in bi) {
                        list.Add(b);
                    }
                }

                return list;
            }
        }

        public MachineController(List<MachineBase> machines) => _machines = machines.AsValueEnumerable().ToList();

        public MachineController() : this(new List<MachineBase>()) { }

        public override void OnAwake() {
            base.OnAwake();

            _constructionLayer = GameController.Instance.ConstructionLayer;
            _constructionLayer.TryGetComponent<ConstructionLayer>(out _constructionLayerScript);
            _path = new();
        }

        public override void OnStart() {
            base.OnStart();

            ResetRecoveryMachinesCache();
        }

        private void ResetRecoveryMachinesCache() {
            _recoverMachinesCache.Clear();
            foreach (var machine in _machines.AsValueEnumerable()) {
                if (machine.PrefabName == null) continue;
                var recoverable = RecoveryMachines.AsValueEnumerable()
                    .Where(m => m.Key.Machine.Name == machine.PrefabName);
                foreach (var r in recoverable) {
                    if (_recoverMachinesCache.TryGetValue(machine, out var value)) {
                        var mcr = (from core in r.Value
                            from worker in r.Key.Worker
                            select new MachineCoreRecovery() { Core = core, Worker = worker }).ToList();
                        value.Recoveries.AddRange(mcr);
                        value.ForWorker.AddRange(r.Key.Worker);
                    }
                    else {
                        var mcr = (from core in r.Value
                            from worker in r.Key.Worker
                            select new MachineCoreRecovery() { Core = core, Worker = worker }).ToList();

                        _recoverMachinesCache.Add(machine, (mcr, r.Key.Worker));
                    }
                }
            }
        }

        private Dictionary<MachineBase, (List<MachineCoreRecovery> Recoveries, List<WorkerType> ForWorker)>
            _recoverMachinesCache = new();

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

        public bool IsRecoveryMachine(MachineBase machine, WorkerType workerType) =>
            IsRecoveryMachine(machine, out var forWorkers, out _) && forWorkers.Contains(workerType);

        public bool IsRecoveryMachine(MachineBase machine) => IsRecoveryMachine(machine, out _, out _);

        public bool IsRecoveryMachine(MachineBase machine, out List<WorkerType> forWorkers) =>
            IsRecoveryMachine(machine, out forWorkers, out _);

        public bool IsRecoveryMachine(MachineBase machine, WorkerType workerType,
            out List<MachineCoreRecovery> recoveries) {
            var ret = IsRecoveryMachine(machine, out var forWorkers, out recoveries) && forWorkers.Contains(workerType);
            if (!ret) recoveries = default;
            return ret;
        }

        public bool IsRecoveryMachine(MachineBase machine, out List<WorkerType> forWorkers,
            out List<MachineCoreRecovery> recoveries) {
            forWorkers = null;
            recoveries = null;
            // if (machine.PrefabName == null) return false;
            //
            // var prefab = GetPrefab(machine);
            // if (prefab is null) return false;
            //
            // var keys = RecoveryMachines.Keys?.AsValueEnumerable().Where(r => r.Machine == prefab) ?? new();

            if (!_recoverMachinesCache.TryGetValue(machine, out var value)) return false;
            forWorkers = value.ForWorker;
            recoveries = value.Recoveries;
            return true;

            // recoveries = RecoveryMachines
            //     .AsValueEnumerable()
            //     .Where(r => keys.Contains(r.Key))
            //     .Select(r => {
            //         var list = new List<MachineCoreRecovery>();
            //         foreach (var worker in r.Key.Worker) {
            //             foreach (var core in r.Value) {
            //                 list.Add(new MachineCoreRecovery() { Worker = worker, Core = core });
            //             }
            //         }
            //
            //         return list;
            //     })
            //     .Aggregate(new List<MachineCoreRecovery>(), (x, y) => x.AsValueEnumerable().Concat(y).ToList());
            // forWorkers = RecoveryMachines
            //     .AsValueEnumerable()
            //     .Where(r => keys.Contains(r.Key))
            //     .Select(r => r.Key.Worker)
            //     .Aggregate(new List<WorkerType>(), (x, y) => x.AsValueEnumerable().Concat(y).ToList());
            // return recoveries.Count > 0;
        }

        [CanBeNull]
        public BuildableItem GetPrefab(MachineBase machine) =>
            machine.PrefabName == null
                ? null
                : Buildables.AsValueEnumerable().FirstOrDefault(b => b.Name == machine.PrefabName);

        public void AddMachine(MachineBase machine) {
            _machines.Add(machine);
            ResetRecoveryMachinesCache();
            onMachineAdded?.Invoke(machine);
        }

        public void RemoveMachine(MachineBase machine) {
            _machines.Remove(machine);
            ResetRecoveryMachinesCache();
            onMachineRemoved?.Invoke(machine);
        }

        public ValueEnumerable<ListWhere<MachineBase>, MachineBase>
            FindRecoveryMachine<TWorker>(CoreType core, TWorker worker = null)
            where TWorker : Worker {
            // foreach (var machine in workableMachines) {
            //     var key = _recoverMachines.Keys.FirstOrDefault(k => k.GetType() == machine.GetType());
            //     if (key == null || !_recoverMachines.TryGetValue(key, out var recoveryInfo)) continue;
            //     if (recoveryInfo.All(r => r.Core != core)) continue;
            //     if (worker != null && recoveryInfo.All(r => r.Worker != IWorker.ToWorkerType(worker))) continue;
            //
            //     list.Add(machine);
            // }

            // foreach (var machine in workableMachines) {
            //     if (IsRecoveryMachine(machine, out var workerType, out _)
            //         && workerType.Contains(IWorker.ToWorkerType(worker)))
            //         list.Add(machine);
            // }
            //
            // return list;

            var type = IWorker.ToWorkerType(worker);
            return FindWorkableMachines(worker as Worker).AsValueEnumerable().Where(m =>
                IsRecoveryMachine(m, out var workerType, out var recoveries)
                && workerType.AsValueEnumerable().Contains(type)
                && recoveries.AsValueEnumerable().Any(r => r.Core == core));
        }

        // public IEnumerable<MachineBase>  FindWorkableMachines(
        //     [CanBeNull] Worker worker = null, [CanBeNull] IEnumerable<MachineBase> machines = null) {
        //     if (machines == null) return FindWorkableMachines(_machines, worker); 
        //     return FindWorkableMachines(machines, worker);
        // }

        private NavMeshPath _path;

        public List<MachineBase> FindWorkableMachines(
            [CanBeNull] Worker worker = null, [CanBeNull] List<MachineBase> machines = null) {
            if (machines == null) machines = _machines;
            var list = new List<MachineBase>(machines.Count);
            foreach (var m in _machines) {
                if (!m.IsWorkable) continue;
                if (m.Product is NullProduct) continue;
                if (m.Product is BlindBox { BoxTypeName: BoxTypeName.Null }) continue;
                if (worker == null) {
                    if (m.Slots.AsValueEnumerable().TryGetNonEnumeratedCount(out var count1) &&
                        m.Workers.AsValueEnumerable().TryGetNonEnumeratedCount(out var count2)) {
                        if (count1 <= count2) continue;
                    }
                    else {
                        if (m.Slots.AsValueEnumerable().Count() <=
                            m.Workers.AsValueEnumerable().Count()) continue;
                    }

                    list.Add(m);
                }
                else {
                    if (!worker.Agent.isOnNavMesh) continue;
                    if (!worker.Agent.CalculatePath(GetNavMeshHit(worker), _path)) continue;

                    var canAdd = false;
                    foreach (var s in m.Slots.AsValueEnumerable()) {
                        if (!s.CanAddWorker(worker)) continue;

                        canAdd = true;
                        break;
                    }

                    if (canAdd)
                        list.Add(m);
                }

            }
            
            return list;

            // return machines.AsValueEnumerable().Where(m => m.IsWorkable
            //                                                && 
            //                                                (worker == null
            //                                                    ? 
            //                                                    m.Slots.AsValueEnumerable().Count() >
            //                                                      m.Workers.AsValueEnumerable().Count()
            //                                                    : 
            //                                                    (m.Slots.AsValueEnumerable().All(s =>
            //                                                    !(s.CanAddWorker(worker)
            //                                                     && worker.Agent.isOnNavMesh == true
            //                                                     && worker.Agent.CalculatePath(GetNavMeshHit(worker),
            //                                                         _path))
            //                                                        ))
            //                                                )
            //                                                && m.Product is not NullProduct
            //                                                && m.Product is not BlindBox { BoxTypeName: BoxTypeName.Null }
            //                                                );
        }

        private Vector3 GetNavMeshHit(Worker worker) {
            NavMeshHit hit;
            if (!NavMesh.SamplePosition(worker.transform.position, out hit, Single.MaxValue, NavMesh.AllAreas))
                return Vector3.zero;
            return hit.position;
        }

        [Serializable]
        public struct MachineCoreRecovery {
            public CoreType Core;
            public WorkerType Worker;
        }

        public override void OnValidate() {
            base.OnValidate();

            Buildables.AsValueEnumerable().Select(b => b.Name)
                .GroupBy(n => n)
                .ForEach(g => {
                    if (g.AsValueEnumerable().Count() > 1) {
                        Debug.LogError("Buildable name conflict: " + g.Key);
                    }
                });

            foreach (var machine in Buildables) {
                if (UnlockMachines.ContainsKey(machine.Name)) continue;
                else _unlockMachines.Add(machine.Name, false);
            }

            var redundantKeys = UnlockMachines.AsValueEnumerable()
                .Where(m => Buildables.AsValueEnumerable().All(b => b.Name != m.Key)).Select(m => m.Key);
            redundantKeys.ForEach(k => _unlockMachines.Remove(k));

            ResetRecoveryMachinesCache();
        }

        public override void Load(SaveManager saveManager) {
            try {
                if (!saveManager.TryGetValue(this.GetType().Name, out var saveData)
                    || SaveManager.Deserialize<SaveData>(saveData) is not SaveData data)
                    return;


                _unlockMachines = new(data.UnlockMachines);
                if (_constructionLayerScript == null || _constructionLayerScript == default) {
                    return;
                }

                if (_log) Debug.Log($"Machine count: {data.Machines.Count}");
                if (_log)
                    Debug.Log(
                        $"Buildable prefab list: {string.Join(", ", Buildables.AsValueEnumerable().Select(b => b.Name))}");

                try {
                    if (_log) Debug.Log($"Machine count: {data.Machines.Count}");
                    if (_log)
                        Debug.Log(
                            $"Buildable prefab list: {string.Join(", ", Buildables.AsValueEnumerable().Select(b => b.Name))}");

                    foreach (var m in data.Machines) {
                        try {
                            if (_log) Debug.Log($"Building prefab: {m.PrefabName}");
                            var prefab = Buildables.AsValueEnumerable().FirstOrDefault(b => b.Name == m.PrefabName);
                            if (prefab == default) continue;

                            if (_log) Debug.Log($"Building machine: {prefab.Name} at {m.Position}");
                            var constructedGameObject = _constructionLayerScript.Build(m.Position, prefab);

                            if (constructedGameObject is null
                                || !constructedGameObject.TryGetComponent<MachineBase>(out var machine)) continue;
                            machine.Load(m);
                        }
                        catch (System.Exception e) {
                            Debug.LogWarning(new System.Exception($"Cannot load machine {m.PrefabName}", e));
                            e.RaiseException();
                        }
                    }
                }
                catch (System.Exception ex) {
                    Debug.LogError($"Cannot load {nameof(MachineController)}");
                    Debug.LogException(ex);
                    ex.RaiseException();
                }
            }
            catch (System.Exception e) {
                Debug.LogError($"Cannot load {GetType()}");
                Debug.LogException(e);
                e.RaiseException();

                return;
            }
        }

        public override void Save(SaveManager saveManager) {
            var newSave = new SaveData() {
                Machines = new(),
                UnlockMachines = _unlockMachines
            };
            Machines.ForEach(m => {
                if (_log) Debug.Log($"Saving machine: {m.name}");
                try {
                    var machine = m.Save();
                    newSave.Machines.Add(machine);
                }
                catch (System.Exception e) {
                    Debug.LogWarning(new System.Exception($"Cannot save machine {m.name}", e));
                    e.RaiseException();
                }
            });
            // Debug.LogWarning(SaveManager.Serialize(newSave));


            try {
                var serialized = SaveManager.Serialize(newSave);
                saveManager.AddOrUpdate(this.GetType().Name, serialized);
                // if (!saveManager.TryGetValue(this.GetType().Name, out var saveData)
                //     || SaveManager.Deserialize<SaveData>(saveData) is SaveData data)
                //     saveManager.SaveData.TryAdd(this.GetType().Name,
                //         SaveManager.Serialize(newSave));
                // else
                //     saveManager.SaveData[this.GetType().Name]
                //         = SaveManager.Serialize(newSave);
            }
            catch (System.Exception ex) {
                Debug.LogError($"Cannot save {GetType()}");
                Debug.LogException(ex);
                ex.RaiseException();
            }
        }

        private class SaveData {
            public List<MachineBase.MachineBaseData> Machines;
            public Dictionary<string, bool> UnlockMachines;
        }
    }

    [Serializable]
    public class RecoveryMachineKey : IEquatable<RecoveryMachineKey> {
        [SerializeField] public BuildableItem Machine;
        [SerializeField] public List<WorkerType> Worker;

        public bool Equals(RecoveryMachineKey other) {
            return Machine == other?.Machine && Equals(Worker, other?.Worker);
        }
    }
}