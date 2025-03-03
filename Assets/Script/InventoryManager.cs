using UnityEngine;
using System.Collections.Generic;
using Script.Controller;

public class InventoryManager : MonoBehaviour
{
    [SerializeField] private Dictionary<BoxTypeName, int> blindBoxInventory = new Dictionary<BoxTypeName, int>();
    [SerializeField] private Dictionary<CraftingMaterial, int> craftingMaterials = new Dictionary<CraftingMaterial, int>();
    [SerializeField] private int maxAmount;
    private static InventoryManager instance;
    public static InventoryManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<InventoryManager>();
                if (instance == null)
                {
                    GameObject obj = new GameObject(nameof(InventoryManager));
                    instance = obj.AddComponent<InventoryManager>();
                }
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private int GetTotalInventoryCount()
    {
        int totalCount = 0;
        foreach (var item in blindBoxInventory.Values)
        {
            totalCount += item;
        }
        foreach (var material in craftingMaterials.Values)
        {
            totalCount += material;
        }
        return totalCount;
    }
    public bool AddBlindBox(BoxTypeName boxType, int amount)
    {
        if (GetTotalInventoryCount() + amount > maxAmount)
        {
            Debug.LogWarning("Inventory is full! Cannot add more blind boxes.");
            return false;
        }

        if (blindBoxInventory.ContainsKey(boxType))
        {
            blindBoxInventory[boxType] += amount;
        }
        else
        {
            blindBoxInventory[boxType] = amount;
        }
        return true;
    }
    public bool UseBlindBox(BoxTypeName boxType, int amount)
    {
        if (blindBoxInventory.ContainsKey(boxType) && blindBoxInventory[boxType] >= amount)
        {
            blindBoxInventory[boxType] -= amount;
            return true;
        }
        Debug.LogWarning($"Not enough {boxType} boxes. Available: {GetBlindBoxAmount(boxType)}, Required: {amount}");
        return false;
    }
    public int GetBlindBoxAmount(BoxTypeName boxType)
    {
        return blindBoxInventory.ContainsKey(boxType) ? blindBoxInventory[boxType] : 0;
    }

    public int GetMaxAmount()
    {
        return maxAmount;
    }
    
    public void AddMaxAmount(int amount)
    {
        maxAmount += amount;
    }

    public void MinusMaxAmount(int amount)
    {
        maxAmount -= amount;
    }

    public bool AddCraftingMaterial(CraftingMaterial material, int amount)
    {
        if (GetTotalInventoryCount() + amount > maxAmount)
        {
            Debug.LogWarning("Inventory is full! Cannot add more crafting materials.");
            return false;
        }

        if (craftingMaterials.ContainsKey(material))
        {
            craftingMaterials[material] += amount;
        }
        else
        {
            craftingMaterials[material] = amount;
        }
        return true;
    }
    public bool UseCraftingMaterial(CraftingMaterial material, int amount)
    {
        if (craftingMaterials.ContainsKey(material) && craftingMaterials[material] >= amount)
        {
            craftingMaterials[material] -= amount;
            return true;
        }
        Debug.LogWarning($"Not enough {material} resources. Available: {GetCraftingMaterialAmount(material)}, Required: {amount}");
        return false;
    }
    public int GetCraftingMaterialAmount(CraftingMaterial material)
    {
        return craftingMaterials.ContainsKey(material) ? craftingMaterials[material] : 0;
    }
    public void DisplayInventory()
    {
        Debug.Log($"Total Inventory: {GetTotalInventoryCount()}/{maxAmount}");
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

    public Dictionary<BoxTypeName, int> GetAllBlindBoxes()
    {
        return blindBoxInventory;
    }

    public Dictionary<CraftingMaterial, int> GetAllCraftingMaterials()
    {
        return craftingMaterials;
    }

}
