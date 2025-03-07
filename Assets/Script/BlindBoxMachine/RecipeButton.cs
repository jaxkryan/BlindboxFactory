using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
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

    private BlindBox currentBlindBox;

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
    public void SetupRecipe(BlindBox blindbox)
    {
        currentBlindBox = blindbox;
        // Clean up existing material objects first
        foreach (Transform child in materialContainer)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < currentBlindBox.ResourceUse.Count; i++)
        {
            var material = currentBlindBox.ResourceUse[i];
            string materialObjectName = $"Material_{i}";

            GameObject materialObj = Instantiate(materialPrefab, materialContainer);
            materialObj.name = materialObjectName;

            // Get references from prefab (Image & Text components)
            Image materialImage = materialObj.transform.Find("Image")?.GetComponent<Image>();
            TextMeshProUGUI amountText = materialObj.transform.Find("Text")?.GetComponent<TextMeshProUGUI>();

            // Set sprite if available
            CMData cMData = CraftingMaterialTypeManager.Instance.GetCraftingMaterialData(material.Resource);
            if (cMData.sprite && materialImage)
            {
                materialImage.sprite = cMData.sprite;
                materialImage.preserveAspect = true;
            }

            // Set material amount
            if (amountText)
            {
                amountText.text = material.Resource.ToString() + " X " + material.Amount.ToString();
            }
        }

        BoxTypeManager boxTypeManager = FindFirstObjectByType<BoxTypeManager>();

        BoxData boxData = boxTypeManager.GetBoxData(blindbox.boxTypeName);
        // Setup result and text
        if (resultImage && boxData.sprite)
        {
            if (boxData.sprite == null)
            {
                Debug.LogError($"[RecipeButton] Missing boxSprite for recipe result: {blindbox.boxTypeName}");
            }
            if (resultImage == null)
            {
                Debug.LogError("[RecipeButton] resultImage reference is missing in the Inspector!");
            }
            Debug.Log($"[RecipeButton] Setting sprite for {blindbox.boxTypeName}");
            resultImage.sprite = boxData.sprite;
            resultImage.preserveAspect = true;

            if (recipeText)
            {
                recipeText.text = $"{blindbox.boxTypeName}";
            }
        }

        if (recipeText)
        {
            string formulaText = $"{blindbox.boxTypeName}";
            recipeText.text = formulaText;
        }
    }
    public void CraftingAdd()
    {
        
            Debug.Log("[CraftingAdd] Function was called.");

        if (BlindBoxQueueDisplay.Instance == null)
        {
            Debug.LogError("[CraftingAdd] BlindBoxQueueDisplay.Instance is NULL!");
            return;
        }

        //BlindBoxWithNumber blindBoxNumberPerCraft = new BlindBoxWithNumber();

        //if ((int)numberSlider.value == 0) return;


        //blindBoxNumberPerCraft.number = (int)numberSlider.value;
        //blindBoxNumberPerCraft.boxTypeName = currentrecipe.result.boxTypeName;

        //RecipeListUI.Instance.Machine.EnqueueProduct(blindBoxNumberPerCraft);
        //BlindBoxQueueDisplay.Instance.UpdateQueueUI();
        var amount = RecipeListUI.Instance.Machine.amount;
        if ((int)numberSlider.value == 0) return;

        if (currentBlindBox.boxTypeName == currentBlindBox.boxTypeName || currentBlindBox == null)
        {
            RecipeListUI.Instance.Machine.Product = currentBlindBox;
            
            RecipeListUI.Instance.Machine.amount += (int)numberSlider.value;
        }
        else if(amount == 0)
        {
            RecipeListUI.Instance.Machine.Product = currentBlindBox;
            RecipeListUI.Instance.Machine.amount = (int)numberSlider.value;
        }
        BlindBoxQueueDisplay.Instance.UpdateQueueUI();

    }
}