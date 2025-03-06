using UnityEngine;
using System.Collections.Generic;
using Script.Controller;
using Script.Resources;

public class InventoryManager : MonoBehaviour
{
    private Dictionary<BoxTypeName, int> blindBoxInventory = new Dictionary<BoxTypeName, int>();
    private Dictionary<Resource, int> craftingMaterials = new Dictionary<Resource, int>();

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
            InitializeInventory(); // Initialize default values
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeInventory()
    {
        // Initialize Blind Box Inventory
        foreach (BoxTypeName boxType in System.Enum.GetValues(typeof(BoxTypeName)))
        {
            blindBoxInventory[boxType] = 0;
        }

        // Initialize Crafting Material Inventory
        foreach (Resource material in System.Enum.GetValues(typeof(Resource)))
        {
            if(material != Resource.Energy && material != Resource.Gold && material != Resource.Meal)
            {
                craftingMaterials[material] = 1000;
            }
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

        blindBoxInventory[boxType] += amount;
        return true;
    }

    public bool UseBlindBox(BoxTypeName boxType, int amount)
    {
        if (blindBoxInventory[boxType] >= amount)
        {
            blindBoxInventory[boxType] -= amount;
            return true;
        }
        Debug.LogWarning($"Not enough {boxType} boxes. Available: {GetBlindBoxAmount(boxType)}, Required: {amount}");
        return false;
    }

    public int GetBlindBoxAmount(BoxTypeName boxType)
    {
        return blindBoxInventory[boxType];
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

    public bool AddCraftingMaterial(Resource material, int amount)
    {
        if (GetTotalInventoryCount() + amount > maxAmount)
        {
            Debug.LogWarning("Inventory is full! Cannot add more crafting materials.");
            return false;
        }

        craftingMaterials[material] += amount;
        return true;
    }

    public bool UseCraftingMaterial(Resource material, int amount)
    {
        if (craftingMaterials[material] >= amount)
        {
            craftingMaterials[material] -= amount;
            return true;
        }
        Debug.LogWarning($"Not enough {material} resources. Available: {GetCraftingMaterialAmount(material)}, Required: {amount}");
        return false;
    }

    public int GetCraftingMaterialAmount(Resource material)
    {
        return craftingMaterials[material];
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

    public Dictionary<Resource, int> GetAllCraftingMaterials()
    {
        return craftingMaterials;
    }
}
