using System;
using UnityEngine;

namespace Script.Machine.Products {
    [Serializable]
    public abstract class ProductBase : IProduct {
        public abstract float MaxProgress { get; }
        public abstract void OnProductCreated();
    }
}