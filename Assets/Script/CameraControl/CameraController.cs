using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class CameraController : MonoBehaviour
{
    [SerializeField] public float minZoom = 2f;   // Minimum zoom level
    [SerializeField] public float maxZoom = 8f;   // Maximum zoom level
    [SerializeField] public float zoomSpeed = 0.5f;
    [SerializeField] public float dragSpeed = 0.01f;
    [SerializeField] private Tilemap _backgroundTilemap;

    private Vector3 touchStart;
    private bool canDrag = false;

    void Update()
    {
        if (!IsPointerOverUI())
        {
            HandleZoom();
            HandleDrag();
        }
    }

    void HandleZoom()
    {
        if (Input.touchCount == 2)
        {
            Touch touch0 = Input.GetTouch(0);
            Touch touch1 = Input.GetTouch(1);

            Vector2 touch0PrevPos = touch0.position - touch0.deltaPosition;
            Vector2 touch1PrevPos = touch1.position - touch1.deltaPosition;

            float prevMagnitude = (touch0PrevPos - touch1PrevPos).magnitude;
            float currentMagnitude = (touch0.position - touch1.position).magnitude;

            float difference = currentMagnitude - prevMagnitude;

            Camera.main.orthographicSize -= difference * zoomSpeed;
            Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, minZoom, maxZoom);
        }
    }

    void HandleDrag()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                touchStart = Camera.main.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, 0));

                // Only allow dragging if the first touch is on a valid tile
                canDrag = IsTileAtPosition(touchStart);
                Debug.Log(canDrag ? "Dragging allowed" : "Dragging blocked");
            }
            else if (touch.phase == TouchPhase.Moved && canDrag)
            {
                Vector3 newTouchPos = Camera.main.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, 0));
                Vector3 direction = touchStart - newTouchPos;
                direction.z = 0;

                Camera.main.transform.position += direction * dragSpeed;
            }
        }
    }

    private bool IsTileAtPosition(Vector3 worldPosition)
    {
        if (_backgroundTilemap == null)
        {
            Debug.LogError("_backgroundTilemap is NULL! Make sure it's assigned in the Inspector.");
            return false;
        }

        Vector3Int cellPosition = _backgroundTilemap.WorldToCell(worldPosition);
        cellPosition.z = 0; // Ensure we're checking the correct layer

        TileBase tile = _backgroundTilemap.GetTile(cellPosition);

        if (tile == null)
        {
            return false;
        }

        return true;
    }

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;
        if (EventSystem.current.IsPointerOverGameObject()) return true;

        if (Application.isMobilePlatform && Input.touchCount > 0)
        {
            return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
        }

        return false;
    }
}
