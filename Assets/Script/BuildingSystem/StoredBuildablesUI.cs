using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BuildingSystem.Models;
using BuildingSystem;

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
        _buildingPlacer.SetActiveBuildable(null);
        _buildingPlacer.ClearPreview();
    }

    private Dictionary<BuildableItem, Button> _buttons = new();

    private void OnEnable()
    {
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

            Button newButton = Instantiate(_buttonPrefab, _contentParent);
            TMP_Text buttonText = newButton.GetComponentInChildren<TMP_Text>();
            Image buttonImage = null;
            Image[] images = newButton.GetComponentsInChildren<Image>();

            foreach (var img in images)
            {
                if (img.gameObject.name == "PreviewImg")
                {
                    buttonImage = img;
                    break;
                }
            }

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
        _buildingPlacer.SetBuildableFromInventory(buildable);
        _buildingPlacer.SetStoreMode(false);
    }
}
