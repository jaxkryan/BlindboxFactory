using System;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using Script.Controller;
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

        public bool SpawnAtPosition(WorkerType type, Transform location, out Worker worker) => SpawnAtPosition(type, location.position, out worker);
        public bool SpawnAtPosition(WorkerType type, Vector3 position, out Worker worker) {
            if (!GameController.Instance.WorkerController.WorkerPrefabs.TryGetValue(type, out var prefab)) {
                Debug.LogError("Cannot find worker prefab " + nameof(WorkerSpawner));
                worker = null;
                return false;
            }
            
            var instance = UnityEngine.Object.Instantiate(prefab.gameObject, position, Quaternion.identity);
            worker = instance.GetComponent<Worker>();
            return instance;
        }
        
        public void RemoveWorker(Worker worker) => UnityEngine.Object.Destroy(worker.gameObject);
    }
}