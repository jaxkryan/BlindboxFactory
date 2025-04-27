using Script.Controller;
using Script.Machine.ResourceManager;
using Script.Resources;
using System;
using System.Collections.Generic;
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

    private BlindBox currentBlindBox;

    private void Update()
    {
        UpdateArrows();
        numberSlider.maxValue = caculateMaxAmount();
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

    private int caculateMaxAmount()
    {
        BlindBoxMachine thismachine = (BlindBoxMachine)BlindBoxInformationDisplay.Instance.currentMachine;
        GameController.Instance.ResourceController.TryGetAllResourceAmounts(out var resourceAmount);
        var machineLimited = thismachine.maxAmount;
        var resouceLimited = CalculateMinResouce(resourceAmount, currentBlindBox.ResourceUse);
        return Math.Min(machineLimited,resouceLimited);
    }

    private int CalculateMinResouce(Dictionary<Resource, long> resourceAmount, List<ResourceUse> recipe)
    {
        int maxAmount = int.MaxValue;

        foreach (var item in recipe)
        {
            if (resourceAmount.TryGetValue(item.Resource, out long availableAmount))
            {
                maxAmount = Math.Min(maxAmount, (int)(availableAmount / item.Amount));
            }
            else
            {
                return 0;
            }
        }

        return maxAmount;
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

        BoxData boxData = boxTypeManager.GetBoxData(blindbox.BoxTypeName);
        // Setup result and text
        if (resultImage && boxData.sprite)
        {
            if (boxData.sprite == null)
            {
                Debug.LogError($"[RecipeButton] Missing boxSprite for recipe result: {blindbox.BoxTypeName}");
            }
            if (resultImage == null)
            {
                Debug.LogError("[RecipeButton] resultImage reference is missing in the Inspector!");
            }
            Debug.Log($"[RecipeButton] Setting sprite for {blindbox.BoxTypeName}");
            resultImage.sprite = boxData.sprite;
            resultImage.preserveAspect = true;

            if (recipeText)
            {
                recipeText.text = $"{blindbox.BoxTypeName}";
            }
        }

        if (recipeText)
        {
            string formulaText = $"{blindbox.BoxTypeName}";
            recipeText.text = formulaText;
        }
    }
    public void CraftingAdd()
    {
        if (BlindBoxQueueDisplay.Instance == null)
        {
            return;
        }

        if ((int)numberSlider.value == 0) return;

        var machine = RecipeListUI.Instance.Machine;
        int maxAmount = machine.maxAmount;
        int selectedAmount = Mathf.Min((int)numberSlider.value, maxAmount);

        if (machine.amount <= 0)
        {
            machine.Product = currentBlindBox;
            machine.lastBox = currentBlindBox.BoxTypeName;
            machine.amount = selectedAmount;
            machine.CurrentProgress = 0;
        }
        else if (machine.Product == currentBlindBox)
        {
            machine.amount = Mathf.Min(machine.amount + selectedAmount, maxAmount);
        }

        BlindBoxQueueDisplay.Instance.UpdateQueueUI();
    }
}