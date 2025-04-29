using Script.Controller;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LandGenerator : MonoBehaviour
{
    public Tilemap tilemap;
    public Tile groundTile;
    public Tile portalTile;
    public GameObject leftGroundPrefab;
    public GameObject rightGroundPrefab;
    public GameObject portal;

    public int size = 32;
    public int portalsize = 3;

    void Start()
    {
        GenerateDefaultMap();
        var tilePos = tilemap.gameObject.transform.position;
        SpawnGroundSides();
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

        Vector3 portalPosition = portal.transform.position;
        Vector3Int portalCellPosition = tilemap.WorldToCell(portalPosition);
        for (int x = -portalsize / 2; x < portalsize / 2; x++)
        {
            for (int y = -portalsize / 2; y < portalsize / 2; y++)
            {
                if (Mathf.Abs(x) + Mathf.Abs(y) <= size / 2)
                {
                    Vector3Int tilePosition = portalCellPosition + new Vector3Int(x, y, 0);
                    tilemap.SetTile(tilePosition, portalTile);
                }
            }
        }
        tilemap.CompressBounds();
    }

    void SpawnGroundSides()
    {
        Vector3 leftMost;
        Vector3 rightMost;

        if (tilemap == null)
        {
            Debug.LogError("Tilemap is not assigned!");
            return;
        }

        // Lấy giới hạn của tilemap (các ô có tile)
        BoundsInt bounds = tilemap.cellBounds;
        float minX = float.MaxValue;
        float maxX = float.MinValue;
        Vector3Int leftMostCell = Vector3Int.zero;
        Vector3Int rightMostCell = Vector3Int.zero;

        // Duyệt qua tất cả các ô trong tilemap
        foreach (Vector3Int cellPos in bounds.allPositionsWithin)
        {
            // Kiểm tra xem ô có tile không
            if (tilemap.HasTile(cellPos))
            {
                // Chuyển đổi tọa độ lưới sang tọa độ thế giới
                Vector3 worldPos = tilemap.CellToWorld(cellPos) + tilemap.tileAnchor;

                // Cập nhật tọa độ x nhỏ nhất và lớn nhất
                if (worldPos.x < minX)
                {
                    minX = worldPos.x;
                    leftMostCell = cellPos;
                }
                if (worldPos.x > maxX)
                {
                    maxX = worldPos.x;
                    rightMostCell = cellPos;
                }
            }
        }

        // Nếu không tìm thấy tile nào
        if (minX == float.MaxValue || maxX == float.MinValue)
        {
            Debug.LogWarning("No tiles found in the tilemap!");
            return;
        }

        // Chuyển đổi tọa độ lưới của các ô biên sang tọa độ thế giới
        leftMost = tilemap.CellToWorld(leftMostCell) + tilemap.tileAnchor;
        rightMost = tilemap.CellToWorld(rightMostCell) + tilemap.tileAnchor;

        leftMost.x = leftMost.x - 1;
        leftMost.y = (float)(leftMost.y - 0.25);
        rightMost.y = (float)(rightMost.y - 0.25);

        GameObject leftGround = Instantiate(leftGroundPrefab, leftMost, Quaternion.identity, this.transform);
        GameObject rightGround = Instantiate(rightGroundPrefab, rightMost, Quaternion.identity, this.transform);

        float distancelr = Vector3.Distance(leftMost, rightMost);
        float distance = distancelr / 2;
        float ratio = distance / leftGround.GetComponent<Renderer>().bounds.size.x;

        Vector3 newScale = leftGround.transform.localScale;
        newScale.x = ratio;
        newScale.y = ratio;
        leftGround.transform.localScale = newScale;

        newScale = rightGround.transform.localScale;
        newScale.x = ratio;
        newScale.y = ratio;
        rightGround.transform.localScale = newScale;

    }
}
