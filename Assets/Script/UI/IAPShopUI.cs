using Script.Resources;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Script.HumanResource.Administrator;
using Script.Controller;
using Script.Gacha.Base;
using UnityEngine.Purchasing;
using System;
using System.Linq;
using Unity.Services.Core; // Required for Unity Gaming Services
using System.Threading.Tasks;

public class IAPShopUI : MonoBehaviour, IStoreListener
{
    [SerializeField] private IAPShop iapShop;
    [SerializeField] private GameObject consumableItemUIPrefab;
    [SerializeField] private GameObject nonConsumableItemUIPrefab;
    [SerializeField] private GameObject subscriptionItemUIPrefab;
    [SerializeField] private Transform consumableContent;
    [SerializeField] private Transform nonConsumableContent;
    [SerializeField] private Transform subscriptionContent;
    [SerializeField] private AdministratorGacha administratorGacha;
    [SerializeField] private GachaRevealPanelUI revealPanel;

    IStoreController m_StoreController;
    private bool _isUGSInitialized = false;

    async void Awake()
    {
        // Initialize Unity Gaming Services
        try
        {
            await UnityServices.InitializeAsync();
            _isUGSInitialized = true;
            //Debug.Log("Unity Gaming Services initialized successfully.");
        }
        catch (System.Exception e)
        {
            _isUGSInitialized = false;
           // Debug.LogError($"Failed to initialize Unity Gaming Services: {e.Message}");
        }
    }

    void Start()
    {
        // Ensure the GachaRevealPanelUI is initially inactive
        if (revealPanel != null)
        {
            revealPanel.gameObject.SetActive(false);
        }
        else
        {
            //Debug.LogWarning("Start: revealPanel is not assigned in IAPShopUI!");
        }

        PopulateShopUI();

        // Only initialize IAP if UGS is initialized
        if (_isUGSInitialized)
        {
            SetUpBuilder();
        }
        else
        {
            //Debug.LogWarning("Skipping IAP initialization because Unity Gaming Services failed to initialize.");
        }
    }

