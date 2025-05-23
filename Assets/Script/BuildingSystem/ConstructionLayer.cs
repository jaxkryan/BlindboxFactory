using System;
using BuildingSystem.Models;
using Exception;
using System.Collections.Generic;
using System.Linq;
using Script.Machine;
using UnityEngine;
using Script.Controller;
using Script.HumanResource.Worker;
using ZLinq;
using Script.Machine.Machines.Canteen;
using Script.Utils;
using UnityEngine.UI;
using UnityEditor.Build.Reporting;

namespace BuildingSystem {
    public class ConstructionLayer : TilemapLayer {
        private Dictionary<BuildableItem, int> _storedBuildables = new();

        private Dictionary<Vector3Int, Buildable> _buildables = new();
        [SerializeField] private CollisionLayer _collisionLayer;
        public event Action<GameObject> onItemBuilt = delegate { };

        public GameObject Build(Vector3 worldCoords, BuildableItem item) {
            if (!BuildingPlacer.Instance.IsBuildingFromInventory()) {
                var itemCost = 0;
                if (item.Cost == null) { itemCost = 0; }
                else { itemCost = item.Cost; }

                GameController.Instance.ResourceController.TryGetAmount(Script.Resources.Resource.Gold,
                    out long currentMoney);
                // Ensure we have enough currency before proceeding
                if (currentMoney < itemCost) {
                    //Debug.Log("Not enough money to build this item!" + currentMoney + "   /   " + itemCost);
                    return null;
                }

                GameController.Instance.ResourceController.TrySetAmount(Script.Resources.Resource.Gold,
                    (long)(currentMoney - itemCost));
            }

            worldCoords.z = 0;
            GameObject itemObject = null;
            var coords = _tilemap.WorldToCell(worldCoords);

            Vector3 tilemapPosition = _tilemap.GetCellCenterLocal(coords);

            if (item.gameObject != null) {
                itemObject = Instantiate(
                    item.gameObject,
                    _tilemap.GetCellCenterLocal(coords),
                    Quaternion.identity
                );
                int sortingOrder = SortingOrder.Invoke(itemObject.transform.position);
                itemObject.GetComponent<SpriteRenderer>().sortingOrder = sortingOrder;

                // Add data to building
                var machine = itemObject.GetComponent<MachineBase>();
                if (machine is not null) {
                    machine.Position = worldCoords;
                    machine.PrefabName = item.Name;
                    GameController.Instance.MachineController.AddMachine(machine);
                }
            }

            var buildable = new Buildable(item, coords, _tilemap, itemObject);
            if (item.UseCustomCollisionSpace) {
                _collisionLayer.SetCollisions(buildable, true);
                RegisterBuildableCollisionSpace(buildable);
            }
            else { _buildables.Add(coords, buildable); }

            var buildingPlacer = BuildingPlacer.instance;
            if (buildingPlacer != null && buildingPlacer.IsBuildingFromInventory()) {
                if (_storedBuildables.ContainsKey(item) && _storedBuildables[item] > 1) { _storedBuildables[item]--; }
                else {
                    _storedBuildables.Remove(item);
                    buildingPlacer.SetBuildableFromInventory(null);
                    buildingPlacer.ClearPreview();
                }

                FindFirstObjectByType<StoredBuildablesUI>()?.UpdateStoredBuildablesUI();
            }

            // Invoke built event
            if (itemObject) { onItemBuilt?.Invoke(itemObject); }
            AudioManager.Instance.PlaySfx("place");
            return itemObject;
        }
        
        public static Func<Vector3, int> SortingOrder = (worldCoords) 
            => Mathf.FloorToInt(-worldCoords.y * 100) + Mathf.FloorToInt(worldCoords.x * 10); 


        //public void Stored(Vector3 worldCoords)
        //{
        //    var coords = _tilemap.WorldToCell(worldCoords);

