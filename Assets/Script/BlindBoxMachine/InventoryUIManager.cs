using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Script.Controller;

public class InventoryUIManager : MonoBehaviour
{
    public Transform gridParent;  // Assign Grid Layout Group here
    public GameObject inventoryItemPrefab; // Assign InventoryItemUI prefab

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
        // Check if the inventory is active before updating

        // Clear existing UI items before updating
        foreach (Transform child in gridParent)
        {
            Destroy(child.gameObject);
        }

        // Get Inventory Data
        GameController inventory = GameController.Instance;
        inventory.ResourceController.TryGetAllResourceAmounts(out var materials);

        // Display Crafting Materials
        foreach (var material in materials)
        {
            if (material.Key == Script.Resources.Resource.Gold ||
                material.Key == Script.Resources.Resource.Gem)
            {
                continue;
            }
            CreateInventoryItem(material.Key.ToString(), CraftingMaterialTypeManager.Instance.GetCraftingMaterialData(material.Key).sprite, material.Value);
        }

        inventory.BoxController.TryGetAllBoxAmounts(out var boxes);

        // Display Blind Boxes
        foreach (var box in boxes)
        {
            if (box.Key == BoxTypeName.Null)
            {
                continue;
            }
            CreateInventoryItem(box.Key.ToString(), BoxTypeManager.Instance.GetBoxData(box.Key).sprite, box.Value);
        }
    }


    private void CreateInventoryItem(string itemName, Sprite itemSprite, long amount)
    {
        GameObject item = Instantiate(inventoryItemPrefab, gridParent);
        item.transform.Find("ItemName").GetComponent<TMP_Text>().text = itemName;
        item.transform.Find("ItemImage").GetComponent<Image>().sprite = itemSprite;
        item.transform.Find("ItemAmount").GetComponent<TMP_Text>().text = FormatNumber(amount);
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
