using BuildingSystem;
using BuildingSystem.Models;
using Script.Machine;
using Script.Machine.Machines.Canteen;
using Script.Machine.Machines.Generator;
using System.Linq;
using System.Resources;
using Unity.VisualScripting;
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
    [SerializeField] private GameObject machineUI;

    public StructureUIToggles uIToggles;

    private Camera mainCamera;
    private Vector3 targetCameraPosition;
    private bool isMovingCamera = false;
    private bool isTouching = false;
    private float touchStartTime;
    private const float maxTouchDuration = 0.2f;

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
            isTouching = true;
            touchStartTime = Time.time; // Record when touch starts
        }

        if (Input.GetMouseButtonUp(0) && isTouching)
        {
            float touchDuration = Time.time - touchStartTime;

            if (touchDuration <= maxTouchDuration) // Ensure it's a quick tap
            {
                Vector3 worldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                worldPosition.z = 0;
                HandleBuildableSelection(worldPosition);
            }

            isTouching = false; // Reset touch state
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
    private void OnDrawGizmos()
    {
        if (_hitCoord != Vector3.zero)
        {
            Gizmos.DrawWireSphere(_hitCoord, 0.5f);
            Gizmos.DrawLine(_hitCoord, _hitCoord + Vector3.zero);
        }
    }
    Vector3 _hitCoord = Vector3.zero;
    private void HandleBuildableSelection(Vector3 worldCoords)
    {
        if (IsPointerOverUI()) return;
        if (_buildingPlacer.IsActiveBuildable()) return;
        RaycastHit2D hit = Physics2D.RaycastAll(worldCoords, Vector2.zero).FirstOrDefault(h => h.collider is not null 
        && (h.collider.gameObject.CompareTag("BoxMachine")
            || h.collider.gameObject.CompareTag("Canteen")
            || h.collider.gameObject.CompareTag("Excavator")
            || h.collider.gameObject.CompareTag("StoreHouse")
            || h.collider.gameObject.CompareTag("RestRoom")
            || h.collider.gameObject.CompareTag("ElectricMachine")));
        _hitCoord = hit.centroid;

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
                machineUI.SetActive(true);
                machineUI.transform.Find("Chose Panel Box Machine").gameObject.SetActive(true);

                // Move the camera to focus on the selected object
                MoveCameraToFocus(buildableObject.transform.position);

                BlindBoxMachine machine = buildableObject.GetComponent<BlindBoxMachine>();
                if (machine != null)
                {
                    BlindBoxInformationDisplay.Instance.currentMachine = machine;
                }
            }
            else
            {
                Debug.Log($"[HandleBuildableSelection] Buildable found: {buildableObject.name}");

                // Show UI
                machineUI.SetActive(true);
                machineUI.transform.Find("Chose Panel All").gameObject.SetActive(true);

                // Move the camera to focus on the selected object
                MoveCameraToFocus(buildableObject.transform.position);
                Component machine = buildableObject.GetComponent(typeof(MachineBase));
                BlindBoxInformationDisplay.Instance.currentMachine = (MachineBase) machine;
            }

            BlindBoxInformationDisplay.Instance.currentCoordinate = worldCoords;
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
