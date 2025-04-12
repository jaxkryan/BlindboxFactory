using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Script.Controller;
using Script.Resources;

public class InventoryUIManager : MonoBehaviour
{
    public Transform gridResourcesParent;  // Grid for Resources
    public Transform gridBoxesParent;      // Grid for Blind Boxes
    public GameObject inventoryItemPrefab; // Assign InventoryItemUI prefab
    public TMP_Text maxBBText;
    [SerializeField] private RetailUI retailUI;
    [SerializeField] private GameObject retailexits;


    private void Start()
    {
        DisplayInventory();
        GameController.Instance.ResourceController.onResourceAmountChanged += HandleResourceAmountChanged;
        GameController.Instance.BoxController.onBoxAmountChanged += HandleBoxAmountChanged;
    }

    //private void Update()
    //{
    //    DisplayInventory();
    //}

    public void DisplayInventory()
    {
        GameController.Instance.BoxController.TryGetWarehouseMaxAmount(out var whMA);
        maxBBText.text = "Box amount : " +
            FormatNumber(GameController.Instance.BoxController.GetTotalBlindBoxAmount()) +
            " / " + FormatNumber(whMA);

        // Clear existing UI items before updating
        ClearInventoryUI(gridResourcesParent);
        ClearInventoryUI(gridBoxesParent);

        // Get Inventory Data
        GameController inventory = GameController.Instance;
        inventory.ResourceController.TryGetAllResourceAmounts(out var materials);

        // Display Crafting Materials in Resource Grid
        foreach (var material in materials)
        {
            if (material.Key == Script.Resources.Resource.Gold ||
                material.Key == Script.Resources.Resource.Gem)
            {
                continue;
            }
            GameController.Instance.ResourceController.TryGetData(material.Key, out var resourceData, out var currentAmount);
            CreateInventoryItemForResource(gridResourcesParent, material.Key,
                CraftingMaterialTypeManager.Instance.GetCraftingMaterialData(material.Key).sprite,
                material.Value,
                resourceData.MaxAmount
                );
        }

        inventory.BoxController.TryGetAllBoxAmounts(out var boxes);

        // Display Blind Boxes in Box Grid
        foreach (var box in boxes)
        {
            if (box.Key == BoxTypeName.Null)
            {
                continue;
            }
            CreateInventoryItem(gridBoxesParent, box.Key,
                BoxTypeManager.Instance.GetBoxData(box.Key).sprite, box.Value);
        }
    }

    private void ClearInventoryUI(Transform gridParent)
    {
        foreach (Transform child in gridParent)
        {
            Destroy(child.gameObject);
        }
    }

    private void CreateInventoryItem(Transform parentGrid, BoxTypeName itemName, Sprite itemSprite, long amount)
    {
        GameObject item = Instantiate(inventoryItemPrefab, parentGrid);
        item.transform.Find("ItemName").GetComponent<TMP_Text>().text = itemName.ToString();
        item.transform.Find("ItemImage").GetComponent<Image>().sprite = itemSprite;
        item.transform.Find("ItemAmount").GetComponent<TMP_Text>().text = FormatNumber(amount);
        Button itemButton = item.GetComponent<Button>();
        itemButton.onClick.AddListener(() => retailUI.Setup(itemName, null));
        itemButton.onClick.AddListener(() => retailUI.turnOn());
        itemButton.onClick.AddListener(() => retailexits.SetActive(true));
    }

    private void CreateInventoryItemForResource(Transform parentGrid, Resource itemName, Sprite itemSprite, long amount, long maxAmount)
    {
        GameObject item = Instantiate(inventoryItemPrefab, parentGrid);
        item.transform.Find("ItemName").GetComponent<TMP_Text>().text = itemName.ToString();
        item.transform.Find("ItemImage").GetComponent<Image>().sprite = itemSprite;
        item.transform.Find("ItemAmount").GetComponent<TMP_Text>().text = FormatNumber(amount) + " / " + FormatNumber(maxAmount);
        Button itemButton = item.GetComponent<Button>();
        itemButton.onClick.AddListener(() => retailUI.Setup(null, itemName));
        itemButton.onClick.AddListener(() => retailUI.turnOn());
        itemButton.onClick.AddListener(() => retailexits.SetActive(true));
    }

    private void HandleResourceAmountChanged(Resource resource, long oldAmount, long newAmount)
    {
        DisplayInventory();
    }

    private void HandleBoxAmountChanged(BoxTypeName btn, long oldAmount, long newAmount)
    {
        DisplayInventory();
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
