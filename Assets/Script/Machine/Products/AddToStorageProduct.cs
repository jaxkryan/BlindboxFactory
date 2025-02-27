using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using MyBox;
using Script.Machine.Machines.Canteen;
using Script.Machine.ResourceManager;
using Script.Resources;
using UnityEngine;

namespace Script.Machine.Products {
    [Serializable]
    public abstract class AddToStorageProduct : ProductBase {
        public override float MaxProgress { get => _maxProgress; }
        [SerializeField] private float _maxProgress = 100f; 
        public override List<ResourceUse> ResourceUse { get => _resourceUse.Value; }
        [SerializeReference] private CollectionWrapperList<ResourceUse> _resourceUse;
        public int Amount { get => _amount; set => _amount = value; }
        [SerializeField] private int _amount;
    }
    
    [Serializable]
    public class KitchenMealProduct : AddToStorageProduct {
        [SerializeField] CanteenFoodStorage _storage;
        public override void OnProductCreated() {
            _storage.TryChangeMealAmount(Amount);
        }
    }
}