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
    [SerializeField] public ScrollRect scrollRect;
    [SerializeField] public RectTransform content;
    [SerializeField] public GameObject leftArrow;
    [SerializeField] public GameObject rightArrow;
    [SerializeField] public Slider numberSlider;

    private Recipe currentrecipe;

    private void Update()
    {
        UpdateArrows();
    }

    void UpdateArrows()
    {
        if (!scrollRect || !content) return;

        float contentWidth = content.rect.width;
        float viewportWidth = scrollRect.viewport.rect.width;
        float normalizedPos = scrollRect.horizontalNormalizedPosition;

        bool canScroll = contentWidth > viewportWidth;
        leftArrow.SetActive(canScroll && normalizedPos > 0.01f);
        rightArrow.SetActive(canScroll && normalizedPos < 0.99f);
    }
    public void SetupRecipe(Recipe recipe, Dictionary<CraftingMaterial, Sprite> materialSprites)
    {
        currentrecipe = recipe;
        // Clean up existing material objects first
        foreach (Transform child in materialContainer)
        {
            Destroy(child.gameObject);
        }

        //if(recipe.materials.Count > 2)
        //{
        //     seeMore.SetActive(true);
        //}
        //else
        //{
        //     seeMore.SetActive(false);
        //}

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

        BoxTypeManager boxTypeManager = FindObjectOfType<BoxTypeManager>();

        BoxData boxData = boxTypeManager.GetBoxData(recipe.result.boxTypeName);
        // Setup result and text
        if (resultImage && boxData.sprite)
        {
            if (boxData.sprite == null)
            {
                Debug.LogError($"[RecipeButton] Missing boxSprite for recipe result: {recipe.result.boxTypeName}");
            }
            if (resultImage == null)
            {
                Debug.LogError("[RecipeButton] resultImage reference is missing in the Inspector!");
            }
            Debug.Log($"[RecipeButton] Setting sprite for {recipe.result.boxTypeName}");
            resultImage.sprite = boxData.sprite;
            resultImage.preserveAspect = true;

            if (recipeText)
            {
                recipeText.text = $"{recipe.result.boxTypeName}";
            }
        }

        if (recipeText)
        {
            string formulaText = $"{recipe.result.boxTypeName}";
            recipeText.text = formulaText;
        }
    }
    public void CraftingAdd()
    {
        BlindBoxWithNumber blindBoxNumberPerCraft = new BlindBoxWithNumber();

        if ((int)numberSlider.value == 0) return;


        blindBoxNumberPerCraft.number = (int)numberSlider.value;
        blindBoxNumberPerCraft.boxTypeName = currentrecipe.result.boxTypeName;

        RecipeListUI.Instance.machine.EnqueueProduct(blindBoxNumberPerCraft);
        BlindBoxQueueDisplay.Instance.UpdateQueueUI();

    }
}