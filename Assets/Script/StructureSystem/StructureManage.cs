using BuildingSystem;
using BuildingSystem.Models;
using GameInput;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class StructureManage : MonoBehaviour
{
    private CrossPlatformInputUser _inputUser;

    private ConstructionLayer _constructionLayer;

    private BuildingPlacer _buildingPlacer;

    [SerializeField] private Tilemap _tilemap;
    [SerializeField] private CollisionLayer _collisionLayer;
    [SerializeField] private GameObject generalUI;

    private void Awake()
    {
        _buildingPlacer = FindObjectOfType<BuildingPlacer>();
        _constructionLayer = FindObjectOfType<ConstructionLayer>();
        _inputUser = FindObjectOfType<CrossPlatformInputUser>(); // Get input handler
    }

    private void Update()
    {
        if (_inputUser.IsInputButtonPressed(InputButton.Primary))
        {
            Vector3 worldPosition = _inputUser.PointerWorldPosition;
            Debug.Log($"[CrossPlatformInputUser] Click detected at world position: {worldPosition}");
            HandleBuildableSelection(worldPosition);
        }
    }


    private void HandleBuildableSelection(Vector3 worldCoords)
    {
        if (_buildingPlacer.IsActiveBuildable())
        {
            return;
        }
        var buildables = _constructionLayer.GetBuildables();

        Debug.Log($"[HandleBuildableSelection] Called with worldCoords: {worldCoords}");

        // Convert world position to tilemap grid coordinates
        var coords = _tilemap.WorldToCell(worldCoords);
        Debug.Log($"[HandleBuildableSelection] Converted to tilemap coords: {coords}");

        // Check if buildable exists at the clicked location
        if (!buildables.ContainsKey(coords))
        {
            Debug.LogWarning($"[HandleBuildableSelection] No buildable found at {coords}");
            return;
        }

        // Retrieve the Buildable item
        var buildable = buildables[coords];
        Debug.Log($"[HandleBuildableSelection] Buildable found: {buildable.GameObject.name}");

        // Attempt to find the ChosePanel inside the buildable
        Transform canvasTransform = buildable.GameObject.transform.Find("Canvas");

        if (canvasTransform != null)
        {
            Debug.Log("[HandleBuildableSelection] Canvas found.");

            // Now, try to find the ChosePanel inside the Canvas
            Transform chosePanel = canvasTransform.Find("Chose Panel");

            if (chosePanel != null)
            {
                Debug.Log("[HandleBuildableSelection] ChosePanel found, enabling it.");
                chosePanel.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogWarning("[HandleBuildableSelection] ChosePanel NOT found inside the Canvas.");
            }
        }
        else
        {
            Debug.LogWarning("[HandleBuildableSelection] Canvas NOT found inside the Buildable.");
        }
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
