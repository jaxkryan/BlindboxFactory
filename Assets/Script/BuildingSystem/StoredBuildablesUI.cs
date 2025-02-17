using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BuildingSystem.Models;
using BuildingSystem;
using System.Linq;
using UnityEditor.Build.Reporting;

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

    private Dictionary<BuildableItem, Button> _buttons = new();

    private void OnEnable()
    {
        Debug.Log("StoredBuildablesUI is now active, updating UI...");
        UpdateStoredBuildablesUI();
    }

    private void Start()
    {
        UpdateStoredBuildablesUI();

        if (ShopButton != null)
        {
            ShopButton.onClick.AddListener(OpenShop);
        }

        if (exitButton != null)
        {
            exitButton.onClick.AddListener(ClosePanel);
        }

    }

    private void ClosePanel()
    {
        Debug.Log("clear ActiveBuildable");
        InventoryUI.SetActive(false);
        generalUI.SetActive(true);
        _buildingPlacer.SetActiveBuildable(null);
        _buildingPlacer.ClearPreview();
    }

    public void OpenShop()
    {
        _buildingPlacer.ClearPreview();
        _buildingPlacer.SetActiveBuildable(null);
        InventoryUI.SetActive(false);
        ShopUI.SetActive(true);
    }

    public void UpdateStoredBuildablesUI()
    {
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
            newButton.onClick.AddListener(() => FindObjectOfType<BuildingPlacer>().SetBuildableFromInventory(buildableItem));
            _buttons[buildableItem] = newButton;
        }
    }
}
