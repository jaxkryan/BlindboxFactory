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

namespace Script.Controller {
    [Serializable]
    public class MachineController : ControllerBase {
        [SerializeField] public List<BuildableItem> Buildables;
        public List<MachineBase> Machines { get; private set; }

        public ReadOnlyDictionary<MachineBase, List<MachineCoreRecovery>> RecoveryMachines =>
            new((Dictionary<MachineBase, List<MachineCoreRecovery>>)_recoverMachines);

        [SerializeField] SerializedDictionary<MachineBase, List<MachineCoreRecovery>> _recoverMachines;

        public List<MachineBase> FindMachinesOfType(Type type) {
            if (!type.IsSubclassOf(typeof(MachineBase))) return new();
            
            return Machines.Where(m => m.GetType() == type).ToList();
        }
        
        public MachineController(List<MachineBase> machines) => Machines = machines.ToList();
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

        public override void OnValidate() {
            base.OnValidate();

            if (Buildables.Select(b => b.Name).GroupBy(n => n).Any(n => n.Count() > 1)) {
                Debug.LogError("Buildable names conflict");
            }
        }

        public override void Load() {
            if (!GameController.Instance.SaveManager.SaveData.TryGetValue(this.GetType().Name, out var saveData)
                || JsonConvert.DeserializeObject<SaveData>(saveData) is not SaveData data) return;

            var construction = GameController.Instance.ConstructionLayer.GetComponent<ConstructionLayer>();
            if (construction == null || construction == default) {
                Debug.LogError($"No collision layer found for {GameController.Instance.CollisionLayer.name}");
                return;
            }
            foreach (var m in data.Machines) {
                var prefab = Buildables.FirstOrDefault(b => b.Name == m.PrefabName);
                if (prefab == default) continue;
                
                var worldPos = GameController.Instance.ConstructionLayer.CellToWorld(m.Position.ToVector3Int());
                var constructedGO = construction.Build(worldPos, prefab);

                if (constructedGO is null
                    || constructedGO == default
                    || !constructedGO.TryGetComponent<MachineBase>(out var machine)) continue;
                machine.Load(m);
            }
        }
        public override void Save() {
            var newSave = new SaveData() { Machines = new() };
            Machines.ForEach(m => {
                var machine = m.Save();
                newSave.Machines.Add(machine);
            });
            // Debug.LogWarning(JsonConvert.SerializeObject(newSave));
            
            if (!GameController.Instance.SaveManager.SaveData.TryGetValue(this.GetType().Name, out var saveData)
                || JsonConvert.DeserializeObject<SaveData>(saveData) is SaveData data) 
                GameController.Instance.SaveManager.SaveData.TryAdd(this.GetType().Name, JsonConvert.SerializeObject(newSave));
            else GameController.Instance.SaveManager.SaveData[this.GetType().Name] = JsonConvert.SerializeObject(newSave);
        }

        private class SaveData {
            public List<MachineBase.MachineBaseData> Machines;
        }

    }
}