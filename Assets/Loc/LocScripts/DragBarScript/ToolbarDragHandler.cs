using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ToolbarDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private ToolbarItem itemData;
    private GameObject draggedObject;
    private Canvas canvas;
    private RectTransform draggedRect;
    private Vector2 originalPosition;
    private bool isDragging = false;

    [Header("Preview Settings")]
    public float previewAlpha = 0.6f;
    public Vector2 previewScale = new Vector2(1f, 1f);

    // Thêm reference tới prefab
    [SerializeField] private GameObject itemPrefab;

    void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Không tìm thấy Canvas!");
        }
    }

    public void Initialize(ToolbarItem item)
    {
        itemData = item;
        // Cập nhật prefab từ itemData
        itemPrefab = item.itemPrefab;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Kiểm tra null
        if (itemData == null)
        {
            Debug.LogError("ItemData chưa được khởi tạo! Hãy gọi Initialize trước.");
            return;
        }

        if (!itemData.isUnlocked) return;

        isDragging = true;
        originalPosition = transform.position;

        // Sử dụng itemPrefab thay vì itemData.itemPrefab
        if (itemPrefab != null)
        {
            draggedObject = Instantiate(itemPrefab, canvas.transform);
            draggedRect = draggedObject.GetComponent<RectTransform>();
            SetupPreviewObject();
            UpdateDraggedPosition(eventData);
        }
        else
        {
            Debug.LogError("Item Prefab is null!");
        }
        
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        UpdateDraggedPosition(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        isDragging = false;

        // Check if we can place the item
        Vector2 worldPoint = Camera.main.ScreenToWorldPoint(eventData.position);
        bool canPlace = CheckPlacement(worldPoint);

        if (canPlace)
        {
            PlaceItem(worldPoint);
        }

        // Cleanup
        if (draggedObject != null)
            Destroy(draggedObject);
    }

    private void UpdateDraggedPosition(PointerEventData eventData)
    {
        if (draggedRect != null)
        {
            Vector2 position;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                eventData.position,
                canvas.worldCamera,
                out position);
            draggedRect.position = canvas.transform.TransformPoint(position);
        }
    }

    private void SetupPreviewObject()
    {
        // Set preview appearance
        var canvasGroup = draggedObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = draggedObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = previewAlpha;
        draggedRect.localScale = previewScale;

        // Disable any gameplay components during preview
        var components = draggedObject.GetComponents<MonoBehaviour>();
        foreach (var component in components)
        {
            if (component != canvasGroup)
                component.enabled = false;
        }
    }

    private bool CheckPlacement(Vector2 worldPoint)
    {
        // Add your placement validation logic here
        // For example, check grid position, overlapping, etc.
        return true;
    }

    private void PlaceItem(Vector2 worldPoint)
    {
        // Instantiate actual game object
        GameObject placedItem = Instantiate(itemData.itemPrefab, worldPoint, Quaternion.identity);

        // Add any necessary setup for the placed item
        // For example, add to grid system, initialize components, etc.

        // Notify any necessary systems about the placement
        GameEvents.OnItemPlaced?.Invoke(itemData, placedItem, worldPoint);
    }
}

// Optional: Event system for item placement
public static class GameEvents
{
    public delegate void ItemPlacedEvent(ToolbarItem item, GameObject placedObject, Vector2 position);
    public static ItemPlacedEvent OnItemPlaced;
}