        //    var buildable = _buildables[coords];
        //    if (buildable.BuildableType.UseCustomCollisionSpace)
        //    {
        //        _collisionLayer.SetCollisions(buildable, false);
        //        UnRegisterBuildableCollisionSpace(buildable);
        //    }
        //    _buildables.Remove(coords);
        //    buildable.Stored();
        //}

        public void Stored(Vector3 worldCoords) {
            if (!TryGetBuildable(worldCoords, out var buildable)) return;

            if (IsStorageExceed(buildable)) return;
            if (_storedBuildables.ContainsKey(buildable.BuildableType)) {
                _storedBuildables[buildable.BuildableType]++;
            }
            else { _storedBuildables[buildable.BuildableType] = 1; }
            
            Remove(worldCoords);
            onStoredBuilding?.Invoke();
        }

        public static event Action onStoredBuilding = delegate { };

        public void Sell(Vector3 worldCoords) {
            if (!TryGetBuildable(worldCoords, out var buildable)) return;

            if (IsStorageExceed(buildable)) return;

            GameController.Instance.ResourceController.TryGetAmount(Script.Resources.Resource.Gold,
                out long amount);
            GameController.Instance.ResourceController.TrySetAmount(Script.Resources.Resource.Gold,
                amount + buildable.BuildableType.Cost / 2);
            
            Remove(worldCoords);
        }

        public bool IsStorageExceed(Buildable buildable)
        {
            if (buildable.GameObject.CompareTag("StoreHouse"))
            {
                var storeHouse = buildable.GameObject.GetComponent<StoreHouse>();
                var removedBoxAmount = storeHouse.boxamount;
                var removedResourceAmount = storeHouse.resorceamount;

                GameController.Instance.ResourceController.TryGetData(Script.Resources.Resource.Gummy, out var resourceData, out var currentAmountR);
                GameController.Instance.BoxController.TryGetWarehouseMaxAmount(out var currentBoxMax);
                long boxCurrent = GameController.Instance.BoxController.GetTotalBlindBoxAmount();

                long newBoxMax = currentBoxMax - removedBoxAmount;
                long newResourceMax = resourceData.MaxAmount - removedResourceAmount;

                Debug.Log("Current Resource Amount: " + currentAmountR);
                Debug.Log("New Resource Max After Removal: " + newResourceMax);
                Debug.Log("Current Box Amount: " + boxCurrent);
                Debug.Log("New Box Max After Removal: " + newBoxMax);

                if (currentAmountR > newResourceMax || boxCurrent > newBoxMax)
                {
                    ShortNotification.Instance?.ShowNotification("Cannot remove: StoreHouse contains stored items exceeding future capacity!");
                    return true;
                }

                return false;
            }

            return false;
        }


        private bool TryGetBuildable(Vector3 worldCoords, out Buildable buildable) {
            var coords = _tilemap.WorldToCell(worldCoords);
            foreach (var building in _buildables)
            {
                var origin = building.Key;
                var b = building.Value;
                if (b.BuildableType.CollisionSpace.Contains((Vector2Int)(coords - origin)))
                {
                    buildable = b;
                    return true;
                }
            }

            buildable = null;
            return false;
        }
        
        private void Remove(Vector3 worldCoords) {
            if (!TryGetBuildable(worldCoords, out var buildable)) return;
            
            if (buildable.BuildableType.UseCustomCollisionSpace) {
                _collisionLayer.SetCollisions(buildable, false);
                UnRegisterBuildableCollisionSpace(buildable);
            }

            if (buildable.GameObject.TryGetComponent<MachineBase>(out var machine))
                GameController.Instance.MachineController.RemoveMachine(machine);

            var coords = _tilemap.WorldToCell(worldCoords);
            RemoveBuildingWorker(machine);
            _buildables.Remove(coords);
            buildable.Destroy();

            FindFirstObjectByType<StoredBuildablesUI>()?.UpdateStoredBuildablesUI();
        }

