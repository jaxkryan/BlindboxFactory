using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Script.Machine {
    [Serializable]
    public class ProductLineupProduct : ProductBase {
        public override float MaxProgress {
            get {
                if (_currentProduct is null) {
                    if ((_products?.Count ?? 0) == 0) throw new System.Exception("Products lineup cannot be empty");
                    _currentProduct = SelectNextProduct();
                }
                return _currentProduct.MaxProgress;
            }
        }
        
        [SerializeField] protected List<ProductBase> _products;
        private ProductBase _currentProduct;

        public sealed override void OnProductCreated() {
            _currentProduct.OnProductCreated();
            _currentProduct = SelectNextProduct(_currentProduct);
        }

        protected virtual ProductBase SelectNextProduct(ProductBase current = null) {
            if ((_products?.Count ?? 0) == 0) throw new System.Exception("Products lineup cannot be empty");

            if (current is null || !_products.Contains(current)) return _products.First();
            
            return _products.Count > _products.IndexOf(current) + 1 ? _products.ElementAt(_products.IndexOf(current) + 1) : _products.First();
        }

        private void OnEnable() {
            if (_currentProduct is null) {
                if ((_products?.Count ?? 0) == 0) throw new System.Exception("Products lineup cannot be empty");
                _currentProduct = SelectNextProduct();
            }
        }
    }
}