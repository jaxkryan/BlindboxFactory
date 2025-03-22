using System;
using System.Collections.Generic;
using System.Linq;
using Script.Controller;
using Script.Machine.Products;
using Script.Machine.ResourceManager;
using Script.Resources;
using UnityEngine;

namespace Script.Machine.Products
{
    [Serializable]
    public class ResourceExtractorProduct : SingleProductBase
    {
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

        public override float MaxProgress => 100f; // Progress needed to complete one cycle
        public override List<ResourceUse> ResourceUse => new List<ResourceUse>(); // No resources consumed

        // Store the selected resource and quantity for external access (e.g., animation)
        public Resource? SelectedMaterial { get; private set; }
        public int SelectedQuantity { get; private set; }
        public Sprite SelectedSprite { get; private set; }

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
        }

        private Resource? GetRandomMaterial()
        {
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
            return materialDropRates.First().material; // Fallback
        }

        private int GetRandomQuantity()
        {
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
            return 1; // Fallback
        }
    }
}