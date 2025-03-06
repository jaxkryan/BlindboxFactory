using System;
using System.Collections.Generic;
using Script.HumanResource.Worker;
using Script.Machine;
using Unity.VisualScripting;
using UnityEngine;

public abstract class QueueMachine : MachineBase
{
    [SerializeField]
    Queue<IProduct> ProductQueue { get; }

    [SerializeField]




    public override IProduct CreateProduct()
    {
        IProduct createdProduct = base.CreateProduct();

        if (ProductQueue.Count > 0)
        {
            Product = ProductQueue.Dequeue();
        }
        else
        {
            Product = null;
        }

        return createdProduct;
    }

    public virtual IProduct Product
    {
        get => Product;
        protected set => Product = value; 
    }

    public void EnqueueProduct(IProduct product)
    {
        ProductQueue.Enqueue(product);
    }
}