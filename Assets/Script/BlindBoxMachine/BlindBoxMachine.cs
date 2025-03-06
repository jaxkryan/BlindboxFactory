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

        if(CurrentProgress >= MaxProgress)
        {
            CreateProduct();
        }

        Debug.Log(CurrentProgress);
    }

    protected override void Start()
    {
        base.Start();
        Debug.Log($"MachineBase Start(): WorkDetails count = {WorkDetails.Count()}");

        foreach (var detail in WorkDetails)
        {
            Debug.Log($"Starting WorkDetail: {detail.GetType().Name}");
            detail.Start();
        }
    }


    public override ProductBase CreateProduct()
    {
        base.CreateProduct();


        if (amount <= 0)
        {
            Product = null;
            return null;
        }

        ProductBase createdProduct = Product;
        amount--;

        if (amount == 0)
        {
            Product = null;
        }

        return createdProduct;
    }
}
