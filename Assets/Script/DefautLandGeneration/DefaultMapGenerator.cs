using Script.Controller;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DefaultMapGenerator : MonoBehaviour
{
    public Tilemap tilemap;
    public Tile groundTile;
    public Tile portalTile;
    public GameObject leftGroundPrefab;
    public GameObject rightGroundPrefab;

    public int size = 32;
    public int portalsize = 4;

    void Start()
    {
        GenerateDefaultMap();
        var tilePos = tilemap.gameObject.transform.position;
        //SpawnGroundSides(Vector3Int.zero);
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

        for (int x = -portalsize/2; x < portalsize/2; x++)
        {
            for (int y = -portalsize/2; y < portalsize/2; y++)
            {
                tilemap.SetTile(new Vector3Int(x, y, 0), portalTile);
            }
        }

        tilemap.CompressBounds();
        GameController.Instance.BuildNavMesh();
    }

    void SpawnGroundSides(Vector3Int tilePos)
    {
        Vector3 center = tilemap.CellToWorld(tilePos) + tilemap.tileAnchor;
        float width = tilemap.layoutGrid.cellSize.x;
        float height = tilemap.layoutGrid.cellSize.y;

        // Key points
        Vector3 left = center + new Vector3(-width / 2f, 0, 0);
        Vector3 right = center + new Vector3(width / 2f, 0, 0);
        Vector3 bottom = center + new Vector3(0, -height / 2f, 0);

        // Intersection point
        Vector3 intersection = new Vector3(center.x, left.y, 0);

        // Top width for each ground piece
        float halfWidth = Vector3.Distance(left, intersection);

        // Midpoints
        Vector3 leftMid = (left + intersection) / 2f;
        Vector3 rightMid = (right + intersection) / 2f;

        // Height of ground objects
        float groundHeight = Vector3.Distance(intersection, bottom);

        // LEFT ground
        GameObject leftObj = Instantiate(leftGroundPrefab, leftMid - new Vector3(0, groundHeight / 2f, 0), Quaternion.identity, this.transform);
        leftObj.transform.localScale = new Vector3(halfWidth, groundHeight, 1f);

        // RIGHT ground
        GameObject rightObj = Instantiate(rightGroundPrefab, rightMid - new Vector3(0, groundHeight / 2f, 0), Quaternion.identity, this.transform);
        rightObj.transform.localScale = new Vector3(halfWidth, groundHeight, 1f);
    }
}
