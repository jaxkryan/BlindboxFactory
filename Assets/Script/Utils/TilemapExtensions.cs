using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Script.Utils {
    public static class TilemapExtensions {
        public static List<Vector2Int> GetCellPositions(this Tilemap tilemap) {
            var list = new List<Vector2Int>();
            foreach (var pos in tilemap.cellBounds.allPositionsWithin) {
                list.Add(pos.ToVector2Int());
            }
            return list;
        }
    }
}