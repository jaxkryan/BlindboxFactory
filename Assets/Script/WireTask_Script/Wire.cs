using UnityEngine;

public class Wire : MonoBehaviour
{
    public LineRenderer lineRenderer; // Reference to LineRenderer
    public GameObject lightOn; // Light effect when connected
    public Transform wireStart; // Assignable start position in the editor
    public Transform wireEnd; // End of the wire that moves

    private bool isDragging = false;
    private bool isConnected = false; // Track connection state

    public Color wireColor = Color.white; // Default wire color
    public Color connectedColor = Color.green; // Color when connected

    void Start()
    {
        if (wireStart == null)
        {
            Debug.LogError("Wire Start Transform is not assigned!");
            return;
        }

        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, wireStart.position);
        lineRenderer.SetPosition(1, wireStart.position); // Start with both points at the start position

        // Set the initial color
        lineRenderer.startColor = wireColor;
        lineRenderer.endColor = wireColor;

        // Ensure light is off at start
        if (lightOn != null)
        {
            lightOn.SetActive(false);
        }
    }

    private void OnMouseDown()
    {
        if (!isConnected) // Only allow dragging if not connected
        {
            isDragging = true;
        }
    }

    private void OnMouseDrag()
    {
        if (!isDragging) return;

        // Move position to world point
        Vector3 newPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        newPosition.z = 0;

        // Check for a valid connection
        Collider2D[] colliders = Physics2D.OverlapCircleAll(newPosition, 0.2f);
        foreach (Collider2D collider in colliders)
        {
            if (collider.gameObject != gameObject)
            {
                // Connect to the matching wire
                if (wireStart.parent.name.Equals(collider.transform.parent.name))
                {
                    WireTaskMain.Instance.SwitchChange(1);
                    collider.GetComponent<Wire>()?.Done();
                    Done();
                }

                return;
            }
        }

        // Update the wire position while dragging
        wireEnd.position = newPosition;
        UpdateWire();
    }

    private void OnMouseUp()
    {
        if (!isConnected) // Only reset position if not connected
        {
            isDragging = false;
            wireEnd.position = wireStart.position; // Reset if no connection
            UpdateWire();
        }
    }

    public void Done()
    {
        isConnected = true;
        lightOn.SetActive(true);
        isDragging = false;

        // Change wire color when connected
        lineRenderer.startColor = connectedColor;
        lineRenderer.endColor = connectedColor;

        // Snap the wire to the connection point
        UpdateWire();
    }

    public void Reset()
    {
        if (isConnected)
        {
            isConnected = false;
        }

        isDragging = false;
        wireEnd.position = wireStart.position; // Reset wire position
        lineRenderer.SetPosition(0, wireStart.position);
        lineRenderer.SetPosition(1, wireStart.position);

        // Reset wire color
        lineRenderer.startColor = wireColor;
        lineRenderer.endColor = wireColor;

        // Hide the light
        if (lightOn != null)
        {
            lightOn.SetActive(false);
        }
    }

    void UpdateWire()
    {
        lineRenderer.SetPosition(0, wireStart.position);
        lineRenderer.SetPosition(1, wireEnd.position);
    }
}