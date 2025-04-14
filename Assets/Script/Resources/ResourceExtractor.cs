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
        [SerializeField] private Transform miningOutputPoint; // Position where materials appear
        [SerializeField] private GameObject materialObject;   // Reference to the child GameObject (shootout)

        protected override void Start()
        {
            base.Start();

            if (Product == null)
            {
                Debug.LogWarning($"Product is not set for ResourceExtractor on {gameObject.name}. Please assign AddResourceToStorageProduct in the Inspector.");
            }
            else if (!(Product is AddResourceToStorageProduct))
            {
                Debug.LogWarning($"Product on ResourceExtractor {gameObject.name} is not an AddResourceToStorageProduct. Please assign the correct product type in the Inspector.");
            }

            // Subscribe to the product creation event to trigger animation
            onCreateProduct += OnProductCreatedHandler;

            // Ensure the materialObject is initially inactive
            if (materialObject != null)
            {
                materialObject.SetActive(false);
            }
            else
            {
                Debug.LogError("materialObject is not assigned in the Inspector!");
            }
        }

        protected override void Update()
        {
            base.Update();

            if (IsWorkable)
            {
                ProgressionPerSec = 19f;
                IncreaseProgress(ProgressionPerSec * Time.deltaTime);
            }
        }

        private void OnProductCreatedHandler(ProductBase product)
        {
            if (product is AddResourceToStorageProduct resourceProduct && resourceProduct.SelectedMaterial.HasValue)
            {
                AnimateMaterial(resourceProduct.SelectedSprite);
            }
        }

        private void AnimateMaterial(Sprite materialSprite)
        {
            if (materialSprite == null)
            {
                Debug.LogWarning("Material sprite is null, cannot animate.");
                return;
            }

            if (materialObject == null)
            {
                Debug.LogError("materialObject is not assigned in the Inspector!");
                return;
            }

            SpriteRenderer spriteRenderer = materialObject.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                Debug.LogError("materialObject does not have a SpriteRenderer component.");
                return;
            }

            // Set the sprite and activate the object
            spriteRenderer.sprite = materialSprite;
            materialObject.transform.position = miningOutputPoint.position; // Reset position
            materialObject.transform.localScale = Vector3.one * 0.2f;       // Reset scale
            spriteRenderer.color = Color.white;                             // Reset alpha to full opacity
            materialObject.SetActive(true);

            // Animate the object
            materialObject.transform.DOMoveY(miningOutputPoint.position.y + 0.7f, 0.5f)
                .SetEase(Ease.OutQuad);
            spriteRenderer.DOFade(0f, 0.8f)
                .SetDelay(0.3f)
                .OnComplete(() => materialObject.SetActive(false)); // Deactivate instead of destroying
        }
    }
}