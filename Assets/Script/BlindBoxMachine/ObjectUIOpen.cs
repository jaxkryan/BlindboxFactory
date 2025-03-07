using UnityEngine;
using UnityEngine.EventSystems;

public class ObjectUIOpen : MonoBehaviour
{
    [SerializeField] private GameObject uiPanel; // Assign in Inspector
    [SerializeField] private GameObject generalUI;
    private bool isActive = false;

    private void Start()
    {
        generalUI = GameObject.FindGameObjectWithTag("generalUI");

        if (uiPanel != null)
            uiPanel.SetActive(false); // Ensure UI is hidden initially
    }
    private void Update()
    {

        if (Input.GetMouseButtonDown(0)) // Detect mouse click or mobile tap
        {
            DetectClick();
        }
    }

    private void DetectClick()
    {
        // Check if the click is on a UI element
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            Debug.Log("Click detected on UI, ignoring.");
            return;
        }

        // Convert screen position to world position
        Vector3 worldPoint = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane));
        Debug.Log($"World Position of Click: {worldPoint}");

        Collider2D hit = Physics2D.OverlapPoint(worldPoint);
        if (hit != null && hit.gameObject == gameObject)
        {
            Debug.Log("Clicked on: " + gameObject.name);
            ToggleUI();
        }
        else
        {
            Debug.Log("Click missed the object.");
        }
    }

    private void ToggleUI()
    {
        uiPanel.SetActive(true);
        generalUI?.SetActive(false);
        Debug.Log("UI Panel Active: " + isActive);
    }

    public void CloseUI()
    {
        Debug.Log("UI Closed.");
            uiPanel.SetActive(false);
            generalUI?.SetActive(true);

        isActive = false;
    }
}
