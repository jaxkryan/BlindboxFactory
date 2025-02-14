using BuildingSystem.Models;
using Exception;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.Tilemaps;

namespace BuildingSystem
{
    public class ConstructionLayer : TilemapLayer
    {
        private Dictionary<BuildableItem, int> _storedBuildables = new();

        private Dictionary<Vector3Int, Buildable> _buildables = new();
        [SerializeField]
        private CollisionLayer _collisionLayer;
        public void Build(Vector3 worldCoords, BuildableItem item)
        {
            GameObject itemObject = null;
            var coords = _tilemap.WorldToCell(worldCoords);
            if(item.Tile != null)
            {
                var tileChangeData = new TileChangeData(
                    coords,
                    item.Tile,
                    Color.white,
                    Matrix4x4.Translate(Vector3.zero)
                    //Matrix4x4.Translate(item.TileOffSet)
                    );
                _tilemap.SetTile(tileChangeData, false);
            }
            if (item.gameObject != null)
            {
                itemObject = Instantiate(
                    item.gameObject,
                    _tilemap.GetCellCenterLocal(coords),
                    //_tilemap.CellToWorld(coords) + _tilemap.cellSize / 2 + item.TileOffSet,
                    Quaternion.identity
                    );
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

        //public void Destroy(Vector3 worldCoords)
        //{
        //    var coords = _tilemap.WorldToCell(worldCoords);

        //    var buildable = _buildables[coords];
        //    if (buildable.BuildableType.UseCustomCollisionSpace)
        //    {
        //        _collisionLayer.SetCollisions(buildable, false);
        //        UnRegisterBuildableCollisionSpace(buildable);
        //    }
        //    _buildables.Remove(coords);
        //    buildable.Destroy();
        //}

        public void Destroy(Vector3 worldCoords)
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
    }
}