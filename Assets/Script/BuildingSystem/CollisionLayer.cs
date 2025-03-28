using System;
using System.Collections.Generic;
using System.Linq;
using BuildingSystem.Models;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace BuildingSystem
{
    public class CollisionLayer : TilemapLayer
    {
        [SerializeField] private TileBase _collisionTileBase;

        [SerializeField] private TileBase _foundationTileBase;

        public void SetCollisions(Buildable buildable, bool value) {
            if (_collisionTileBase == null || _foundationTileBase == null) {
                Debug.LogError("Collision layer Tile missing!");
                return;
            }
            var list = new List<Vector3Int>();
            buildable.IterateCollisionSpace(tileCoords => list.Add(tileCoords));
            list = list.Select(v => v.ToVector2Int().ToVector3Int()).ToList();
            foreach (var v3 in list) {
                if (!value) _tilemap.SetTile(v3, null);
                else {
                    _tilemap.SetTile(v3, IsInnerTile(v3) ? _foundationTileBase : _collisionTileBase);
                }
            }

            bool IsInnerTile(Vector3Int pos) {
                //Get neighboring pos
                var neighbors = list.Where(v =>
                    Math.Abs(v.x - pos.x) <= 1 && Math.Abs(v.y - pos.y) <= 1
                    && v != pos).ToList();
                //If all neighbors are in the list then true
                return neighbors.Count == 8 && neighbors.All(v => list.Contains(v));
            }
        }

    }
}
