using Script.Machine;
using Script.Machine.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class CraftingMaterialSprite
{
    public CraftingMaterial material;
    public Sprite sprite;
}

public class BlindBoxMachine : MachineBase
{
    public BlindBox currentBlindBox;
    public int amount;
    [SerializeField] public List<Recipe> recipes;
    [SerializeField] public int maxAmount;
    [SerializeField] public int electricConsumption ;
    [SerializeField] public int level;
    private void Update()
    {
        if (Product == null && amount > 0)
        {
            Product = currentBlindBox;
        }

        if(CurrentProgress >= MaxProgress)
        {
            CreateProduct();
        }
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
