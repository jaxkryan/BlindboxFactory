using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour
{
    [SerializeField] public float minZoom = 2f;   // Minimum zoom level
    [SerializeField] public float maxZoom = 8f;   // Maximum zoom level
    [SerializeField] public float zoomSpeed = 0.5f;
    [SerializeField] public float dragSpeed = 0.01f;

    private Vector3 touchStart;

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
                // Correctly set z to camera's distance from the scene
                touchStart = Camera.main.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, Camera.main.transform.position.z));
                Debug.Log("touchStart: " + touchStart);
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                // Convert touch position to world space, ensuring correct depth
                Vector3 newTouchPos = Camera.main.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, Camera.main.transform.position.z));

                // Calculate the movement direction
                Vector3 direction = touchStart - newTouchPos;
                direction.z = 0; // Prevent movement along the Z-axis

                // Move the camera
                Camera.main.transform.position += direction * dragSpeed;
                Debug.Log("direction: " + direction);
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
}