        private void RemoveBuildingWorker(MachineBase machine)
        {
            var worker = GameController.Instance.WorkerSpawner.FindSpawnedWorkers(machine);
            if (worker != null) {
                var workers = worker.AsValueEnumerable();
                if(workers.Count() > 0)
                {
                    foreach(Worker w in workers)
                    {
                        GameController.Instance.WorkerSpawner.RemoveWorker(w);
                    }
                }
            }
        }

        public Dictionary<Vector3Int, Buildable> GetBuildables() { return _buildables; }

        public Dictionary<BuildableItem, int> GetStoredBuildables() { return _storedBuildables; }

        public void RemoveStoredBuildable(BuildableItem item) {
            if (_storedBuildables.ContainsKey(item)) {
                _storedBuildables[item]--;
                if (_storedBuildables[item] <= 0) { _storedBuildables.Remove(item); }
            }
        }


        public bool IsEmpty(Vector3 worldCoords, RectInt collisionSpace = default) {
            var coords = _tilemap.WorldToCell(worldCoords);
            if (!collisionSpace.Equals(default)) { return !IsRectOccupied(coords, collisionSpace); }

            return !_buildables.ContainsKey(coords) && _tilemap.GetTile(coords) == null;
        }

        private void RegisterBuildableCollisionSpace(Buildable buildable) {
            buildable.IterateCollisionSpace(tileCoord => _buildables.Add(tileCoord, buildable));
        }

        private void UnRegisterBuildableCollisionSpace(Buildable buildable) {
            buildable.IterateCollisionSpace(tileCoord => { _buildables.Remove(tileCoord); });
        }

        private bool IsRectOccupied(Vector3Int coords, RectInt rect) {
            return rect.Iterate(coords, tileCoord => _buildables.ContainsKey(tileCoord));
        }

        public List<Worker> FindWorkersByMachine(MachineBase machine)
        {
            int number = machine.SpawnWorkers;

            // Find Wokers in the machine
            var workers = machine.Workers.ToList();

            if (workers.Count < number)
            {
                number -= workers.Count;

                // Find Resting Workers
                var restingWorkers = GameController.Instance.WorkerController.WorkerList
                    .AsValueEnumerable()
                    .SelectMany(k => k.Value)
                    .Where(w => w.Machine == null)
                    .Take(number)
                    .ToList();

                workers.AddRange(restingWorkers);
                number -= restingWorkers.Count;

                // Find Workers in Canteen or Restroom
                if (number > 0)
                {
                    List<Worker> coreUpdateWorkers = GameController.Instance.WorkerController.WorkerList
                        .AsValueEnumerable()
                        .SelectMany(k => k.Value)
                        .Where(w => w.Machine is Canteen || w.Machine is RestRoom)
                        .Take(number)
                        .ToList();

                    workers.AddRange(coreUpdateWorkers);
                    number -= coreUpdateWorkers.Count;
                }

                // Find Workers Assigned to Other Machines
                if (number > 0)
                {
                    List<Worker> otherWorkers = GameController.Instance.WorkerController.WorkerList
                        .SelectMany(k => k.Value)
                        .Where(w => w.Machine is not Canteen
                            && w.Machine is not RestRoom
                            && w.Machine != null
                            && (MachineBase)w.Machine != machine)
                        .Take(number)
                        .ToList();

                    workers.AddRange(otherWorkers);
                }
            }

            //Debug.LogWarning($"{workers.Count} workers found for machine {machine.name}");
            return workers;
        }

        public SaveData Save()
            => new SaveData() {
                StoredBuilding = _storedBuildables.ToDictionary(k => k.Key.Name, v => v.Value),
            };

        public void Load(SaveData saveData) {
            var controller = GameController.Instance.MachineController;
            foreach (var pair in saveData.StoredBuilding) {
                var prefab = controller.Buildables.Find(b => b.Name == pair.Key);

                if (_storedBuildables.TryGetValue(prefab, out var amount)) {
                    _storedBuildables.AddOrUpdate(prefab, amount + pair.Value);
                }
                _storedBuildables.TryAdd(prefab, pair.Value);
            }
        }

        public class SaveData {
            public Dictionary<string, int> StoredBuilding;
        }
    }
}