using Script.Controller;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DefaultMapGenerator : MonoBehaviour
{
    public Tilemap tilemap;
    public Tile groundTile;

    public int size = 32;

    void Start()
    {
        GenerateDefaultMap();
    }

    void GenerateDefaultMap()
    {
        tilemap.ClearAllTiles();

        Vector2Int center = new Vector2Int(0, 0);
        int halfSize = size / 2;

        for (int x = -halfSize; x < halfSize; x++)
        {
            for (int y = -halfSize; y < halfSize; y++)
            {
                tilemap.SetTile(new Vector3Int(x, y, 0), groundTile);
            }
        }

        tilemap.CompressBounds();
        Debug.Log("Default 32x32 map generated with 4-way crossroad.");

        GameController.Instance.BuildNavMesh();
    }
}
