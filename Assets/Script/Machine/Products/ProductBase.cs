using System;
using System.Collections.Generic;
using Script.Machine.ResourceManager;
using Script.Resources;
using UnityEngine;

namespace Script.Machine.Products {
    [Serializable]
    public abstract class ProductBase : IProduct {
        public abstract float MaxProgress { get; }
        public abstract List<ResourceUse> ResourceUse { get; }
        public abstract void OnProductCreated();
    }
}