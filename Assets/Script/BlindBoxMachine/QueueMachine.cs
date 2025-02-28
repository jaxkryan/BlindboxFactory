using Script.Machine;
using System.Collections.Generic;
using UnityEngine;

public abstract class QueueMachine<T> : MachineBase where T : IProduct
{
    protected Queue<T> productQueue = new Queue<T>();
    protected T currentProduct;

    public virtual void EnqueueProduct(T product)
    {
        if (currentProduct == null)
        {
            currentProduct = product;
        }
        else
        {
            productQueue.Enqueue(product);
        }
    }

    public virtual T DequeueProduct()
    {
        if (productQueue.Count > 0)
        {
            currentProduct = productQueue.Dequeue();
            return currentProduct;
        }

        return default;
    }

    public virtual List<T> GetProductQueue()
    {
        return new List<T>(productQueue);
    }

    public virtual T GetCurrentProduct() 
    {
        return currentProduct;    
    }
}