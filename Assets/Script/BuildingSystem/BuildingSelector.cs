using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // Use TextMeshPro
using BuildingSystem.Models;
using BuildingSystem;
using System.Linq;

public class BuildingSelector : MonoBehaviour
{
    [SerializeField] private List<BuildableItem> _buildables;
    [SerializeField] private BuildingPlacer _buildingPlacer;
    [SerializeField] private Transform _contentParent;  // Scroll View Content
    [SerializeField] private Button _buttonPrefab;      // Button Prefab
    [SerializeField] private GameObject buildingSelectionUI;
    [SerializeField] private Button exitButton;
    [SerializeField] private GameObject generalUI;
    [SerializeField] private Button InventoryButton;
    [SerializeField] private GameObject InventoryUI;
    private void Start()
    {
        Debug.Log("Starting PopulateScrollView...");
        PopulateScrollView();
        if (exitButton != null)
        {
            exitButton.onClick.AddListener(ClosePanel);
        }

        if (InventoryButton != null)
        {
            InventoryButton.onClick.AddListener(OpenInventory);
        }
    }

    private void ClosePanel()
    {
        Debug.Log("clear ActiveBuildable");
        buildingSelectionUI.SetActive(false);  
        generalUI.SetActive(true);
        _buildingPlacer.SetActiveBuildable(null);
        _buildingPlacer.ClearPreview();
    }

    public void OpenPanel()
    {
        Debug.Log("open Panel Building Selection, clear General UI");
        buildingSelectionUI.SetActive(true);   
        generalUI.SetActive(false);            
    }

    public void OpenInventory()
    {
        buildingSelectionUI.SetActive(false);
        InventoryUI.SetActive(true);
    }

    private void PopulateScrollView()
    {
        if (_buildables == null || _buildables.Count == 0)
        {
            Debug.LogError("No buildable items found!");
            return;
        }

        if (_contentParent == null)
        {
            Debug.LogError("Content parent is not assigned!");
            return;
        }

        if (_buttonPrefab == null)
        {
            Debug.LogError("Button prefab is not assigned!");
            return;
        }

        foreach (var buildable in _buildables)
        {
            Debug.Log("Creating button for: " + buildable.name);

            // Instantiate button
            Button newButton = Instantiate(_buttonPrefab, _contentParent);
            newButton.name = "Button_" + buildable.name;

            // Find TMP_Text component
            TMP_Text buttonText = newButton.GetComponentInChildren<TMP_Text>();
            Image buttonImage = newButton.GetComponentsInChildren<Image>()
                .FirstOrDefault(img => img.gameObject.name == "PreviewImg");
            buttonText.text = buildable.name;
            buttonImage.sprite = buildable.PreviewSprite;

            // Assign click event
            newButton.onClick.AddListener(() => SelectBuildable(buildable));
        }
    }

    private void SelectBuildable(BuildableItem buildable)
    {
        Debug.Log("Selected Buildable: " + buildable.name);
        _buildingPlacer.SetActiveBuildable(buildable);
    }
}
