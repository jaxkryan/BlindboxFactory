using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecipeButton : MonoBehaviour
{
    [SerializeField] private Transform materialContainer;
    [SerializeField] private TextMeshProUGUI recipeText;
    [SerializeField] private Image resultImage;
    [SerializeField] private Button button;
    [SerializeField] private float materialImageSize = 32f;
    [SerializeField] private GameObject materialPrefab;

    public void SetupRecipe(Recipe recipe, Dictionary<CraftingMaterial, Sprite> materialSprites)
    {
        // Clean up existing material objects first
        foreach (Transform child in materialContainer)
        {
            Destroy(child.gameObject);
        }

        // Generate material objects
        for (int i = 0; i < recipe.materials.Count; i++)
        {
            var material = recipe.materials[i];
            string materialObjectName = $"Material_{i}";

            GameObject materialObj = Instantiate(materialPrefab, materialContainer);
            materialObj.name = materialObjectName;

            // Get references from prefab (Image & Text components)
            Image materialImage = materialObj.transform.Find("Image")?.GetComponent<Image>();
            TextMeshProUGUI amountText = materialObj.transform.Find("Text")?.GetComponent<TextMeshProUGUI>();

            // Set sprite if available
            if (materialSprites.TryGetValue(material.material, out Sprite sprite) && materialImage)
            {
                materialImage.sprite = sprite;
                materialImage.preserveAspect = true;
            }

            // Set material amount
            if (amountText)
            {
                amountText.text = material.material.ToString() + " X " + material.amount.ToString();
            }
        }

        // Setup result and text
        if (resultImage && recipe.result.boxSprite)
        {
            if (recipe.result.boxSprite == null)
            {
                Debug.LogError($"[RecipeButton] Missing boxSprite for recipe result: {recipe.result.boxName}");
            }
            if (resultImage == null)
            {
                Debug.LogError("[RecipeButton] resultImage reference is missing in the Inspector!");
            }
            Debug.Log($"[RecipeButton] Setting sprite for {recipe.result.boxName}");
            resultImage.sprite = recipe.result.boxSprite;
            resultImage.preserveAspect = true;

            if (recipeText)
            {
                recipeText.text = $"{recipe.result.boxName}";
            }
        }

        if (recipeText)
        {
            string formulaText = $"{recipe.result.boxName}";
            recipeText.text = formulaText;
        }
    }
}