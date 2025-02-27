using System;
using System.Collections.Generic;
using System.Linq;
using MyBox;
using Script.Machine.ResourceManager;
using Script.Resources;
using UnityEngine;

namespace Script.Machine.Products {
    [Serializable]
    public abstract class ProductLineupProduct : ProductBase {
        public override float MaxProgress {
            get {
                if (_currentProduct is null) {
                    _currentProduct = SelectNextProduct();
                }
                return _currentProduct.MaxProgress;
            }
        }
        public override List<ResourceUse> ResourceUse {
            get => GetResourceUse();
        }
        protected ProductBase _currentProduct;

        public sealed override void OnProductCreated() {
            _currentProduct.OnProductCreated();
            _currentProduct = SelectNextProduct(_currentProduct);
        }

        protected abstract List<ResourceUse> GetResourceUse();
        
        protected abstract ProductBase SelectNextProduct(ProductBase product = null);
        
        private void OnEnable() {
            if (_currentProduct is null) {
                _currentProduct = SelectNextProduct();
            }
        }
    }

    [Serializable]
    public class ProductLineupListProduct : ProductLineupProduct {
        [SerializeReference, SubclassSelector] private CollectionWrapperList<ProductBase> _products;

        protected override List<ResourceUse> GetResourceUse() {
            var list = new List<ResourceUse>();
            _products.Value.ForEach(p => list.AddRange(p.ResourceUse));
            return list;
        }

        protected override ProductBase SelectNextProduct(ProductBase current = null) {
            if ((_products?.Value.Count ?? 0) == 0) throw new System.Exception("Products lineup cannot be empty");

            if (current is null || !_products.Value.Contains(current)) return _products.Value.First();
            
            return _products.Value.Count > _products.Value.IndexOf(current) + 1 ? _products.Value.ElementAt(_products.Value.IndexOf(current) + 1) : _products.Value.First();
        }
    }
}