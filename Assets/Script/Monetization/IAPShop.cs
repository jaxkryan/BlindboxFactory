using Script.Resources;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ResourceGainData
{
    public Resource resourceGain;
    public int amountGain;
}

[Serializable]
public class ConsumableItem
{
    public string id;
    public string name;
    public string description;
    public int price;
    public List<ResourceGainData> resourceGainData;
    public bool rewardMascot;
    public Sprite bannerImage;
}

[Serializable]
public class NonConsumableItem
{
    public string id;
    public string name;
    public string description;
    public int price;
    public Sprite bannerImage;
}

[Serializable]
public class SubscriptionItem
{
    public string id;
    public string name;
    public string description;
    public int price;
    public int timeDuration; // in days
    public Sprite bannerImage;
}

public class IAPShop : MonoBehaviour
{
    public List<ConsumableItem> consumableItems;
    public List<NonConsumableItem> nonConsumableItems;
    public List<SubscriptionItem> subscriptionItems;
}