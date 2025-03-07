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

    private void Awake()
    {
        _buildingPlacer = FindFirstObjectByType<BuildingPlacer>();
        _constructionLayer = FindFirstObjectByType<ConstructionLayer>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && !_buildingPlacer.IsbuildMode)
        {
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            worldPosition.z = 0;  

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
            if (buildableObject.CompareTag("BoxMachine"))
            {

                Debug.Log($"[HandleBuildableSelection] Buildable found: {buildableObject.name}");

                blindboxmachineUI.SetActive(true);
                blindboxmachineUI.transform.Find("Chose Panel").gameObject.SetActive(true);
                Debug.Log(buildableObject.name);
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
