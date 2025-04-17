using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // Use TextMeshPro
using BuildingSystem.Models;
using BuildingSystem;
using System.Linq;
using Script.Controller;

[System.Serializable]
public class BuildableCategory
{
    public string categoryName; // Optional: Name for the category
    public List<BuildableItem> buildables = new List<BuildableItem>();
}

public class BuildingSelector : MonoBehaviour
{

    List<BuildableCategory> Categories => GameController.Instance.MachineController.Categories;
    [SerializeField] private Button[] categoryButtons = new Button[6];
    [SerializeField] private Sprite[] categoryIcons = new Sprite[6];

    [SerializeField] private BuildingPlacer _buildingPlacer;
    [SerializeField] private Transform _contentParent;  // Scroll View Content
    [SerializeField] private Button _buttonPrefab;      // Button Prefab
    [SerializeField] private GameObject buildingSelectionUI;
    [SerializeField] private Button exitButton;
    [SerializeField] private Button InventoryButton;
    [SerializeField] private GameObject InventoryUI;
    [SerializeField] private Button StoreModeButton;

    private int _currentCategoryIndex = 0;

    
    private void ToggleStoreMode()
    {
        bool newMode = !_buildingPlacer.GetStoreMode();
        _buildingPlacer.SetStoreMode(newMode);
        _buildingPlacer.SetActiveBuildable(null);
        _buildingPlacer.ClearPreview();
        ColorBlock cb = StoreModeButton.colors;
        cb.normalColor = newMode ? Color.green : Color.white;
        cb.selectedColor = newMode ? Color.green : Color.white;
        StoreModeButton.colors = cb;
    }
    private void Start()
    {
        Color selectedColor;
        if (ColorUtility.TryParseHtmlString("#A6F8F4", out selectedColor))
        {
            ColorBlock firstButtonColors = categoryButtons[0].colors;
            firstButtonColors.normalColor = selectedColor;
            categoryButtons[0].colors = firstButtonColors;
        }

        Debug.Log("Starting PopulateScrollView...");
        PopulateScrollView(0);

        if (exitButton != null)
        {
            exitButton.onClick.AddListener(ClosePanel);
        }

        if (InventoryButton != null)
        {
            InventoryButton.onClick.AddListener(OpenInventory);
        }

        if (StoreModeButton != null)
        {
            //StoreModeButton.GetComponentInChildren<TMP_Text>().text =
            //    $"Store Mode: {(_buildingPlacer.GetStoreMode() ? "ON" : "OFF")}";

            StoreModeButton.onClick.AddListener(ToggleStoreMode);
        }

        // Assign category button click listeners
        for (int i = 0; i < categoryButtons.Count(); i++)
        {
            int index = i; // Prevent closure issue
            categoryButtons[i].onClick.AddListener(() => SwitchCategory(index));
        }

        UpdateCategoryUI();
    }


    private void ClosePanel()
    {
        _buildingPlacer.IsbuildMode = false;
        Debug.Log("clear ActiveBuildable");
        //buildingSelectionUI.SetActive(false);
        _buildingPlacer.SetActiveBuildable(null);
        _buildingPlacer.ClearPreview();
    }

    public void OpenPanel()
    {
        _buildingPlacer.IsbuildMode = true;
        Debug.Log("open Panel Building Selection, clear General UI");
        buildingSelectionUI.SetActive(true);       
    }

    public void OpenInventory()
    {
        _buildingPlacer.IsbuildMode = true;
        _buildingPlacer.ClearPreview();
        _buildingPlacer.SetActiveBuildable(null);
        buildingSelectionUI.SetActive(false);
        InventoryUI.SetActive(true);
    }

