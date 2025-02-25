using BuildingSystem;
using BuildingSystem.Models;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class StructureManage : MonoBehaviour
{

    private ConstructionLayer _constructionLayer;

    private BuildingPlacer _buildingPlacer;

    [SerializeField] private Tilemap _tilemap;
    [SerializeField] private CollisionLayer _collisionLayer;
    [SerializeField] private GameObject generalUI;

    private void Awake()
    {
        _buildingPlacer = FindObjectOfType<BuildingPlacer>();
        _constructionLayer = FindObjectOfType<ConstructionLayer>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && !_buildingPlacer.IsbuildMode)
        {
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            worldPosition.z = 0;  
            Debug.Log($"[CrossPlatformInputUser] Click detected at world position: {worldPosition}");

            HandleBuildableSelection(worldPosition);
        }
    }

    private void HandleBuildableSelection(Vector3 worldCoords)
    {
        if (IsPointerOverUI ())
        {
            return;
        }
        if (_buildingPlacer.IsActiveBuildable())
        {
            return;
        }

        // Perform a raycast from the mouse position in world space
        RaycastHit2D hit = Physics2D.Raycast(worldCoords, Vector2.zero);  // Raycast directly under the mouse position

        // Check if the raycast hit an object
        if (hit.collider != null)
        {
            GameObject buildableObject = hit.collider.gameObject;
            if (buildableObject == null)
            {
                Debug.LogWarning("[HandleBuildableSelection] The buildable object is null.");
                return;
            }

            Debug.Log($"[HandleBuildableSelection] Buildable found: {buildableObject.name}");

            // Check if the buildable object has a Canvas component
            Transform canvasTransform = buildableObject.transform.Find("Canvas");
            if (canvasTransform == null)
            {
                Debug.LogWarning("[HandleBuildableSelection] Canvas NOT found inside the Buildable.");
                return;
            }

            Debug.Log("[HandleBuildableSelection] Canvas found.");

            // Try to find the ChosePanel inside the Canvas
            Transform chosePanel = canvasTransform.Find("Chose Panel");
            if (chosePanel == null)
            {
                Debug.LogWarning("[HandleBuildableSelection] ChosePanel NOT found inside the Canvas.");
                return;
            }

            Debug.Log("[HandleBuildableSelection] ChosePanel found, enabling it.");
            chosePanel.gameObject.SetActive(true);  // Enable the panel
        }
        else
        {
            Debug.LogWarning("[HandleBuildableSelection] No object found at the clicked position.");
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

    private void OpenDefaultUI()
    {
        Debug.Log("Opening default UI...");
        generalUI?.SetActive(true);
    }

    private void UnRegisterBuildableCollisionSpace(Buildable buildable)
    {
        // Implement how you unregister collision space if necessary
    }

}
