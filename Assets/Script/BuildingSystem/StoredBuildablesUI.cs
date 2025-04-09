using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BuildingSystem.Models;
using BuildingSystem;
using System.Linq;

// using UnityEditor.Build.Reporting;


//using UnityEditor.AddressableAssets.Build.DataBuilders;


public class StoredBuildablesUI : MonoBehaviour
{
    [SerializeField] private ConstructionLayer _constructionLayer;
    [SerializeField] private BuildingPlacer _buildingPlacer;
    [SerializeField] private Transform _contentParent;
    [SerializeField] private Button _buttonPrefab;
    [SerializeField] private GameObject InventoryUI;
    [SerializeField] private Button exitButton;
    [SerializeField] private GameObject generalUI;
    [SerializeField] private Button ShopButton;
    [SerializeField] private GameObject ShopUI;
    [SerializeField] private Button StoreModeButton;

    BuildingSelector _buildingSelector;

    private void ToggleStoreMode()
    {
        bool newMode = !_buildingPlacer.GetStoreMode();
        _buildingPlacer.SetStoreMode(newMode);
        StoreModeButton.GetComponentInChildren<TMP_Text>().text = $"Store Mode: {(newMode ? "ON" : "OFF")}";
        _buildingPlacer.SetActiveBuildable(null);
        _buildingPlacer.ClearPreview();
    }

    private Dictionary<BuildableItem, Button> _buttons = new();

    private void OnEnable()
    {
        Debug.Log("StoredBuildablesUI is now active, updating UI...");
        UpdateStoredBuildablesUI();
    }

    private void Start()
    {
        _buildingSelector = GetComponent<BuildingSelector>();
        UpdateStoredBuildablesUI();

        if (ShopButton != null)
        {
            ShopButton.onClick.AddListener(OpenShop);
        }

        if (exitButton != null)
        {
            exitButton.onClick.AddListener(ClosePanel);
        }

        if(StoreModeButton != null)
        {

            StoreModeButton.onClick.AddListener(ToggleStoreMode);
        }

    }

    private void ClosePanel()
    {
        _buildingPlacer.IsbuildMode = false;
        Debug.Log("clear ActiveBuildable");
        InventoryUI.SetActive(false);
        _buildingPlacer.SetActiveBuildable(null);
        _buildingPlacer.ClearPreview();
    }

    public void OpenShop()
    {
        _buildingPlacer.IsbuildMode = true;
        _buildingPlacer.ClearPreview();
        _buildingPlacer.SetActiveBuildable(null);
        InventoryUI.SetActive(false);
        ShopUI.SetActive(true);
    }

    public void UpdateStoredBuildablesUI()
    {
        _buildingPlacer.IsbuildMode = true;
        Debug.Log("Updating UI...");
        foreach (Transform child in _contentParent)
        {
            Destroy(child.gameObject);
        }

        _buttons.Clear();

        var storedBuildables = _constructionLayer.GetStoredBuildables();
        Debug.Log("Stored buildables count: " + storedBuildables.Count);

        foreach (var entry in storedBuildables)
        {
            var buildableItem = entry.Key;
            var count = entry.Value;

            Debug.Log($"Creating button for {buildableItem.name}, Count: {count}");

            Button newButton = Instantiate(_buttonPrefab, _contentParent);
            TMP_Text buttonText = newButton.GetComponentInChildren<TMP_Text>();
            Image buttonImage = newButton.GetComponentsInChildren<Image>()
                .FirstOrDefault(img => img.gameObject.name == "PreviewImg");

            if (buttonText == null)
            {
                Debug.LogError("Button Prefab is missing TMP_Text component!");
                return;
            }

            buttonText.text = $"{buildableItem.name} x{count}";
            buttonImage.sprite = buildableItem.PreviewSprite;
            newButton.onClick.AddListener(() => SelectBuildable(buildableItem));
            _buttons[buildableItem] = newButton;
        }
    }

    private void SelectBuildable(BuildableItem buildable)
    {
        Debug.Log("Selected Buildable: " + buildable.name);
        _buildingPlacer.SetBuildableFromInventory(buildable);
        _buildingPlacer.SetStoreMode(false);
    }
}
