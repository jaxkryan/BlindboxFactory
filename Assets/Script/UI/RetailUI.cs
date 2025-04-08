using Script.Controller;
using Script.Resources;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RetailUI : MonoBehaviour
{
    [SerializeField] public BoxTypeName? boxTypeName;
    [SerializeField] public Resource? resource;

    [SerializeField] GameObject panel;
    [SerializeField] Image SellingStuffImg;
    [SerializeField] TMP_Text SellingStuffName;
    [SerializeField] TMP_Text Description;
    [SerializeField] TMP_Text SellingStuffNumber;
    [SerializeField] TMP_Text RecevingNumber;
    [SerializeField] Slider SellingStuffSlider;
    [SerializeField] Button ConfirmButton;

    int mutiply = 1;

    private void OnEnable()
    {
        // Clear existing listeners to prevent stacking
        SellingStuffSlider.onValueChanged.RemoveAllListeners();
        ConfirmButton.onClick.RemoveAllListeners();

        UpdateUI();
        // Initialize slider to 0 or 1 instead of max
        SellingStuffSlider.value = 0;
    }

    private void SellingThings()
    {
        if (SellingStuffSlider.value <= 0) return; // Prevent selling 0 or negative amounts

        if (resource.HasValue)
        {
            SellingResource(resource.Value);
        }
        else if (boxTypeName.HasValue)
        {
            SellingBB(boxTypeName.Value);
        }
        UpdateUI();
    }

    private void SellingResource(Resource resource)
    {
        float number = SellingStuffSlider.value;
        if (GameController.Instance.ResourceController.TryGetAmount(resource, out long oldamount))
        {
            long sellAmount = (long)Mathf.Min((long)number, oldamount); // Don't sell more than available
            GameController.Instance.ResourceController.TrySetAmount(resource, oldamount - sellAmount);

            if (GameController.Instance.ResourceController.TryGetAmount(Resource.Gold, out long oldgold))
            {
                GameController.Instance.ResourceController.TrySetAmount(Resource.Gold, oldgold + (sellAmount * mutiply));
            }
        }
        SellingStuffSlider.value = 0; // Reset slider after selling
    }

    private void SellingBB(BoxTypeName box)
    {
        float number = SellingStuffSlider.value;
        if (GameController.Instance.BoxController.TryGetAmount(box, out long oldamount))
        {
            long sellAmount = (long)Mathf.Min((long)number, oldamount); // Don't sell more than available
            GameController.Instance.BoxController.TrySetAmount(box, oldamount - sellAmount);

            if (GameController.Instance.ResourceController.TryGetAmount(Resource.Gold, out long oldgold))
            {
                GameController.Instance.ResourceController.TrySetAmount(Resource.Gold, oldgold + (sellAmount * mutiply));
            }
        }
        SellingStuffSlider.value = 0; // Reset slider after selling
    }

    private void UpdateText(float number)
    {
        if (SellingStuffNumber != null) SellingStuffNumber.text = ((long)number).ToString();
        if (RecevingNumber != null) RecevingNumber.text = ((long)(number * mutiply)).ToString();
    }

    public void Setup(BoxTypeName? newBoxType, Resource? newResource)
    {
        boxTypeName = newBoxType;
        resource = newResource;
        if (isActiveAndEnabled) // Only update if already enabled
        {
            OnEnable();
        }
    }

    public void turnOn()
    {
        panel.gameObject.SetActive(true);
    }

    private void UpdateUI()
    {
        if (boxTypeName.HasValue)
        {
            BoxData boxdata = BoxTypeManager.Instance.GetBoxData(boxTypeName.Value);
            SellingStuffImg.sprite = boxdata.sprite;
            SellingStuffName.text = boxTypeName.Value.ToString();
            Description.text = boxdata.description.ToString();

            if (GameController.Instance.BoxController.TryGetAmount(boxTypeName.Value, out long boxAmounts))
            {
                SellingStuffSlider.maxValue = boxAmounts;
            }
            mutiply = boxdata.value;
        }
        else if (resource.HasValue)
        {
            CMData cmData = CraftingMaterialTypeManager.Instance.GetCraftingMaterialData(resource.Value);
            SellingStuffImg.sprite = cmData.sprite;
            SellingStuffName.text = resource.Value.ToString();
            Description.text = cmData.description.ToString();

            if (GameController.Instance.ResourceController.TryGetAmount(resource.Value, out long boxAmounts))
            {
                SellingStuffSlider.maxValue = boxAmounts;
            }
            mutiply = cmData.value;
        }

        // Add listeners after setting up values
        SellingStuffSlider.onValueChanged.AddListener(UpdateText);
        UpdateText(SellingStuffSlider.value);
        ConfirmButton.onClick.AddListener(SellingThings);
    }
}