using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;
using Script.Controller;
using Script.Machine;
using Script.Resources;

public class ResourceExtractor : MachineBase
{
    [System.Serializable]
    public class MaterialDropRate
    {
        public Resource material;
        public float dropRate; // Xác suất rơi
        public Sprite materialSprite; // Sprite của nguyên liệu
    }

    [System.Serializable]
    public class QuantityDropRate
    {
        public int quantity;
        public float probability; // Xác suất số lượng
    }

    public List<MaterialDropRate> materialDropRates = new List<MaterialDropRate>
    {
        new MaterialDropRate { material = Resource.Rainbow, dropRate = 0.20f },
        new MaterialDropRate { material = Resource.Cloud, dropRate = 0.20f },
        new MaterialDropRate { material = Resource.Gummy, dropRate = 0.20f },
        new MaterialDropRate { material = Resource.Ruby, dropRate = 0.15f },
        new MaterialDropRate { material = Resource.Star, dropRate = 0.15f },
        new MaterialDropRate { material = Resource.Diamond, dropRate = 0.10f }
    };

    public List<QuantityDropRate> quantityDropRates = new List<QuantityDropRate>
    {
        new QuantityDropRate { quantity = 1, probability = 0.50f },
        new QuantityDropRate { quantity = 2, probability = 0.25f },
        new QuantityDropRate { quantity = 3, probability = 0.15f },
        new QuantityDropRate { quantity = 4, probability = 0.08f },
        new QuantityDropRate { quantity = 5, probability = 0.02f }
    };

    public int level = 1;
    private float miningInterval = 5f;
    public Transform miningOutputPoint; // Vị trí xuất nguyên liệu
    public GameObject materialPrefab; // Prefab hiển thị nguyên liệu đào được

    protected override void Start()
    {
        base.Start();
        StartCoroutine(AutoMineMaterials());
    }

    private IEnumerator AutoMineMaterials()
    {
        while (true)
        {
            yield return new WaitForSeconds(miningInterval);
            MineMaterials();
        }
    }

    private void MineMaterials()
    {
        Resource? selectedMaterial = GetRandomMaterial();
        if (selectedMaterial.HasValue)
        {
            int quantity = GetRandomQuantity() + (level - 1);
            if (GameController.Instance.ResourceController.TryGetAmount(selectedMaterial.Value, out var value))
                GameController.Instance.ResourceController.TrySetAmount(selectedMaterial.Value, quantity + value);
            else Debug.LogError("Resource controller cannot find resource: " + selectedMaterial.Value);
            Debug.Log($"Mined {quantity} of {selectedMaterial.Value}");

            Sprite materialSprite = materialDropRates.First(m => m.material == selectedMaterial.Value).materialSprite;
            AnimateMaterial(materialSprite);
        }
    }

    private Resource? GetRandomMaterial()
    {
        float roll = Random.value;
        float cumulative = 0f;

        foreach (var materialRate in materialDropRates.OrderBy(m => m.dropRate))
        {
            cumulative += materialRate.dropRate;
            if (roll <= cumulative)
            {
                return materialRate.material;
            }
        }
        return materialDropRates.First().material; 
    }


    private int GetRandomQuantity()
    {
        float roll = Random.value;
        float cumulative = 0f;

        foreach (var quantityRate in quantityDropRates.OrderBy(q => q.quantity))
        {
            cumulative += quantityRate.probability;
            if (roll <= cumulative)
            {
                return quantityRate.quantity;
            }
        }
        return 1;
    }

    public void LevelUp()
    {
        level++;
        miningInterval *= 0.9f;
        Debug.Log($"Level Up! New Level: {level}, New Mining Interval: {miningInterval}s");
    }

    private void AnimateMaterial(Sprite materialSprite)
    {
        GameObject materialObject = Instantiate(materialPrefab, miningOutputPoint.position, Quaternion.identity);
        SpriteRenderer spriteRenderer = materialObject.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = materialSprite;

        materialObject.transform.localScale = Vector3.one * 0.2f;
        materialObject.transform.DOMoveY(miningOutputPoint.position.y + 0.7f, 0.5f).SetEase(Ease.OutQuad);
        spriteRenderer.DOFade(0f, 0.8f).SetDelay(0.3f).OnComplete(() => Destroy(materialObject));
    }

}
