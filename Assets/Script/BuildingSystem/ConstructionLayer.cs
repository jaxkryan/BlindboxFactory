using System;
using BuildingSystem.Models;
using Exception;
using System.Collections.Generic;
using Script.Machine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.Tilemaps;
using Script.Controller;

namespace BuildingSystem
{
    public class ConstructionLayer : TilemapLayer
    {
        private Dictionary<BuildableItem, int> _storedBuildables = new();

        private Dictionary<Vector3Int, Buildable> _buildables = new();
        [SerializeField]
        private CollisionLayer _collisionLayer;
        public event Action<GameObject> onItemBuilt = delegate { };

        private BuildableItem _movedBuildable = null;
        public GameObject Build(Vector3 worldCoords, BuildableItem item)
        {
            if (!BuildingPlacer.Instance.IsBuildingFromInventory())
            {

                var itemCost = 0;
                if (item.Cost == null)
                {
                    itemCost = 0;
                }
                else
                {
                    itemCost = item.Cost;
                }
                GameController.Instance.ResourceController.TryGetAmount(Script.Resources.Resource.Gold, out long currentMoney);
                // Ensure we have enough currency before proceeding
                if (currentMoney < itemCost)
                {
                    Debug.Log("Not enough money to build this item!");
                    return null;
                }
                GameController.Instance.ResourceController.TrySetAmount(Script.Resources.Resource.Gold, (long)(currentMoney - itemCost));
            }

            worldCoords.z = 0;
            GameObject itemObject = null;
            var coords = _tilemap.WorldToCell(worldCoords);

            Vector3 tilemapPosition = _tilemap.GetCellCenterLocal(coords);
            Debug.Log($"Tilemap Center Position: {tilemapPosition}");

            if (item.gameObject != null)
            {
                itemObject = Instantiate(
                    item.gameObject,
                    _tilemap.GetCellCenterLocal(coords),
                    Quaternion.identity
                );
                int sortingOrder = Mathf.FloorToInt(-worldCoords.y * 100) + Mathf.FloorToInt(worldCoords.x * 10);
                itemObject.GetComponent<SpriteRenderer>().sortingOrder = sortingOrder;

                // Add data to building
                var machine = itemObject.GetComponent<MachineBase>();
                if (machine is not null)
                {
                    machine.Position = coords.ToVector2Int();
                    machine.PrefabName = item.Name;
                    GameController.Instance.MachineController.AddMachine(machine);
                }
            }

            var buildable = new Buildable(item, coords, _tilemap, itemObject);
            if (item.UseCustomCollisionSpace)
            {
                _collisionLayer.SetCollisions(buildable, true);
                RegisterBuildableCollisionSpace(buildable);
            }
            else
            {
                _buildables.Add(coords, buildable);
            }

            var buildingPlacer = BuildingPlacer.instance;
            if (buildingPlacer != null && buildingPlacer.IsBuildingFromInventory())
            {
                if (_storedBuildables.ContainsKey(item) && _storedBuildables[item] > 1)
                {
                    _storedBuildables[item]--;
                }
                else
                {
                    _storedBuildables.Remove(item);
                    buildingPlacer.SetBuildableFromInventory(null);
                    buildingPlacer.ClearPreview();
                }

                FindFirstObjectByType<StoredBuildablesUI>()?.UpdateStoredBuildablesUI();
            }
            // Invoke built event
            if (itemObject)
            {
                onItemBuilt?.Invoke(itemObject);
            }

            return itemObject;
        }


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

