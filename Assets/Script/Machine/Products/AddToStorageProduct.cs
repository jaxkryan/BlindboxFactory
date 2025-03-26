using System;
using System.Collections.Generic;
using System.Linq;
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
        public override bool CanCreateProduct { get => _storage.MaxCapacity > _storage.AvailableMeals; }
        [SerializeField] CanteenFoodStorage _storage;
        public override void OnProductCreated() {
            _storage.TryChangeMealAmount(Amount);
        }
    }



[Serializable]
    public class MaterialDropRate
    {
        public Resource material;
        public float dropRate; // Probability of dropping this material
        public Sprite materialSprite; // Sprite for animation
    }

    [Serializable]
    public class QuantityDropRate
    {
        public int quantity;
        public float probability; // Probability of this quantity
    }

    [Serializable]
    public class AddResourceToStorageProduct : AddToStorageProduct
    {
        [SerializeField] public Resource Resource;

        [SerializeField]
        private List<MaterialDropRate> materialDropRates = new List<MaterialDropRate> {
            new MaterialDropRate { material = Resource.Rainbow, dropRate = 0.20f },
            new MaterialDropRate { material = Resource.Cloud, dropRate = 0.20f },
            new MaterialDropRate { material = Resource.Gummy, dropRate = 0.20f },
            new MaterialDropRate { material = Resource.Ruby, dropRate = 0.15f },
            new MaterialDropRate { material = Resource.Star, dropRate = 0.15f },
            new MaterialDropRate { material = Resource.Diamond, dropRate = 0.10f }
        };

        [SerializeField]
        private List<QuantityDropRate> quantityDropRates = new List<QuantityDropRate> {
            new QuantityDropRate { quantity = 1, probability = 0.50f },
            new QuantityDropRate { quantity = 2, probability = 0.25f },
            new QuantityDropRate { quantity = 3, probability = 0.15f },
            new QuantityDropRate { quantity = 4, probability = 0.08f },
            new QuantityDropRate { quantity = 5, probability = 0.02f }
        };

        public override float MaxProgress => 100f;
        public override List<ResourceUse> ResourceUse => new List<ResourceUse>(); // No resources consumed

        public Resource? SelectedMaterial { get; private set; }
        public int SelectedQuantity { get; private set; }
        public Sprite SelectedSprite { get; private set; }

        public override bool CanCreateProduct
        {
            get
            {
                var controller = GameController.Instance.ResourceController;
                if (controller == null)
                {
                    Debug.LogError("ResourceController is null in GameController.Instance.");
                    return false;
                }

                // Check materials in materialDropRates
                if (materialDropRates != null && materialDropRates.Count > 0)
                {
                    foreach (var materialRate in materialDropRates)
                    {
                     

                        if (controller.TryGetData(materialRate.material, out var resourceData, out var amount))
                        {
                            long capacity = resourceData.MaxAmount; // Get the capacity from ResourceData
                            if (capacity > amount)
                            {
                                Debug.Log($"Can create product: {materialRate.material} has capacity (amount: {amount}, capacity: {capacity})");
                                return true;
                            }
                            else
                            {
                                Debug.Log($"Material {materialRate.material} is at full capacity (amount: {amount}, capacity: {capacity})");
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"Failed to get data for {materialRate.material}.");
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("materialDropRates is empty. Falling back to Resource field.");
                }

                Debug.Log("Cannot create product: No materials have available capacity.");
                return false;
            }
        }

        public override void OnProductCreated()
        {
            // Select random material and quantity
            SelectedMaterial = GetRandomMaterial();
            SelectedQuantity = GetRandomQuantity();

            if (SelectedMaterial.HasValue)
            {
                var controller = GameController.Instance.ResourceController;
                if (controller.TryGetAmount(SelectedMaterial.Value, out var currentAmount))
                {
                    controller.TrySetAmount(SelectedMaterial.Value, currentAmount + SelectedQuantity);
                    SelectedSprite = materialDropRates.First(m => m.material == SelectedMaterial.Value).materialSprite;
                    Debug.Log($"Produced {SelectedQuantity} of {SelectedMaterial.Value}");
                }
                else
                {
                    Debug.LogError($"Resource not found in controller: {SelectedMaterial.Value}");
                }
            }
            else
            {
                // Fallback to the Resource field if random selection fails
                if (Resource != null)
                {
                    var controller = GameController.Instance.ResourceController;
                    if (controller.TryGetAmount(Resource, out var currentAmount))
                    {
                        // Use the smallest quantity as a fallback
                        int fallbackQuantity = quantityDropRates.OrderBy(q => q.quantity).FirstOrDefault()?.quantity ?? 1;
                        controller.TrySetAmount(Resource, currentAmount + fallbackQuantity);
                        Debug.Log($"Produced {fallbackQuantity} of {Resource} (fallback)");
                    }
                    else
                    {
                        Debug.LogError($"Resource not found in controller: {Resource}");
                    }
                }
                else
                {
                    Debug.LogError("No material selected and Resource field is null!");
                }
            }
        }

        private Resource? GetRandomMaterial()
        {
            if (materialDropRates == null || materialDropRates.Count == 0)
            {
                return null; // No materials to select from
            }

            float roll = UnityEngine.Random.value;
            float cumulative = 0f;

            foreach (var materialRate in materialDropRates.OrderBy(m => m.dropRate))
            {
                cumulative += materialRate.dropRate;
                if (roll <= cumulative)
                {
                    return materialRate.material;
                }
            }
            return materialDropRates.FirstOrDefault()?.material; // Fallback to first material
        }

        private int GetRandomQuantity()
        {
            if (quantityDropRates == null || quantityDropRates.Count == 0)
            {
                return 1; // Fallback to 1 if no quantities are defined
            }

            float roll = UnityEngine.Random.value;
            float cumulative = 0f;

            foreach (var quantityRate in quantityDropRates.OrderBy(q => q.quantity))
            {
                cumulative += quantityRate.probability;
                if (roll <= cumulative)
                {
                    return quantityRate.quantity;
                }
            }
            return quantityDropRates.FirstOrDefault()?.quantity ?? 1; // Fallback to first quantity or 1
        }
    }
}