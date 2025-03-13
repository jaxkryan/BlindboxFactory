using BuildingSystem;
using BuildingSystem.Models;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class StructureManage : MonoBehaviour
{
    private ConstructionLayer _constructionLayer;
    private BuildingPlacer _buildingPlacer;

    [SerializeField] private Tilemap _tilemap;
    [SerializeField] private CollisionLayer _collisionLayer;
    [SerializeField] private GameObject blindboxmachineUI;

    public StructureUIToggles uIToggles;

    private Camera mainCamera;
    private Vector3 targetCameraPosition;
    private bool isMovingCamera = false;

    private void Awake()
    {
        _buildingPlacer = FindFirstObjectByType<BuildingPlacer>();
        _constructionLayer = FindFirstObjectByType<ConstructionLayer>();
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && !_buildingPlacer.IsbuildMode)
        {
            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(
                Input.mousePosition);
            worldPosition.z = 0;

            HandleBuildableSelection(worldPosition);
        }

        // Smooth camera movement
        if (isMovingCamera)
        {
            
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, targetCameraPosition, Time.deltaTime * 5f);
            if (Vector3.Distance(mainCamera.transform.position, targetCameraPosition) < 0.01f)
            {
                isMovingCamera = false;
            }
        }
    }

    private void HandleBuildableSelection(Vector3 worldCoords)
    {
        if (IsPointerOverUI()) return;
        if (_buildingPlacer.IsActiveBuildable()) return;

        RaycastHit2D hit = Physics2D.Raycast(worldCoords, Vector2.zero);

        if (hit.collider != null)
        {
            GameObject buildableObject = hit.collider.gameObject;
            if (buildableObject == null)
            {
                Debug.LogWarning("[HandleBuildableSelection] The buildable object is null.");
                return;
            }

            if (buildableObject.CompareTag("BoxMachine"))
            {
                Debug.Log($"[HandleBuildableSelection] Buildable found: {buildableObject.name}");

                // Show UI
                blindboxmachineUI.SetActive(true);
                blindboxmachineUI.transform.Find("Chose Panel").gameObject.SetActive(true);

                // Move the camera to focus on the selected object
                MoveCameraToFocus(buildableObject.transform.position);

                BlindBoxMachine machine = buildableObject.GetComponent<BlindBoxMachine>();
                if (machine != null)
                {
                    BlindBoxInformationDisplay.Instance.SetCurrentDisplayedObject(machine);
                }
            }
        }
        else
        {
            Debug.LogWarning("[HandleBuildableSelection] No object found at the clicked position.");
        }
    }

    private void MoveCameraToFocus(Vector3 targetPosition)
    {
        if (mainCamera == null) return;

        // Adjust target position to be slightly lower in the camera view
        targetCameraPosition = new Vector3(targetPosition.x, targetPosition.y + 2f, mainCamera.transform.position.z);
        isMovingCamera = true;
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
