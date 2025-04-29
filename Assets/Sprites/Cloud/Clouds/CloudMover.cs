using UnityEngine;
using UnityEngine.Tilemaps;

public class CloudMover : MonoBehaviour
{
    public float speed = 1f;
    public float travelDistance = 20f;
    
    public Tilemap targetTilemap;  // Gán Tilemap từ Spawner
    public bool spawnRight = true; // True: bay trái → phải, False: phải → trái
    private Vector3 startPos;

    private SpriteRenderer spriteRenderer;
    private static float initialCameraSize = -1f; // static để nhớ size ban đầu
    public Camera mainCamera;
    private void OnEnable()
    {
        startPos = transform.position;
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (mainCamera == null)
            mainCamera = Camera.main;

        // Ghi lại size ban đầu
        if (initialCameraSize < 0f && mainCamera != null)
            initialCameraSize = mainCamera.orthographicSize;
    }

    private void Update()
    {
        transform.Translate(Vector3.right * speed * Time.deltaTime);

        UpdateOpacityBasedOnCamera(); // <<< THÊM DÒNG NÀY NGAY ĐÂY

        if (targetTilemap != null)
        {
            var bounds = targetTilemap.cellBounds;
            Vector3 endWorld = targetTilemap.CellToWorld(new Vector3Int(bounds.xMax, 0, 0));
            Vector3 startWorld = targetTilemap.CellToWorld(new Vector3Int(bounds.xMin, 0, 0));

            if (spawnRight && transform.position.x >= endWorld.x + 2f)
            {
                gameObject.SetActive(false);
            }
            else if (!spawnRight && transform.position.x <= startWorld.x - 2f)
            {
                gameObject.SetActive(false);
            }
        }
    }

    void UpdateOpacityBasedOnCamera()
    {
        if (mainCamera == null || spriteRenderer == null)
            return;

        float currentSize = mainCamera.orthographicSize;
        float sizeRatio = currentSize / initialCameraSize;

        Color color = spriteRenderer.color;

        if (sizeRatio >= 4f)
        {
            // Camera zoom ra gấp 4 → Mây hiện rõ (opacity 100%)
            color.a = 1f;
        }
        else if (sizeRatio >= 3f)
        {
            // Camera zoom ra gấp 3 → Mây hiện mờ mờ (opacity 40%)
            color.a = 0.4f;
        }
        else
        {
            // Camera nhỏ hơn → mây ẩn hoàn toàn
            color.a = 0f;
        }

        spriteRenderer.color = color;
    }
}
