using System.Linq;
using DG.Tweening;
using Script.Controller;
using Script.HumanResource.Worker;
using Script.Machine.Products;
using UnityEngine;

namespace Script.Machine
{
    public class ResourceExtractor : MachineBase
    {
        [SerializeField] private int level = 1;
        [SerializeField] private Transform miningOutputPoint; // Position where materials appear
        [SerializeField] private GameObject materialPrefab;   // Prefab for material animation

        protected override void Start()
        {
            base.Start();

            ProgressionPerSec = 20f;
            if (level > 1) ProgressionPerSec *= 1.5f;
            //SetMachineHasEnergyForWork(true);
            if (Product == null)
            {
                Debug.LogWarning($"Product is not set for ResourceExtractor on {gameObject.name}. Please assign AddResourceToStorageProduct in the Inspector.");
            }
            else if (!(Product is AddResourceToStorageProduct))
            {
                Debug.LogWarning($"Product on ResourceExtractor {gameObject.name} is not an AddResourceToStorageProduct. Please assign the correct product type in the Inspector.");
            }

            //Debug.Log("Machine workable: " + IsWorkable + CanCreateProduct + HasEnergyForWork + HasResourceForWork);

            // Subscribe to the product creation event to trigger animation
            onCreateProduct += OnProductCreatedHandler;
        }

        protected override void Update()
        {
            base.Update();

            // Increase progress each frame if workable
            if (IsWorkable)
            {
                IncreaseProgress(ProgressionPerSec * Time.deltaTime);
            }
           
        }

        public override ProductBase CreateProduct()
        {
            var product = base.CreateProduct();
            return product;
        }

        private void OnProductCreatedHandler(ProductBase product)
        {
            // Ensure the product is AddResourceToStorageProduct and cast it
            if (product is AddResourceToStorageProduct resourceProduct && resourceProduct.SelectedMaterial.HasValue)
            {
                // Trigger the animation with the selected sprite
                AnimateMaterial(resourceProduct.SelectedSprite);
            }
        }

        public void LevelUp()
        {
            level++;
        }

        private void AnimateMaterial(Sprite materialSprite)
        {
            if (materialSprite == null)
            {
                Debug.LogWarning("Material sprite is null, cannot animate.");
                return;
            }

            GameObject materialObject = Instantiate(materialPrefab, miningOutputPoint.position, Quaternion.identity);
            SpriteRenderer spriteRenderer = materialObject.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                Debug.LogError("Material prefab does not have a SpriteRenderer component.");
                Destroy(materialObject);
                return;
            }

            spriteRenderer.sprite = materialSprite;

            materialObject.transform.localScale = Vector3.one * 0.2f;
            materialObject.transform.DOMoveY(miningOutputPoint.position.y + 0.7f, 0.5f).SetEase(Ease.OutQuad);
            spriteRenderer.DOFade(0f, 0.8f).SetDelay(0.3f).OnComplete(() => Destroy(materialObject));
        }

        // Override to disable worker functionality
        public override void AddWorker(Worker worker, MachineSlot slot)
        {
            Debug.LogWarning("ResourceExtractor does not support workers.");
        }

        public override void RemoveWorker(Worker worker)
        {
            Debug.LogWarning("ResourceExtractor does not support workers.");
        }

       
    }
}