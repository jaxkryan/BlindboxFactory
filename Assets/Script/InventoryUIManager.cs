using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUIManager : MonoBehaviour
{
    public Transform gridParent;  // Assign Grid Layout Group here
    public GameObject inventoryItemPrefab; // Assign InventoryItemUI prefab

    private void Start()
    {
        DisplayInventory();
    }

    public void DisplayInventory()
    {
        // Clear existing UI items before updating
        foreach (Transform child in gridParent)
        {
            Destroy(child.gameObject);
        }

        // Get Inventory Data
        InventoryManager inventory = InventoryManager.Instance;


        // Display Crafting Materials
        foreach (var material in inventory.GetAllCraftingMaterials())
        {
            CreateInventoryItem(material.Key.ToString(), CraftingMaterialTypeManager.Instance.GetCraftingMaterialData(material.Key).sprite, material.Value);
        }

        // Display Blind Boxes
        foreach (var box in inventory.GetAllBlindBoxes())
        {
            CreateInventoryItem(box.Key.ToString(), BoxTypeManager.Instance.GetBoxData(box.Key).sprite, box.Value);
        }
    }

    private void CreateInventoryItem(string itemName, Sprite itemSprite, int amount)
    {
        GameObject item = Instantiate(inventoryItemPrefab, gridParent);
        item.transform.Find("ItemName").GetComponent<TMP_Text>().text = itemName;
        item.transform.Find("ItemImage").GetComponent<Image>().sprite = itemSprite;
        item.transform.Find("ItemAmount").GetComponent<TMP_Text>().text = FormatNumber(amount);
    }

    private string FormatNumber(int number)
    {
        if (number >= 1_000_000_000)
            return (number / 1_000_000_000f).ToString("0.##") + "B";
        else if (number >= 1_000_000)
            return (number / 1_000_000f).ToString("0.##") + "M";
        else if (number >= 1_000)
            return (number / 1_000f).ToString("0.##") + "k";
        else
            return number.ToString(); // normal number
    }
}
