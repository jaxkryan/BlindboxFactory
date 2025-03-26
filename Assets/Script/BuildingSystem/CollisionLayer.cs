using System.Collections.Generic;
using BuildingSystem.Models;
using Script.Controller;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace BuildingSystem
{
    public class CollisionLayer : TilemapLayer
    {
        [SerializeField]
        private TileBase _collisionTileBase;

        public void SetCollisions(Buildable buildable, bool value) {
            var tile = value ? _collisionTileBase : null;
            var list = new List<Vector3Int>();
            buildable.IterateCollisionSpace(tileCoords => list.Add(tileCoords));
            foreach (var vector3Int in list) {
                var v3 = new Vector3Int(vector3Int.x, vector3Int.y);
                _tilemap.SetTile(v3, tile);
            }
        }

    }
}
