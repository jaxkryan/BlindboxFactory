using UnityEngine;

public class Wire : MonoBehaviour
{
    public LineRenderer lineRenderer; // Reference to LineRenderer
    public GameObject lightOn; // Light effect when connected
    public Transform wireStart; // Assignable start position in the editor
    public Transform wireEnd; // End of the wire that moves

    private bool isDragging = false;

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
    }

    private void OnMouseDown()
    {
        isDragging = true;
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
        isDragging = false;
        wireEnd.position = wireStart.position; // Reset if no connection
        UpdateWire();
    }

    void Done()
    {
        lightOn.SetActive(true);
        isDragging = false;

        // Change wire color when connected
        lineRenderer.startColor = connectedColor;
        lineRenderer.endColor = connectedColor;

        Destroy(this);
    }

    void UpdateWire()
    {
        lineRenderer.SetPosition(0, wireStart.position);
        lineRenderer.SetPosition(1, wireEnd.position);
    }
}
