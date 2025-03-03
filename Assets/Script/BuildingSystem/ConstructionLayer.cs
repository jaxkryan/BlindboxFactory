using BuildingSystem.Models;
using Exception;
using BuildingSystem.Models;
using Exception;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.Tilemaps;
using Script.Machine; // For MachineBase

namespace BuildingSystem
{
    public class ConstructionLayer : TilemapLayer
    {
        private Dictionary<BuildableItem, int> _storedBuildables = new();
        private Dictionary<Vector3Int, Buildable> _buildables = new();
        [SerializeField] private CollisionLayer _collisionLayer;

        public void Build(Vector3 worldCoords, BuildableItem item)
        {
            worldCoords.z = 0;
            var coords = _tilemap.WorldToCell(worldCoords);

            // Check if the buildable item has a MachineBase component.
            MachineBase machinePrefab = null;
            if (item.gameObject != null)
            {
                machinePrefab = item.gameObject.GetComponent<MachineBase>();
                if (machinePrefab != null)
                {
                    float requiredEnergy = machinePrefab.PowerUse;
                    if (!ElectricGenerator.HasEnoughEnergy(requiredEnergy))
                    {
                        Debug.LogWarning($"Not enough energy to build machine {item.gameObject.name}.");
                        return;
                    }
                }
            }

            // Instantiate the buildable object.
            GameObject itemObject = null;
            if (item.gameObject != null)
            {
                itemObject = Instantiate(
                    item.gameObject,
                    _tilemap.GetCellCenterLocal(coords),
                    Quaternion.identity
                );
                int sortingOrder = Mathf.FloorToInt(-worldCoords.y * 100) + Mathf.FloorToInt(worldCoords.x * 10);
                itemObject.GetComponent<SpriteRenderer>().sortingOrder = sortingOrder;
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

            var buildingPlacer = FindObjectOfType<BuildingPlacer>();
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
                FindObjectOfType<StoredBuildablesUI>()?.UpdateStoredBuildablesUI();
            }
        }

        private void RegisterBuildableCollisionSpace(Buildable buildable)
        {
            buildable.IterateCollisionSpace(tileCoord => _buildables.Add(tileCoord, buildable));
        }

        private void UnRegisterBuildableCollisionSpace(Buildable buildable)
        {
            buildable.IterateCollisionSpace(tileCoord => _buildables.Remove(tileCoord));
        }

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
            FindObjectOfType<StoredBuildablesUI>()?.UpdateStoredBuildablesUI();
        }

        public Dictionary<Vector3Int, Buildable> GetBuildables() => _buildables;
        public Dictionary<BuildableItem, int> GetStoredBuildables() => _storedBuildables;

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

        private bool IsRectOccupied(Vector3Int coords, RectInt rect)
        {
            return rect.Iterate(coords, tileCoord => _buildables.ContainsKey(tileCoord));
        }
    }
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


