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
        public virtual bool CanCreateProduct { get => true; }

        public abstract void OnProductCreated();
    }

    [Serializable]
    public abstract class SingleProductBase : ProductBase {
        public override float MaxProgress { get => _maxProgress; }
        [SerializeField] private float _maxProgress = 100f;
        public override List<ResourceUse> ResourceUse { get => _resourceUse; }
        [SerializeReference, SubclassSelector] private List<ResourceUse> _resourceUse = new();
    }
}