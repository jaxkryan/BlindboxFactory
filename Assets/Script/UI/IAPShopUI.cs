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
    void Start()
    {
        // Ensure the GachaRevealPanelUI is initially inactive
        if (revealPanel != null)
        {
            revealPanel.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Start: revealPanel is not assigned in IAPShopUI!");
        }

        PopulateShopUI();

        SetUpBuilder();
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

        m_StoreController.InitiatePurchase(item.id);
    }
    public void NonConsumableBtnPress(NonConsumableItem item)
    {

        m_StoreController.InitiatePurchase(item.id);
    }
    public void SubscriptionBtnPress(SubscriptionItem item)
    {

        m_StoreController.InitiatePurchase(item.id);
    }

    void BuyConsumableItem(ConsumableItem item)
    {

        ResourceController resourceController = GameController.Instance.ResourceController;

        // Tăng resource dựa trên resourceGain và amountGain
        if (item.resourceGainData != null && item.resourceGainData.Count > 0)
        {
            foreach (ResourceGainData resource in item.resourceGainData)
            {
                if (resourceController.TryGetAmount(resource.resourceGain, out long currentAmount))
                {
                    long newAmount = currentAmount + resource.amountGain;
                    if (resourceController.TrySetAmount(resource.resourceGain, newAmount))
                    {
                        Debug.Log($"Increased {resource} by {resource.amountGain}. New amount: {newAmount}");
                    }
                    else
                    {
                        Debug.LogWarning($"Failed to set {resource} to {newAmount}. Max amount exceeded or invalid.");
                    }
                }
                else
                {
                    Debug.LogWarning($"Failed to get current amount for {resource}.");
                }
            }
        }
        else
        {
            Debug.Log("No resources to gain from this consumable.");
        }

        // Nếu rewardMascot = true, pull Epic Mascot và hiển thị animation
        if (item.rewardMascot)
        {
            if (administratorGacha != null)
            {
                Mascot epicMascot = administratorGacha.PullMascotByGrade(Grade.Epic);
                if (epicMascot != null)
                {
                    ShowReveal(new List<Mascot> { epicMascot });
                    Debug.Log($"Pulled Epic Mascot: {epicMascot.Name}");
                }
                else
                {
                    Debug.LogWarning("Failed to pull Epic Mascot.");
                }
            }
            else
            {
                Debug.LogError("AdministratorGacha is not assigned in IAPShopUI.");
            }
        }
        Debug.Log($"Bought Consumable: {item.name}");
    }

    void BuyNonConsumableItem(NonConsumableItem item)
    {
        m_StoreController.InitiatePurchase(item.id);

        Debug.Log("Remove ads");

    }

    void BuySubscriptionItem(SubscriptionItem item)
    {
        m_StoreController.InitiatePurchase(item.id);

        Debug.Log($"Bought Subscription: {item.name} for ${item.price}");
    }

    private void ShowReveal(List<Mascot> mascots)
    {
        if (revealPanel == null)
        {
            Debug.LogWarning("Reveal panel is not assigned in IAPShopUI!");
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
        Debug.Log("INITSUCCESS");
        m_StoreController = controller;
    }

    //processing purchase 
    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
    {
        var product = purchaseEvent.purchasedProduct;

        if (product.definition.id == iapShop.consumableItems.FirstOrDefault(i => i.id == product.definition.id).id)
        {
            print("got in");
            BuyConsumableItem(iapShop.consumableItems.FirstOrDefault(i => i.id == product.definition.id));
        }
        if (product.definition.id == iapShop.nonConsumableItems.FirstOrDefault(i => i.id == product.definition.id).id)
        {
            BuyNonConsumableItem(iapShop.nonConsumableItems.FirstOrDefault(i => i.id == product.definition.id));

        }
        if (product.definition.id == iapShop.subscriptionItems.FirstOrDefault(i => i.id == product.definition.id).id)
        {
            BuySubscriptionItem(iapShop.subscriptionItems.FirstOrDefault(i => i.id == product.definition.id));

        }

        Debug.Log("Purchase compelte" + product.definition.id);
        return PurchaseProcessingResult.Complete;
    }
    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.Log("Init purchasing failed:" + error);
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Debug.Log("Init purchasing failed:" + error + message);

    }



    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.Log("Purchasing failed:" + failureReason);

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
                    Debug.Log("Receipt not found");
                }
            }
            else
            {
                Debug.Log("No product found");
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
                        Debug.Log("EXP DATE"+ info.getExpireDate());

                        if (info.isSubscribed() == Result.True)
                        {
                            Debug.Log("Subscribed");
                        }
                        else
                        {
                            Debug.Log(" Not Subscribed");

                        }
                    }
                    else
                    {
                        Debug.Log("Receipt not found");
                    }
                }  catch(System.Exception e)
                {
                    Debug.Log("Only work in GG, Appstore, amazone store");
                }
            }
            else
            {
                Debug.Log("No product found");
            }
        }
    }
    private void RemoveAds()
    {
        Debug.Log("Remove ads");
    }
}