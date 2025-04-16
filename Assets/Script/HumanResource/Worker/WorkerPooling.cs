using System;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using JetBrains.Annotations;
using MyBox;
using Script.Controller;
using Script.Utils;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

namespace Script.HumanResource.Worker {
    public class WorkerPooling : PersistentSingleton<WorkerPooling> {
        [SerializeField] [Min(0)] private int _minimumReservedSize = 10;
        [SerializeField] [Min(0)] private int _maximumReservedSize = 40;
        [SerializeField] private SerializedDictionary<WorkerType, Worker> _workerPrefabs = new();
        private List<Worker> _workerPool = new();

        protected override void Awake() {
            base.Awake();
            if (_minimumReservedSize > _maximumReservedSize) {
                (_minimumReservedSize, _maximumReservedSize) = (_maximumReservedSize, _minimumReservedSize);
            }

            SetReservedWorkers();
        }

        private void CheckMissingPrefab() {
            foreach (var key in _workerPrefabs.Keys) {
                if (IsPrefabMissing(key)) {
                    Debug.LogError($"Prefab is missing for worker {key}");
                    GetAllWorkers(key).ForEach(DespawnWorker);
                    _workerPrefabs.Remove(key);
                }
            }
        }

        private bool IsPrefabMissing(WorkerType workerType)
            => _workerPrefabs.GetValueOrDefault(workerType) is null;

        public List<Worker> GetAllWorkers(WorkerType workerType, bool refill = true) {
            if (IsPrefabMissing(workerType)) return new List<Worker>();
            var children = _workerPool;
            var list = children.Where(w => w.ToWorkerType() == workerType).ToList();

            for (int i = 0; i < list.Count; i++) {
                list[i].Director.AgentUpdateOrder = i;
            }

            return list;
        }

        public List<Worker> GetReservedWorkers(WorkerType workerType)
            => GetAllWorkers(workerType).Where(w => w.ToWorkerType() == workerType).ToList();

        [CanBeNull]
        public Worker GetReservedWorker(WorkerType workerType) {
            SetReservedWorkers();
            var list = GetAllWorkers(workerType);

            return list.Count == 0 ? null : list.FirstOrDefault(w => !w.gameObject.activeInHierarchy);
        }

        private void SetReservedWorkers() {
            CheckMissingPrefab();
            foreach (var key in _workerPrefabs.Keys) {
                if (IsPrefabMissing(key)) {
                    Debug.LogError($"Prefab is missing for worker {key}");
                    GetAllWorkers(key, false).ForEach(DespawnWorker);
                    _workerPrefabs.Remove(key);
                    continue;
                }

                var list = GetAllWorkers(key, false).Where(w => !w.gameObject.activeInHierarchy).ToList();
                //If reserved workers exceed maximum
                if (list.Count > _maximumReservedSize) {
                    var worker = GetReservedWorker(key);
                    if (worker is not null) DespawnWorker(worker);
                }

                //If reserved workers less than minimum
                if (list.Count < _minimumReservedSize) {
                    var prefab = _workerPrefabs[key];
                    for (int i = 0; i < _minimumReservedSize - list.Count; i++) {
                        var worker = SpawnWorker(key);
                        if (worker is null) {
                            Debug.LogWarning("Cannot instantiate worker: " + key);
                            break;
                        }
                    }
                }
            }
        }

        public void ActivateWorker(Worker worker, Vector3 position) {
            DeactivateWorker(worker);

            worker.transform.position = position.ToVector2().ToVector3(GameController.Instance.NavMeshSurface.transform.position.z);
            
            if (NavMesh.SamplePosition(position, out var hit, float.MaxValue, NavMesh.AllAreas)) {
                worker.transform.position = hit.position;
                worker.Agent.enabled = true;
            }
            else {
                Debug.LogWarning("Worker placed off NavMesh — cannot enable NavMeshAgent!");
            }

            worker.Animator.enabled = true;
            worker.Director.enabled = true;
            if (worker.TryGetComponent<Rigidbody2D>(out var rb)) rb.simulated = false;
            worker.gameObject.SetActive(true);
        }

        public void DeactivateWorker(Worker worker) {
            if (worker.Agent.hasPath) worker.Agent.ResetPath();
            worker.Agent.enabled = false;
            worker.Animator.Rebind();
            worker.Animator.enabled = false;
            worker.Director.ActionPlan = null;
            worker.Director.CurrentAction = null;
            worker.Director.CurrentGoal = null;
            worker.Director.TargetSlot = null;
            worker.Director.enabled = false;
            if (worker.TryGetComponent<Rigidbody2D>(out var rb)) rb.simulated = true;
            worker.gameObject.SetActive(false);
        }


        [CanBeNull]
        private Worker SpawnWorker(WorkerType workerType) {
            CheckMissingPrefab();
            if (IsPrefabMissing(workerType)) return null;
            var prefab = _workerPrefabs[workerType];
            if (NavMesh.SamplePosition(this.transform.position, out NavMeshHit hit, float.MaxValue, NavMesh.AllAreas)) {
                var go = Instantiate(prefab.gameObject, hit.position, Quaternion.identity);
                if (go.TryGetComponent(out Worker worker)) {
                    go.transform.SetParent(transform);
                    _workerPool.Add(worker);

                    DeactivateWorker(worker);
                    return worker;
                }
                Destroy(go);
            }

            return null;
        }

        private void DespawnWorker(Worker worker) {
            if (_workerPool.Contains(worker)) _workerPool.Remove(worker);
            Destroy(worker.gameObject);
        }
    }
}