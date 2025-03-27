using System;
using System.Collections.Generic;
using Script.Controller;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Script.Quest {
    [Serializable]
    public class NewLandQuestReward : QuestReward {
        [SerializeField] public List<NewLandData> Data;

        public override void Grant() {
            var tilemap = GameController.Instance.Ground;
            foreach (var data in Data) {
                //With direction and offset, get the most outer tile
                var direction = data.ExtendDirection switch {
                    NewLandData.Direction.up => Vector2Int.up,
                    NewLandData.Direction.down => Vector2Int.down,
                    NewLandData.Direction.left => Vector2Int.left,
                    NewLandData.Direction.right => Vector2Int.right,
                    _ => throw new ArgumentOutOfRangeException()
                };
                var anchor = GetOuterMostPosition(Vector2Int.zero, direction, tilemap) + direction;
                
                //With the outer tile as anchor, calculate the span of the new land
                HashSet<Vector2Int> span  = new();
                bool isVertical = data.ExtendDirection == NewLandData.Direction.up || data.ExtendDirection == NewLandData.Direction.down;
                int width = isVertical ? tilemap.size.x : tilemap.size.y;
                for (int x = 0; x < data.Extend; x++) {
                    for (int y = 0; y < width; y++) {
                        var tile = isVertical ? new Vector2Int(y,x) : new Vector2Int(x,y);
                        span.Add(tile + anchor);
                    }
                }
                //Add the new tiles to the tilemap
                foreach (var i in span) {
                    tilemap.SetTile(i.ToVector3Int(), data.Tile);
                }
                GameController.Instance.BuildNavMesh();
            }

            Vector2Int GetOuterMostPosition(Vector2Int initialPosition, Vector2Int direction, Tilemap map) {
                int maxTries = 100;
                var current = initialPosition.ToVector3Int();
                var directionV3 = direction.ToVector3Int();
                if (map.HasTile(current)) {
                    while (map.HasTile(current)) { current += directionV3; }

                    return current.ToVector2Int();
                }

                while (maxTries-- > 0) {
                    current += directionV3;
                    if (map.HasTile(current)) return current.ToVector2Int();
                }

                return new Vector2Int(Int32.MinValue, Int32.MinValue);
            }
        }


        [Serializable]
        public struct NewLandData {
            [Tooltip("The tile use for the new land area")]
            public Tile Tile;

            [Tooltip("How far away from the grid is the new plot.")]
            public int Extend;
            [Tooltip("Which direction to extend the new land")]
            public Direction ExtendDirection;
            public enum Direction {
                up,down,left,right
            }
        }
    }
}