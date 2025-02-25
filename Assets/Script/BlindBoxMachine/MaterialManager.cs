using Script.Controller;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MaterialManager : PersistentSingleton<GameController>
{
    [System.Serializable]
    public class InventoryMaterial
    {
        public CraftingMaterial material;
        public int amount;
        public int maxAmount;
    }

    public List<InventoryMaterial> inventoryMaterials;

    private static MaterialManager instance;
    public static MaterialManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<MaterialManager>();
                if (instance == null)
                {
                    GameObject obj = new GameObject();
                    obj.name = typeof(MaterialManager).Name;
                    instance = obj.AddComponent<MaterialManager>();
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
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddMaterial(CraftingMaterial craftingMaterial, int amount)
    {
        InventoryMaterial material = inventoryMaterials.FirstOrDefault(m => m.material == craftingMaterial);

        if (material != null)
        {
            material.amount = Mathf.Min(material.amount + amount, material.maxAmount);
            Debug.Log($"Added {amount} to {craftingMaterial}. New total: {material.amount}");
        }
        else
        {
            Debug.LogWarning($"Attempted to add material that does not exist: {craftingMaterial}");
        }
    }

    public bool UseMaterial(CraftingMaterial craftingMaterial, int amount)
    {
        InventoryMaterial material = inventoryMaterials.FirstOrDefault(m => m.material == craftingMaterial);

        if (material != null && material.amount >= amount)
        {
            material.amount -= amount;
            Debug.Log($"Used {amount} of {craftingMaterial}. Remaining: {material.amount}");
            return true;
        }
        else
        {
            Debug.LogWarning($"Not enough {craftingMaterial} resources. Required: {amount}, Available: {(material != null ? material.amount : 0)}");
            return false;
        }
    }

    public int GetMaterialAmount(CraftingMaterial craftingMaterial)
    {
        InventoryMaterial material = inventoryMaterials.FirstOrDefault(m => m.material == craftingMaterial);
        return material != null ? material.amount : 0;
    }

    public int GetMaterialMaxAmount(CraftingMaterial craftingMaterial)
    {
        InventoryMaterial material = inventoryMaterials.FirstOrDefault(m => m.material == craftingMaterial);
        return material != null ? material.maxAmount : 0;
    }

    public void AddMaterialMaxAmount(CraftingMaterial craftingMaterial, int amount)
    {
        InventoryMaterial material = inventoryMaterials.FirstOrDefault(m => m.material == craftingMaterial);
        if (material != null)
        {
            material.maxAmount += amount; 
        }
        //else
        //{
        //    inventoryMaterials.Add(new InventoryMaterial { material = craftingMaterial, maxAmount = amount });
        //}
    }

    public void MinusMaterialMaxAmount(CraftingMaterial craftingMaterial, int amount)
    {
        InventoryMaterial material = inventoryMaterials.FirstOrDefault(m => m.material == craftingMaterial);
        if (material != null)
        {
            material.maxAmount = Mathf.Max(0, material.maxAmount - amount); 
        }
    }
}
