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

        private ResourceExtractorProduct extractorProduct;

        protected override void Start()
        {
            base.Start();

            // Initialize the product
            extractorProduct = new ResourceExtractorProduct();
            Product = extractorProduct;

            // Set initial progression rate: 20 progress/sec to complete 100 progress in 5 seconds
            ProgressionPerSec = 20f;

            // Register with MachineController (handled by base.Start())
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
            // Call the base method to trigger OnProductCreated
            var product = base.CreateProduct();

            // Animate the produced resource
            if (extractorProduct.SelectedMaterial.HasValue && extractorProduct.SelectedSprite != null)
            {
                AnimateMaterial(extractorProduct.SelectedSprite);
            }

            return product;
        }

        public void LevelUp()
        {
            level++;
            ProgressionPerSec *= 1.1f; // Increase progress speed by 10% per level
            Debug.Log($"Level Up! New Level: {level}, New ProgressionPerSec: {ProgressionPerSec}");
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

        // Override to disable worker functionality (optional, since we’re not using workers)
        public override void AddWorker(IWorker worker, MachineSlot slot)
        {
            Debug.LogWarning("ResourceExtractor does not support workers.");
        }

        public override void RemoveWorker(IWorker worker)
        {
            Debug.LogWarning("ResourceExtractor does not support workers.");
        }
    }
}