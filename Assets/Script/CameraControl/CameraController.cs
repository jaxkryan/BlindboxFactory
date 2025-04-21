using DG.Tweening;
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

    void Update()
    {
        if (!IsPointerOverUI())
        {
            HandleZoom();
            HandleDrag();
        }
        else
        {

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
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                Vector3 newTouchPos = Camera.main.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, 0));
                Vector3 direction = touchStart - newTouchPos;
                direction.z = 0;

                Camera.main.transform.position += direction * dragSpeed;
            }
            else if (touch.phase == TouchPhase.Ended || IsPointerOverUI())
            {
                if (!IsTileAtCameraCenter())
                {
                    SnapCameraToNearestTile();
                }
            }
        }
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

    private bool IsTileAtCameraCenter()
    {
        if (_backgroundTilemap == null)
        {
            return false;
        }

        Vector3Int centerCell = _backgroundTilemap.WorldToCell(Camera.main.transform.position);
        centerCell.z = 0;
        TileBase tile = _backgroundTilemap.GetTile(centerCell);

        return tile != null;
    }

    private void SnapCameraToNearestTile()
    {
        Vector3 center = Vector3.zero;
        center.z = 7;
        if (_backgroundTilemap == null) return;

        Vector3 cameraPos = Camera.main.transform.position;
        Vector3 direction = (center - cameraPos).normalized;
        float stepSize = 0.1f;

        float maxDistance = Vector3.Distance(cameraPos, center) + 1;

        for (float dist = 0; dist <= maxDistance; dist += stepSize)
        {
            Vector3 checkPos = cameraPos + direction * dist;

            //  Only use X & Y, set Z to 0 always
            Vector3Int cellPos = _backgroundTilemap.WorldToCell(checkPos);
            cellPos.z = 0;

            if (_backgroundTilemap.HasTile(cellPos))
            {
                Vector3 tileWorldPos = _backgroundTilemap.GetCellCenterWorld(cellPos);
                Vector3 targetPosition = new Vector3(tileWorldPos.x, tileWorldPos.y, Camera.main.transform.position.z);
                Camera.main.transform.DOMove(targetPosition, 0.5f).SetEase(Ease.OutQuad);
                return;
            }
        }
    }

}
