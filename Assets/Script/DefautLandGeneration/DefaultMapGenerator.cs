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
    public GameObject portal;

    public int size = 32;
    public int portalsize = 3;

    public GameObject cloudPrefab;
    public float cloudSpacing = 2f;
    public int cloudLayers = 2;

    private Transform groundSideContainer;
    private Transform cloudContainer;

    void Start()
    {
        GenerateDefaultMap();
        var tilePos = tilemap.gameObject.transform.position;
        SpawnGroundSides();
        SpawnCloudsAroundMap();
    }

    void OnEnable()
    {
        if (GameController.Instance != null)
        {
            GameController.Instance.onSave += SpawnGroundSides;
            GameController.Instance.onLoad += SpawnGroundSides;
            GameController.Instance.onSave += SpawnCloudsAroundMap;
            GameController.Instance.onLoad += SpawnCloudsAroundMap;
        }
        else
        {
            Debug.LogWarning("GameController.Instance is null in OnEnable.");
        }
    }

    void OnDisable()
    {
        if (GameController.Instance != null)
        {
            GameController.Instance.onSave -= SpawnGroundSides;
            GameController.Instance.onLoad -= SpawnGroundSides;
        }
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
        GameController.Instance.BuildNavMesh();
    }

    void SpawnGroundSides()
    {
        if (groundSideContainer == null)
        {
            groundSideContainer = new GameObject("GroundSides").transform;
            groundSideContainer.parent = this.transform;
        }

        // Destroy previous ground side objects
        foreach (Transform child in groundSideContainer)
        {
            Destroy(child.gameObject);
        }

        Vector3 leftMost;
        Vector3 rightMost;

        if (tilemap == null)
        {
            Debug.LogError("Tilemap is not assigned!");
            return;
        }

        BoundsInt bounds = tilemap.cellBounds;
        float minX = float.MaxValue;
        float maxX = float.MinValue;
        Vector3Int leftMostCell = Vector3Int.zero;
        Vector3Int rightMostCell = Vector3Int.zero;

        foreach (Vector3Int cellPos in bounds.allPositionsWithin)
        {
            if (tilemap.HasTile(cellPos))
            {
                Vector3 worldPos = tilemap.CellToWorld(cellPos) + tilemap.tileAnchor;

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

        if (minX == float.MaxValue || maxX == float.MinValue)
        {
            Debug.LogWarning("No tiles found in the tilemap!");
            return;
        }


        leftMost = tilemap.CellToWorld(leftMostCell) + tilemap.tileAnchor;
        rightMost = tilemap.CellToWorld(rightMostCell) + tilemap.tileAnchor;

        leftMost.x = leftMost.x - 1;
        leftMost.y = (float)(leftMost.y - 0.25);
        rightMost.y = (float)(rightMost.y - 0.25);

        GameObject leftGround = Instantiate(leftGroundPrefab, leftMost, Quaternion.identity, groundSideContainer);
        GameObject rightGround = Instantiate(rightGroundPrefab, rightMost, Quaternion.identity, groundSideContainer);

        float distancelr = Vector3.Distance(leftMost, rightMost);
        float distance = distancelr/2;

        float distanceRight = Vector3.Distance(Vector3.zero, rightMost);
        float distanceLeft = Vector3.Distance(Vector3.zero, leftMost);
        float ratioRight = distanceRight / rightGround.GetComponent<Renderer>().bounds.size.x;
        float ratioLeft = distanceLeft / leftGround.GetComponent<Renderer>().bounds.size.x;

        Vector3 newScale = leftGround.transform.localScale;
        newScale.x = ratioLeft;
        newScale.y = ratioLeft; 
        leftGround.transform.localScale = newScale;

        newScale = rightGround.transform.localScale;
        newScale.x = ratioRight;
        newScale.y = ratioRight; 
        rightGround.transform.localScale = newScale;

    }

    void SpawnCloudsAroundMap()
    {

        if (tilemap == null || cloudPrefab == null)
        {
            Debug.LogError("Tilemap or CloudPrefab is not assigned.");
            return;
        }

        if (cloudContainer != null)
        {
            Destroy(cloudContainer.gameObject);
        }
        else
        {
            cloudContainer = new GameObject("Clouds").transform;
            cloudContainer.parent = this.transform;
        }


        if (tilemap == null || cloudPrefab == null)
        {
            Debug.LogError("Tilemap or CloudPrefab is not assigned.");
            return;
        }

        BoundsInt bounds = tilemap.cellBounds;
        Vector3Int topCell = Vector3Int.zero;
        Vector3Int bottomCell = Vector3Int.zero;
        Vector3Int leftCell = Vector3Int.zero;
        Vector3Int rightCell = Vector3Int.zero;

        float maxY = float.MinValue;
        float minY = float.MaxValue;
        float minX = float.MaxValue;
        float maxX = float.MinValue;

        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            if (!tilemap.HasTile(pos)) continue;
            Vector3 worldPos = tilemap.CellToWorld(pos) + tilemap.tileAnchor;

            if (worldPos.y > maxY)
            {
                maxY = worldPos.y;
                topCell = pos;
            }
            if (worldPos.y < minY)
            {
                minY = worldPos.y;
                bottomCell = pos;
            }
            if (worldPos.x < minX)
            {
                minX = worldPos.x;
                leftCell = pos;
            }
            if (worldPos.x > maxX)
            {
                maxX = worldPos.x;
                rightCell = pos;
            }
        }

        Vector3 top = tilemap.CellToWorld(topCell) + tilemap.tileAnchor;
        top.x -= 0.5f;
        Vector3 bottom = tilemap.CellToWorld(bottomCell) + tilemap.tileAnchor;
        bottom.y -= 0.5f;
        bottom.x -= 0.5f;
        Vector3 left = tilemap.CellToWorld(leftCell) + tilemap.tileAnchor;
        //left.y -= 0.5f;
        left.x -= 1;
        Vector3 right = tilemap.CellToWorld(rightCell) + tilemap.tileAnchor;
        right.y -= 0.5f;

        SpawnCloudsAlongLine(top, left, cloudContainer.transform);
        SpawnCloudsAlongLine(left, bottom, cloudContainer.transform);
        SpawnCloudsAlongLine(bottom, right, cloudContainer.transform);
        SpawnCloudsAlongLine(right, top, cloudContainer.transform);
    }

    void SpawnCloudsAlongLine(Vector3 start, Vector3 end, Transform parent)
    {
        float distance = Vector3.Distance(start, end);
        int cloudCount = Mathf.CeilToInt((distance / cloudSpacing) * 3);

        for (int i = 0; i <= cloudCount; i++)
        {
            float t = i / (float)cloudCount;

            Vector3 pos = Vector3.Lerp(start, end, t);
            //Vector3 directionToCenter = (Vector3.zero - pos).normalized;

            //Quaternion rotation = Quaternion.LookRotation(Vector3.forward, -directionToCenter);
            //Instantiate(cloudPrefab, pos, rotation, parent);
            Instantiate(cloudPrefab, pos, Quaternion.identity, parent);
        }
    }

}
