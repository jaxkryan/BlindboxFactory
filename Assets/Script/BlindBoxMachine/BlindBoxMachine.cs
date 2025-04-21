using Script.Machine;
using Script.Machine.Products;
using Script.Machine.ResourceManager;
using Script.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using Script.Utils;
using UnityEngine;

[Serializable]
public class CraftingMaterialSprite
{
    public Resource material;
    public Sprite sprite;
}

public class BlindBoxMachine : MachineBase
{
    public int amount;
    [SerializeField] public List<BlindBox> recipes;
    [SerializeField] public int maxAmount;
    public BoxTypeName lastBox;
    private Animator _animator;
    protected override void Update()
    {
        base.Update();
        if (_animator != null)
        {
            bool isActive = amount > 0;
            _animator.SetBool("IsActive", isActive);
        }
        else
        {
            Debug.Log("animator not found");
        }
    }

    public override bool IsWorkable => base.IsWorkable && amount > 0;

    protected override void Start()
    {
        BlindBox nullbb = new BlindBox()
        {
            BoxTypeName = BoxTypeName.Null,
        };
        if (CurrentProgress >= MaxProgress)
        {
            CreateProduct();
        }

        if (amount == 0)
        {
            Product = nullbb;
        }

        if (Product == null)
        {
            Product = nullbb;
        }

        base.Start();
    }


    public override ProductBase CreateProduct()
    {
        // Debug.Log($"Before CreateProduct - Amount: {amount}, Product: {Product}");

        var ret = base.CreateProduct();

        // Debug.Log($"After CreateProduct - Ret: {ret}");

        if (amount-- <= 0 && !(Product is BlindBox bbProduct && bbProduct.BoxTypeName == BoxTypeName.Null))
        {
            // Debug.LogWarning("Product order completed");
            amount = 0;

            ProductBase createdProduct = Product ?? new BlindBox { BoxTypeName = BoxTypeName.Null };
            Product = new BlindBox { BoxTypeName = BoxTypeName.Null };
            // Debug.Log($"Reset Product to: {Product}");
        }

        // Debug.Log($"Returning Product: {ret}");
        return ret;
    }


    public override void Load(MachineBaseData data) {
        base.Load(data);
        if (data is not BBMData saveData) return;

        amount = saveData.Amount;
        maxAmount = saveData.MaxAmount;
        lastBox = saveData.LastBox;
        recipes.Clear();
        foreach (var recipe in saveData.Recipes) {
            var r = Activator.CreateInstance<BlindBox>();
            r.Load(recipe);
            recipes.Add(r);
        }
    }

    public override MachineBaseData Save() {
        var data = base.Save().CastToSubclass<BBMData, MachineBaseData>();
        if (data is null) return base.Save();
        
        data.Amount = amount;
        data.MaxAmount = maxAmount;
        data.LastBox = lastBox;
        data.Recipes = recipes.Select(r => r.Save()).Cast<BlindBox.BlindBoxSaveData>().ToList();
        return data;
    }

    public BlindBox GetLastUsedRecipe()
    {
        return recipes.FirstOrDefault(box => box.BoxTypeName == lastBox);
    }

    public class BBMData : MachineBase.MachineBaseData
    {
        public int Amount;
        public List<BlindBox.BlindBoxSaveData> Recipes;
        public int MaxAmount;
        public BoxTypeName LastBox;
    }
}