        public void Stored(Vector3 worldCoords)
        {
            var coords = _tilemap.WorldToCell(worldCoords);

            if (!_buildables.ContainsKey(coords)) return;

            var buildable = _buildables[coords];

            if (_storedBuildables.ContainsKey(buildable.BuildableType))
            {
                _storedBuildables[buildable.BuildableType]++;
            }
            else
            {
                _storedBuildables[buildable.BuildableType] = 1;
            }

            if (buildable.BuildableType.UseCustomCollisionSpace)
            {
                _collisionLayer.SetCollisions(buildable, false);
                UnRegisterBuildableCollisionSpace(buildable);
            }

            _buildables.Remove(coords);
            buildable.Destroy();
            FindFirstObjectByType<StoredBuildablesUI>()?.UpdateStoredBuildablesUI();
        }



        public Dictionary<Vector3Int, Buildable> GetBuildables()
        {
            return _buildables;
        }

        public Dictionary<BuildableItem, int> GetStoredBuildables()
        {
            return _storedBuildables;
        }

        public void RemoveStoredBuildable(BuildableItem item)
        {
            if (_storedBuildables.ContainsKey(item))
            {
                _storedBuildables[item]--;
                if (_storedBuildables[item] <= 0)
                {
                    _storedBuildables.Remove(item);
                }
            }
        }



        public bool IsEmpty(Vector3 worldCoords, RectInt collisionSpace = default)
        {
            var coords = _tilemap.WorldToCell(worldCoords);
            if (!collisionSpace.Equals(default))
            {
                return !IsRectOccupied(coords, collisionSpace);
            }
            return !_buildables.ContainsKey(coords) && _tilemap.GetTile(coords) == null;
        }

        private void RegisterBuildableCollisionSpace(Buildable buildable)
        {
            buildable.IterateCollisionSpace(tileCoord => _buildables.Add(tileCoord, buildable));
        }

        private void UnRegisterBuildableCollisionSpace(Buildable buildable)
        {
            buildable.IterateCollisionSpace(tileCoord =>
            {
                _buildables.Remove(tileCoord);
            });
        }

        private bool IsRectOccupied(Vector3Int coords, RectInt rect)
        {
            return rect.Iterate(coords, tileCoord => _buildables.ContainsKey(tileCoord));
        }

        public long RemoveBuildable(GameObject targetObject)
        {
            foreach (var kvp in _buildables)
            {
                var coords = kvp.Key;
                var buildable = kvp.Value;

                if (buildable.GameObject == targetObject)
                {
                    int cost = buildable.BuildableType.Cost;
                    long refundAmount = cost / 2;
                    if (buildable.BuildableType.UseCustomCollisionSpace)
                    {
                        _collisionLayer.SetCollisions(buildable, false);
                        UnRegisterBuildableCollisionSpace(buildable);
                    }

                    _buildables.Remove(coords);
                    buildable.Destroy();

                    Debug.Log($"Removed buildable at {coords}, refunded: {refundAmount}");
                    return refundAmount;
                }
            }

            Debug.Log("Buildable not found!");
            return 0;
        }

        public void MoveBuildable(GameObject targetObject)
        {
            if (targetObject == null) return;

            BuildableItem buildable = FindBuildableByGameObject(targetObject);

            if (buildable == null) return;

            RemoveBuildable(targetObject);

            _movedBuildable = buildable;

            Debug.Log($"{buildable.name} moved.");
        }

        public bool TryPlaceMovedBuildable(Vector3 position)
        {
            if (_movedBuildable == null) return false;

            bool isSpaceEmpty = IsEmpty(position,
                _movedBuildable.UseCustomCollisionSpace ? _movedBuildable.CollisionSpace : default);

            if (isSpaceEmpty)
            {
                Build(position, _movedBuildable);
                Debug.Log($"{_movedBuildable.name} moved to new position.");
                _movedBuildable = null;
                return true;
            }

            return false;   
        }

        public BuildableItem FindBuildableByGameObject(GameObject targetObject)
        {
            foreach (var kvp in _buildables)
            {
                if (kvp.Value.BuildableType.gameObject == targetObject)
                {
                    return kvp.Value.BuildableType;
                }
            }

            return null;
        }

        public bool HasMovedBuildable()
        {
            return _movedBuildable != null;
        }

    }
}