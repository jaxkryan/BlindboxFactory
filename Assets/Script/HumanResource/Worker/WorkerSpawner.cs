using System;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using Script.Controller;
using Script.Machine;
using Script.Utils;
using UnityEngine;

namespace Script.HumanResource.Worker {
    [Serializable]
    public class WorkerSpawner {
        [SerializeField] public List<Transform> SpawnLocations;


        public bool Spawn(WorkerType type, out Worker worker) {
            var spawnPoint = GameController.Instance.WorkerSpawnPoint;
            if (!spawnPoint) {
                Debug.LogError("Worker spawn point not set!");
                worker = null;
                return false;
            }

            return SpawnAtPosition(type, spawnPoint, out worker);
        }

        public bool SpawnFromPoint(WorkerType type, string name, out Worker worker) {
            var location = SpawnLocations.FirstOrDefault(l => l.name == name);
            if (location is null) {
                Debug.LogError("Location is not registered with the spawner!");
                worker = null;
                return false;
            }

            return SpawnAtPosition(type, location, out worker);
        }

        public bool SpawnAtPosition(WorkerType type, Transform location, out Worker worker) =>
            SpawnAtPosition(type, location.position, out worker);

        public bool SpawnAtPosition(WorkerType type, Vector3 position, out Worker worker) {
            if (!GameController.Instance.WorkerController.WorkerPrefabs.TryGetValue(type, out var prefab)) {
                Debug.LogError("Cannot find worker prefab " + nameof(WorkerSpawner));
                worker = null;
                return false;
            }

            var instance = UnityEngine.Object.Instantiate(prefab.gameObject, position, Quaternion.identity);
            worker = instance.GetComponent<Worker>();
            GameController.Instance.WorkerController.AddWorker(worker);
            return instance;
        }

        public void RemoveWorker(Worker worker) {
            GameController.Instance.WorkerController.RemoveWorker(worker);
            UnityEngine.Object.Destroy(worker.gameObject);
        }

        public IOrderedEnumerable<Worker> FindSpawnedWorkers(MachineBase machine) {
            var workerType = machine.SpawnWorkerType;
            var amount = machine.SpawnWorkers;
            var allWorkers = GameController.Instance.WorkerController.WorkerList.GetValueOrDefault(workerType);
            var machineController = GameController.Instance.MachineController;

            var list = new List<Worker>();

            //Find from machine
            list.AddRange(GetOrderedWorkers(machine.Workers));
            //Find idle workers
            var idleWorker = allWorkers.Where(w => w.Machine is null).ToList();
            list.AddRange(GetOrderedWorkers(idleWorker));
            //Find recovering workers
            var recoveringWorker = allWorkers.Where(w => w.Machine is MachineBase).Where(w =>
                GameController.Instance.MachineController.IsRecoveryMachine(w.Machine as MachineBase, out _)).ToList();
            list.AddRange(GetOrderedWorkers(recoveringWorker));
            //Find working workers
            list.AddRange(GetOrderedWorkers(allWorkers.Except(idleWorker).Except(recoveringWorker)));

            //Debug.LogWarning($"Found worker count: {list.Count}");
            return list.OrderBy(w => Vector3.Distance(machine.transform.position, w.transform.position));

            IEnumerable<Worker> GetOrderedWorkers(IEnumerable<Worker> workersList) {
                var tries = 0;
                var maxTries = GameController.Instance.WorkerController.WorkerList.Select(w => w.Value.Count).Sum();
                if (maxTries <= 100) maxTries = 100;
                if (list.Count >= amount) return Enumerable.Empty<Worker>();
                var ret = new List<Worker>();
                foreach (var worker in workersList.OrderBy(w =>
                             Vector3.Distance(machine.transform.position, w.transform.position))) {
                    if (ret.Count + list.Count >= amount) break;
                    if (worker.ToWorkerType() != workerType) continue;
                    if (ret.Contains(worker) || list.Contains(worker)) continue;

                    ret.Add(worker);
                }

                return ret;
            }
        }
    }
}