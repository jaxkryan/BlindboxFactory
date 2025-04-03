using Script.Resources;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Script.HumanResource.Administrator;
using Script.Controller;
using Script.Gacha.Base;

public class IAPShopUI : MonoBehaviour
{
    [SerializeField] private IAPShop iapShop;
    [SerializeField] private GameObject consumableItemUIPrefab;
    [SerializeField] private GameObject nonConsumableItemUIPrefab;
    [SerializeField] private GameObject subscriptionItemUIPrefab;
    [SerializeField] private Transform consumableContent;
    [SerializeField] private Transform nonConsumableContent;
    [SerializeField] private Transform subscriptionContent;
    [SerializeField] private AdministratorGacha administratorGacha;
    [SerializeField] private GachaRevealPanelUI revealPanelPrefab; // Thêm prefab để hiển thị animation

    void Start()
    {
        PopulateShopUI();
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
        uiItem.transform.Find("Buy_Btn").GetComponent<Button>().onClick.AddListener(() => BuyConsumableItem(item));
    }

    void SetupNonConsumableItemUI(GameObject uiItem, NonConsumableItem item)
    {
        uiItem.transform.Find("BannerImage").GetComponent<Image>().sprite = item.bannerImage;
        uiItem.transform.Find("NameText").GetComponent<TextMeshProUGUI>().text = item.name;
        uiItem.transform.Find("DescriptionText").GetComponent<TextMeshProUGUI>().text = item.description;
        uiItem.transform.Find("PriceText").GetComponent<TextMeshProUGUI>().text = $"${item.price}";
        uiItem.transform.Find("Buy_Btn").GetComponent<Button>().onClick.AddListener(() => BuyNonConsumableItem(item));
    }

    void SetupSubscriptionItemUI(GameObject uiItem, SubscriptionItem item)
    {
        uiItem.transform.Find("BannerImage").GetComponent<Image>().sprite = item.bannerImage;
        uiItem.transform.Find("NameText").GetComponent<TextMeshProUGUI>().text = item.name;
        uiItem.transform.Find("DescriptionText").GetComponent<TextMeshProUGUI>().text = item.description;
        uiItem.transform.Find("PriceText").GetComponent<TextMeshProUGUI>().text = $"${item.price}";
        uiItem.transform.Find("TimeDurationText").GetComponent<TextMeshProUGUI>().text = $"{item.timeDuration} days";
        uiItem.transform.Find("Buy_Btn").GetComponent<Button>().onClick.AddListener(() => BuySubscriptionItem(item));
    }

    void BuyConsumableItem(ConsumableItem item)
    {
        ResourceController resourceController = GameController.Instance.ResourceController;

        // Tăng resource dựa trên resourceGain và amountGain
        if (item.resourceGain != null && item.resourceGain.Count > 0)
        {
            foreach (Resource resource in item.resourceGain)
            {
                if (resourceController.TryGetAmount(resource, out long currentAmount))
                {
                    long newAmount = currentAmount + item.amountGain;
                    if (resourceController.TrySetAmount(resource, newAmount))
                    {
                        Debug.Log($"Increased {resource} by {item.amountGain}. New amount: {newAmount}");
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
                    GameController.Instance.MascotController.AddMascot(epicMascot);
                    ShowReveal(new List<Mascot> { epicMascot }); // Hiển thị animation
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
        Debug.Log($"Bought Non-Consumable: {item.name} for ${item.price}");
    }

    void BuySubscriptionItem(SubscriptionItem item)
    {
        Debug.Log($"Bought Subscription: {item.name} for ${item.price}");
    }

    private void ShowReveal(List<Mascot> mascots)
    {
        if (revealPanelPrefab == null)
        {
            Debug.LogWarning("Reveal panel prefab is not assigned in IAPShopUI!");
            return;
        }

        var revealPanel = Instantiate(revealPanelPrefab, transform.parent);
        revealPanel.RevealMascots(mascots, () =>
        {
        });
    }
}