using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    // Dictionary to store BoxTypeName and their amounts
    private Dictionary<BoxTypeName, int> blindBoxInventory = new Dictionary<BoxTypeName, int>();

    // Dictionary to store CraftingMaterials and their amounts
    private Dictionary<CraftingMaterial, int> craftingMaterials = new Dictionary<CraftingMaterial, int>();

    // Add a BlindBox to inventory
    public void AddBlindBox(BoxTypeName boxType, int amount)
    {
        if (blindBoxInventory.ContainsKey(boxType))
        {
            blindBoxInventory[boxType] += amount;
        }
        else
        {
            blindBoxInventory[boxType] = amount;
        }
    }

    // Add a CraftingMaterial to inventory
    public void AddCraftingMaterial(CraftingMaterial material, int amount)
    {
        if (craftingMaterials.ContainsKey(material))
        {
            craftingMaterials[material] += amount;
        }
        else
        {
            craftingMaterials[material] = amount;
        }
    }

    // Display inventory
    public void DisplayInventory()
    {
        Debug.Log("Blind Box Inventory:");
        foreach (var item in blindBoxInventory)
        {
            Debug.Log($"Box Type: {item.Key}, Amount: {item.Value}");
        }

        Debug.Log("Crafting Material Inventory:");
        foreach (var material in craftingMaterials)
        {
            Debug.Log($"Material: {material.Key}, Amount: {material.Value}");
        }
    }
}
