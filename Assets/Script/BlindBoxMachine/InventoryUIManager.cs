using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Script.Controller;

public class InventoryUIManager : MonoBehaviour
{
    public Transform gridResourcesParent;  // Grid for Resources
    public Transform gridBoxesParent;      // Grid for Blind Boxes
    public GameObject inventoryItemPrefab; // Assign InventoryItemUI prefab
    public TMP_Text maxBBText;

    private void Start()
    {
        DisplayInventory();
    }

    private void Update()
    {
        DisplayInventory();
    }

    public void DisplayInventory()
    {
        GameController.Instance.BoxController.TryGetWarehouseMaxAmount(out long maxAmount);

        maxBBText.text = "Box amount : " +
            FormatNumber(GameController.Instance.BoxController.GetTotalBlindBoxAmount()) +
            " / " + FormatNumber(maxAmount);

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
            CreateInventoryItemForResource(gridResourcesParent, material.Key.ToString(),
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
            CreateInventoryItem(gridBoxesParent, box.Key.ToString(),
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

    private void CreateInventoryItem(Transform parentGrid, string itemName, Sprite itemSprite, long amount)
    {
        GameObject item = Instantiate(inventoryItemPrefab, parentGrid);
        item.transform.Find("ItemName").GetComponent<TMP_Text>().text = itemName;
        item.transform.Find("ItemImage").GetComponent<Image>().sprite = itemSprite;
        item.transform.Find("ItemAmount").GetComponent<TMP_Text>().text = FormatNumber(amount);
    }

    private void CreateInventoryItemForResource(Transform parentGrid, string itemName, Sprite itemSprite, long amount, long maxAmount)
    {
        GameObject item = Instantiate(inventoryItemPrefab, parentGrid);
        item.transform.Find("ItemName").GetComponent<TMP_Text>().text = itemName;
        item.transform.Find("ItemImage").GetComponent<Image>().sprite = itemSprite;
        item.transform.Find("ItemAmount").GetComponent<TMP_Text>().text = FormatNumber(amount) + " / " + FormatNumber(maxAmount);
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
