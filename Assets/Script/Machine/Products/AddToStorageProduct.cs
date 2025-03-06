using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using MyBox;
using Script.Controller;
using Script.Machine.Machines.Canteen;
using Script.Machine.ResourceManager;
using Script.Resources;
using UnityEngine;

namespace Script.Machine.Products {
    [Serializable]
    public abstract class AddToStorageProduct : SingleProductBase {
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
    
    [Serializable]
    public class AddResourceToStorageProduct : AddToStorageProduct {
        [SerializeField] public Resource Resource;
        public override void OnProductCreated() {
            var controller = GameController.Instance.ResourceController;

            if (!controller.TryGetAmount(Resource, out var amount)) {
                Debug.LogError($"Resource not found: {Resource}!");
                return;
            }
            controller.TrySetAmount(Resource, amount + Amount);
        }
        }
    }