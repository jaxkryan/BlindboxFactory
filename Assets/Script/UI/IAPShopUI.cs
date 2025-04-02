using Script.Resources;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IAPShopUI : MonoBehaviour
{
    [SerializeField] private IAPShop iapShop;
    [SerializeField] private GameObject consumableItemUIPrefab;
    [SerializeField] private GameObject nonConsumableItemUIPrefab;
    [SerializeField] private GameObject subscriptionItemUIPrefab;
    [SerializeField] private Transform consumableContent; // Content của ConsumableScrollView
    [SerializeField] private Transform nonConsumableContent; // Content của NonConsumableScrollView
    [SerializeField] private Transform subscriptionContent; // Content của SubscriptionScrollView

    void Start()
    {
        PopulateShopUI();
    }

    void PopulateShopUI()
    {
        // Xóa các item cũ trong mỗi Content
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

        // Hiển thị ConsumableItems
        foreach (var item in iapShop.consumableItems)
        {
            GameObject uiItem = Instantiate(consumableItemUIPrefab, consumableContent);
            SetupConsumableItemUI(uiItem, item);
        }

        // Hiển thị NonConsumableItems
        foreach (var item in iapShop.nonConsumableItems)
        {
            GameObject uiItem = Instantiate(nonConsumableItemUIPrefab, nonConsumableContent);
            SetupNonConsumableItemUI(uiItem, item);
        }

        // Hiển thị SubscriptionItems
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
        Debug.Log($"Bought Consumable: {item.name} for ${item.price}");
        // Thêm logic mua ở đây
    }

    void BuyNonConsumableItem(NonConsumableItem item)
    {
        Debug.Log($"Bought Non-Consumable: {item.name} for ${item.price}");
        // Thêm logic mua ở đây
    }

    void BuySubscriptionItem(SubscriptionItem item)
    {
        Debug.Log($"Bought Subscription: {item.name} for ${item.price}");
        // Thêm logic mua ở đây
    }
}