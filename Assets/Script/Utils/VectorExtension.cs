    using UnityEngine;

    public static class VectorExtension {
        public static Vector3Int ToVector3Int(this Vector2Int vector2Int, int z = 0) => new Vector3Int(vector2Int.x, vector2Int.y, z);
        public static Vector2Int ToVector2Int(this Vector3Int vector3Int) => new Vector2Int(vector3Int.x, vector3Int.y);
        public static Vector3 ToVector3(this Vector2 vector2, int z = 0) => new Vector3(vector2.x, vector2.y, z);
        public static Vector2 ToVector2(this Vector3 vector3) => new Vector2(vector3.x, vector3.y);
    }