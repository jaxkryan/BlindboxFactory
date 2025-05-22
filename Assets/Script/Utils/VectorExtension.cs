using UnityEngine;

public static class VectorExtension {
    public static Vector3Int ToVector3Int(this Vector2Int vector2Int, int z = 0) => new Vector3Int(vector2Int.x, vector2Int.y, z);
    public static Vector2Int ToVector2Int(this Vector3Int vector3Int) => new Vector2Int(vector3Int.x, vector3Int.y);
    public static Vector3 ToVector3(this Vector2 vector2, float z = 0) => new Vector3(vector2.x, vector2.y, z);
    public static Vector2 ToVector2(this Vector3 vector3) => new Vector2(vector3.x, vector3.y);
}


public class V2Int {
    public int x;
    public int y;

    public V2Int(Vector2Int vector2Int) {
        x = vector2Int.x;
        y = vector2Int.y;
    }
    public static implicit operator Vector2Int(V2Int v) => new Vector2Int(v.x, v.y);
}

public class V2 {
    public float x;
    public float y;
    

    public V2(Vector2 vector2) {
        x = vector2.x;
        y = vector2.y;
    }
    public static implicit operator Vector2(V2 v) => new Vector2(v.x, v.y);
}

public class V3Int {
    public int x;
    public int y;
    public int z;

    public V3Int(Vector3Int vector3Int) {
        x = vector3Int.x;
        y = vector3Int.y;
        z = vector3Int.z;
    }
    public static implicit operator Vector3Int(V3Int v) => new Vector3Int(v.x, v.y, v.z);
}

public class V3 {
    public float x;
    public float y;
    public float z;
    
    public V3(Vector3 vector3) {
        x = vector3.x;
        y = vector3.y;
        z = vector3.z;
    }
    public static implicit operator Vector3(V3 v) => new Vector3(v.x, v.y, v.z);
}