    void PopulateShopUI()
    {
        foreach (Transform child in consumableContent)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in nonConsumableContent)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in subscriptionContent)
        {
            Destroy(child.gameObject);
        }

        foreach (var item in iapShop.consumableItems)
        {
            GameObject uiItem = Instantiate(consumableItemUIPrefab, consumableContent);
            SetupConsumableItemUI(uiItem, item);
        }

        foreach (var item in iapShop.nonConsumableItems)
        {
            GameObject uiItem = Instantiate(nonConsumableItemUIPrefab, nonConsumableContent);
            SetupNonConsumableItemUI(uiItem, item);
        }

        foreach (var item in iapShop.subscriptionItems)
        {
            GameObject uiItem = Instantiate(subscriptionItemUIPrefab, subscriptionContent);
            SetupSubscriptionItemUI(uiItem, item);
        }
    }

    void SetupConsumableItemUI(GameObject uiItem, ConsumableItem item)
    {
        uiItem.transform.Find("BannerImage").GetComponent<Image>().sprite = item.bannerImage;
        uiItem.transform.Find("NameText").GetComponent<TextMeshProUGUI>().text = item.name;
        uiItem.transform.Find("DescriptionText").GetComponent<TextMeshProUGUI>().text = item.description;
        uiItem.transform.Find("PriceText").GetComponent<TextMeshProUGUI>().text = $"${item.price}";
        uiItem.transform.Find("Buy_Btn").GetComponent<Button>().onClick.AddListener(() => ConsumableBtnPress(item));
    }

    void SetupNonConsumableItemUI(GameObject uiItem, NonConsumableItem item)
    {
        uiItem.transform.Find("BannerImage").GetComponent<Image>().sprite = item.bannerImage;
        uiItem.transform.Find("NameText").GetComponent<TextMeshProUGUI>().text = item.name;
        uiItem.transform.Find("DescriptionText").GetComponent<TextMeshProUGUI>().text = item.description;
        uiItem.transform.Find("PriceText").GetComponent<TextMeshProUGUI>().text = $"${item.price}";
        uiItem.transform.Find("Buy_Btn").GetComponent<Button>().onClick.AddListener(() => NonConsumableBtnPress(item));
    }

    void SetupSubscriptionItemUI(GameObject uiItem, SubscriptionItem item)
    {
        uiItem.transform.Find("BannerImage").GetComponent<Image>().sprite = item.bannerImage;
        uiItem.transform.Find("NameText").GetComponent<TextMeshProUGUI>().text = item.name;
        uiItem.transform.Find("DescriptionText").GetComponent<TextMeshProUGUI>().text = item.description;
        uiItem.transform.Find("PriceText").GetComponent<TextMeshProUGUI>().text = $"${item.price}";
        uiItem.transform.Find("TimeDurationText").GetComponent<TextMeshProUGUI>().text = $"{item.timeDuration} days";
        uiItem.transform.Find("Buy_Btn").GetComponent<Button>().onClick.AddListener(() => SubscriptionBtnPress(item));
    }

    public void ConsumableBtnPress(ConsumableItem item)
    {
        if (m_StoreController != null)
        {
            m_StoreController.InitiatePurchase(item.id);
        }
        else
        {
           // Debug.LogWarning($"Cannot purchase {item.id}: StoreController is not initialized.");
        }
    }

    public void NonConsumableBtnPress(NonConsumableItem item)
    {
        if (m_StoreController != null)
        {
            m_StoreController.InitiatePurchase(item.id);
        }
        else
        {
           // Debug.LogWarning($"Cannot purchase {item.id}: StoreController is not initialized.");
        }
    }

    public void SubscriptionBtnPress(SubscriptionItem item)
    {
        if (m_StoreController != null)
        {
            m_StoreController.InitiatePurchase(item.id);
        }
        else
        {
            //Debug.LogWarning($"Cannot purchase {item.id}: StoreController is not initialized.");
        }
    }

    void BuyConsumableItem(ConsumableItem item)
    {
        ResourceController resourceController = GameController.Instance.ResourceController;

        // Increase resource based on resourceGain and amountGain
        if (item.resourceGainData != null && item.resourceGainData.Count > 0)
        {
            foreach (ResourceGainData resource in item.resourceGainData)
            {
                if (resourceController.TryGetAmount(resource.resourceGain, out long currentAmount))
                {
                    long newAmount = currentAmount + resource.amountGain;
                    if (resourceController.TrySetAmount(resource.resourceGain, newAmount))
                    {
                        //Debug.Log($"Increased {resource.resourceGain} by {resource.amountGain}. New amount: {newAmount}");
                    }
                    else
                    {
                        //Debug.LogWarning($"Failed to set {resource.resourceGain} to {newAmount}. Max amount exceeded or invalid.");
                    }
                }
                else
                {
                   // Debug.LogWarning($"Failed to get current amount for {resource.resourceGain}.");
                }
            }
        }
        else
        {
            //Debug.Log("No resources to gain from this consumable.");
        }

        // If rewardMascot = true, pull Epic Mascot and show animation
        if (item.rewardMascot)
        {
            if (administratorGacha != null)
            {
                Mascot epicMascot = administratorGacha.PullMascotByGrade(Grade.Epic);
                if (epicMascot != null)
                {
                    ShowReveal(new List<Mascot> { epicMascot });
                   // Debug.Log($"Pulled Epic Mascot: {epicMascot.Name}");
                }
                else
                {
                   // Debug.LogWarning("Failed to pull Epic Mascot.");
                }
            }
            else
            {
                //Debug.LogError("AdministratorGacha is not assigned in IAPShopUI.");
            }
        }
       // Debug.Log($"Bought Consumable: {item.name}");
    }

    void BuyNonConsumableItem(NonConsumableItem item)
    {
       // Debug.Log($"Bought Non-Consumable: {item.name}");
        RemoveAds();
    }

    void BuySubscriptionItem(SubscriptionItem item)
    {
        //Debug.Log($"Bought Subscription: {item.name} for ${item.price}");
    }

    private void ShowReveal(List<Mascot> mascots)
    {
        if (revealPanel == null)
        {
           // Debug.LogWarning("Reveal panel is not assigned in IAPShopUI!");
            return;
        }

        revealPanel.gameObject.SetActive(true);
        revealPanel.RevealMascots(mascots, () =>
        {
            revealPanel.gameObject.SetActive(false);
        });
    }

    void SetUpBuilder()
    {
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        foreach (var i in iapShop.consumableItems)
        {
            builder.AddProduct(i.id, ProductType.Consumable);
        }
        foreach (var i in iapShop.nonConsumableItems)
        {
            builder.AddProduct(i.id, ProductType.NonConsumable);
        }
        foreach (var i in iapShop.subscriptionItems)
        {
            builder.AddProduct(i.id, ProductType.Subscription);
        }

        UnityPurchasing.Initialize(this, builder);
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
       // Debug.Log("IAP Initialization Successful");
        m_StoreController = controller;
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
    {
        var product = purchaseEvent.purchasedProduct;

        var consumableItem = iapShop.consumableItems.FirstOrDefault(i => i.id == product.definition.id);
        if (consumableItem != null)
        {
            BuyConsumableItem(consumableItem);
        }

        var nonConsumableItem = iapShop.nonConsumableItems.FirstOrDefault(i => i.id == product.definition.id);
        if (nonConsumableItem != null)
        {
            BuyNonConsumableItem(nonConsumableItem);
        }

        var subscriptionItem = iapShop.subscriptionItems.FirstOrDefault(i => i.id == product.definition.id);
        if (subscriptionItem != null)
        {
            BuySubscriptionItem(subscriptionItem);
        }

       // Debug.Log($"Purchase completed: {product.definition.id}");
        return PurchaseProcessingResult.Complete;
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
       // Debug.LogError($"IAP Initialization Failed: {error}");
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
       // Debug.LogError($"IAP Initialization Failed: {error}, {message}");
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        //Debug.LogError($"Purchase Failed for {product.definition.id}: {failureReason}");
    }

    public void CheckNonConsumable(string id)
    {
        if (m_StoreController != null)
        {
            var product = m_StoreController.products.WithID(id);
            if (product != null)
            {
                if (product.hasReceipt)
                {
                    RemoveAds();
                }
                else
                {
                  //  Debug.Log("Receipt not found");
                }
            }
            else
            {
              // Debug.Log("No product found");
            }
        }
    }

    public void CheckSubscription(string id)
    {
        if (m_StoreController != null)
        {
            var subProduct = m_StoreController.products.WithID(id);
            if (subProduct != null)
            {
                try
                {
                    if (subProduct.hasReceipt)
                    {
                        var subManager = new SubscriptionManager(subProduct, null);
                        var info = subManager.getSubscriptionInfo();
                      //  Debug.Log($"Subscription Expire Date: {info.getExpireDate()}");

                        if (info.isSubscribed() == Result.True)
                        {
                        //    Debug.Log("Subscribed");
                        }
                        else
                        {
                         //   Debug.Log("Not Subscribed");
                        }
                    }
                    else
                    {
                     //   Debug.Log("Receipt not found");
                    }
                }
                catch (System.Exception e)
                {
                   // Debug.LogWarning($"Subscription check failed: {e.Message}");
                }
            }
            else
            {
               // Debug.Log("No product found");
            }
        }
    }

    private void RemoveAds()
    {
       // Debug.Log("Remove ads");
    }
}