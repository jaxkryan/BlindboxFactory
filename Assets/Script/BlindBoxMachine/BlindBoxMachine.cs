using Script.Machine;
using Script.Machine.Products;
using Script.Machine.ResourceManager;
using Script.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
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
    protected override void Update()
    {
        base.Update();
    }

    protected override void Start()
    {
        BlindBox nullbb = new BlindBox()
        {
            boxTypeName = BoxTypeName.Null,
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
        Debug.Log("calling create");
        var ret = base.CreateProduct();
        if (amount-- <= 0 && !(Product is BlindBox bbProduct && bbProduct.boxTypeName == BoxTypeName.Null))
        {
            Debug.LogWarning("Product order completed");
            amount = 0;
            ProductBase createdProduct = Product ?? new BlindBox { boxTypeName = BoxTypeName.Null };
            Product = new BlindBox { boxTypeName = BoxTypeName.Null };
        }

        return ret;
    }
}