    private void PopulateScrollView(int categoryIndex)
    {
        if (Categories[categoryIndex] == null || Categories[categoryIndex].buildables.Count == 0)
        {
            //Debug.LogError("No buildable items found for category: " + categoryIndex);
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

        // Clear existing buttons
        foreach (Transform child in _contentParent)
        {
            Destroy(child.gameObject);
        }
        foreach (var buildable in Categories[categoryIndex].buildables)
        {
            GameController.Instance.MachineController.UnlockMachines.TryGetValue(buildable.Name, out bool lockstatus);
            Debug.Log("Creating button for: " + buildable.name);

            Button newButton = Instantiate(_buttonPrefab, _contentParent);
            newButton.name = "Button_" + buildable.name;

            TMP_Text goldText = newButton.transform.Find("Image/Gold")?.GetComponentInChildren<TMP_Text>();
            TMP_Text buttonText = newButton.transform.Find("Image (1)/Name")?.GetComponentInChildren<TMP_Text>();
            Image goldImg = newButton.transform.Find("Image/Gold/GoldImg")?.GetComponentInChildren<Image>();
            if (buttonText != null)
            {
                buttonText.text = buildable.Name;
            }

            if (goldText != null)
            {
                if (buildable.Cost == null || buildable.Cost == 0)
                {
                    goldText.text = "Free";
                    goldImg.gameObject.SetActive(false);
                }
                else 
                {
                    goldText.text = FormatNumber(buildable.Cost);
                }
            }

            Image buttonImage = newButton.GetComponentsInChildren<Image>()
                .FirstOrDefault(img => img.gameObject.name == "PreviewImg");

            SpriteRenderer spriteRenderer = buildable.gameObject.GetComponent<SpriteRenderer>();
            if (buttonImage != null && spriteRenderer != null)
            {
                buttonImage.sprite = spriteRenderer.sprite;
            }

            if (lockstatus)
            {
                newButton.transform.Find("LockImg")?.gameObject.SetActive(false);
                newButton.onClick.AddListener(() => SelectBuildable(buildable));
            }
            else
            {
                newButton.onClick.AddListener(() => SelectBuildable(null));
                newButton.onClick.AddListener(() => _buildingPlacer.ClearPreview());
            }
        }
    }

    private void SwitchCategory(int newCategoryIndex)
    {
        _buildingPlacer.SetActiveBuildable(null);
        _buildingPlacer.ClearPreview();
        foreach (Button btn in categoryButtons)
        {
            ColorBlock colors = btn.colors;
            colors.selectedColor = Color.white;
            colors.normalColor = Color.white;
            btn.colors = colors;
        }
        Color selectingColor;
        if (ColorUtility.TryParseHtmlString("#A6F8F4", out selectingColor))
        {
            ColorBlock clickedColors = categoryButtons[newCategoryIndex].colors;
            clickedColors.selectedColor = selectingColor;
            clickedColors.normalColor = selectingColor;
            categoryButtons[newCategoryIndex].colors = clickedColors;
        }
        if (newCategoryIndex == _currentCategoryIndex) return; // Avoid redundant switch

        _currentCategoryIndex = newCategoryIndex;
        PopulateScrollView(newCategoryIndex);
        UpdateCategoryUI();
    }

    private void UpdateCategoryUI()
    {
        for (int i = 0; i < categoryButtons.Length; i++)
        {
            TMP_Text buttonText = categoryButtons[i].GetComponentInChildren<TMP_Text>();
            Image buttonImage = categoryButtons[i].transform.Find("Image")?.GetComponent<Image>();


            if (i == _currentCategoryIndex)
            {
                //categoryButtons[i].GetComponent<RectTransform>().sizeDelta = new Vector2(180, 60); // Expand selected
                if (buttonText != null) buttonText.text = Categories[i].categoryName;
            }
            else
            {
                //categoryButtons[i].GetComponent<RectTransform>().sizeDelta = new Vector2(60, 60); // Shrink others
                if (buttonText != null) buttonText.text = "";
            }

            if (buttonImage != null && categoryIcons.Length > i)
            {
                buttonImage.sprite = categoryIcons[i];
            }
        }
    }

    private void SelectBuildable(BuildableItem buildable)
    {
        _buildingPlacer.SetActiveBuildable(buildable);
        _buildingPlacer.SetStoreMode(false);

        if (StoreModeButton != null)
        {
            TMP_Text buttonText = StoreModeButton.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
            {
                buttonText.text = "Store Mode: OFF";
            }
        }
    }

    private string FormatNumber(long number)
    {
        if (number >= 1_000_000_000_000)
            return (number / 1_000_000_000_000f).ToString("0.##") + "T";
        else if (number >= 1_000_000_000)
            return (number / 1_000_000_000f).ToString("0.##") + "B";
        else if (number >= 1_000_000)
            return (number / 1_000_000f).ToString("0.##") + "M";
        else if (number >= 1_000)
            return (number / 1_000f).ToString("0.##") + "k";
        else
            return number.ToString(); // normal number
    }
}