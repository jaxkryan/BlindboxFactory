using System;
using System.Collections.Generic;
using System.Linq;
using MyBox;
using Script.Machine.ResourceManager;
using Script.Resources;
using Script.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

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
            _currentProduct ??= SelectNextProduct();
            _currentProduct.OnProductCreated();
            _currentProduct = SelectNextProduct(_currentProduct);
        }

        protected abstract List<ResourceUse> GetResourceUse();
        
        protected abstract ProductBase SelectNextProduct(ProductBase product = null);
    }

    [Serializable]
    public class ProductLineupListProduct : ProductLineupProduct {
        [SerializeReference, SubclassSelector] private List<ProductBase> _products;

        protected override List<ResourceUse> GetResourceUse() {
            // var list = new List<ResourceUse>();
            // _products.ForEach(p => list.AddRange(p.ResourceUse));
            // return list;
            _currentProduct ??= SelectNextProduct();
            return _currentProduct.ResourceUse;
        }

        protected override ProductBase SelectNextProduct(ProductBase current = null) {
            if ((_products?.Count ?? 0) == 0) throw new System.Exception("Products lineup cannot be empty");

            if (current is null || !_products.Contains(current)) return _products.First();
            
            return _products.Count > _products.IndexOf(current) + 1 ? _products.ElementAt(_products.IndexOf(current) + 1) : _products.First();
        }

        public override IProduct.SaveData Save() {
            var data = base.Save().CastToSubclass<ProductLineupListSaveData, IProduct.SaveData>();
            if (data is null) return base.Save();

            if (data is null) return base.Save();
            data.Products = _products.Select(p => p.Save()).ToList();
            data.CurrentProductIndex = _products.IndexOf(_currentProduct);
            
            return data;
        }

        public override void Load(IProduct.SaveData saveData) {
            if (saveData is not ProductLineupListSaveData data) return;
            try { 
                _products = data.Products.Select(p => {
                    var type = Type.GetType(p.Type);
                    if (type is null) return (ProductBase)default;
                    var product = Activator.CreateInstance(type) as ProductBase;
                    if (product is null) return (ProductBase)default;
                    product.Load(p);
                    return product;
                }).ToList();
                _currentProduct = _products.ElementAtOrDefault(data.CurrentProductIndex);
            }
            catch (System.Exception e) {
                Debug.LogException(e);
            }
        }

        public class ProductLineupListSaveData : IProduct.SaveData {
            public int CurrentProductIndex;
            public List<IProduct.SaveData> Products;
        }
    }
}