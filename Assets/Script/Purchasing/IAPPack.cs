using System;
using System.Collections.Generic;
using Script.Controller;
using Script.Resources;
using UnityEngine;
using UnityEngine.Rendering;

namespace Script.Purchasing
{
    public interface IBuyableItem
    {
        int Id { get; }
        string Name { get; }
        string Description { get; }
        int Price { get; }
    }
    [Serializable]
    public class ConsumableItem : IBuyableItem
    {
        public int Id { get; private set; }

        public string Name { get; private set; }

        public string Description { get; private set; }

        public int Price { get; private set; }

        public List<Resource> ResourceGain { get; private set; }

        public int AmountGain { get; private set; }

    } 
    [Serializable]
    public class NonConsumableItem : IBuyableItem
    {
        public int Id { get; private set; }

        public string Name { get; private set; }

        public string Description { get; private set; }

        public int Price { get; private set; }
    } 
    [Serializable]
    public class SubcriptionItem : IBuyableItem
    {
        public int Id { get; private set; }

        public string Name { get; private set; }

        public string Description { get; private set; }

        public int Price { get; private set; }

        public int TimeDuration { get; private set; } // inday
    }
    public class IAPShop : MonoBehaviour
    {
        [SerializeField] List<ConsumableItem> consumableItems;
        [SerializeField] List<NonConsumableItem> nonConsumableItems;
        [SerializeField] List<SubcriptionItem> subcriptionItems;
    }
    public class IAPPack
    {
        [SerializeField] private string name;
        [SerializeField] private string type;
        [SerializeField] private int rewardGem; 
        [SerializeField] private int rewardCoin;
        [SerializeField] private int rewardHelper;
        [SerializeField] private float price;

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(name) &&
                   (type == "Gold" || type == "Gem") &&
                   price >= 0 &&
                   (rewardGem > 0 || rewardCoin > 0 || rewardHelper > 0);
        }

        public void ApplyPackage()
        {
            if (!IsValid())
            {
                Debug.LogError($"Invalid package: {name}");

            }
            else
            {
                bool success = true;

                if (rewardCoin > 0 && type == "Gold")
                {
                    if (GameController.Instance.ResourceController.TryGetAmount(Resource.Gold, out var currentGold))
                    {
                        long newGold = currentGold + rewardCoin;
                        success &= GameController.Instance.ResourceController.TrySetAmount(Resource.Gold, newGold);
                    }
                    else
                    {
                        Debug.LogError($"Resource type Gold not found in ResourceController.");
                        success = false;
                    }
                }

                if (rewardGem > 0 && type == "Gem")
                {
                    if (GameController.Instance.ResourceController.TryGetAmount(Resource.Gem, out var currentGem))
                    {
                        long newGem = currentGem + rewardGem;
                        success &= GameController.Instance.ResourceController.TrySetAmount(Resource.Gem, newGem);
                    }
                    else
                    {
                        Debug.LogError($"Resource type Gem not found in ResourceController.");
                        success = false;
                    }
                }

                if (rewardHelper > 0)
                {
                    Debug.Log($"Added {rewardHelper} Helpers (logic not implemented).");
                }

                if (success)
                {
                    Debug.Log($"Applied package {name}: Gold: {rewardCoin}, Gem: {rewardGem}, Helper: {rewardHelper}");
                }
                else
                {
                    Debug.LogError($"Failed to apply package {name} to ResourceController.");
                }

            }


        }
        public string GetPackageInfo()
        {
            return $"Package: {name} | Type: {type} | Price: ${price} | Gold: {rewardCoin} | Gem: {rewardGem} | Helper: {rewardHelper}";
        }
        [Serializable]
        public class InAppPurchaseRecord
        {
            [SerializeField] private string purchaseId;
            [SerializeField] private string packageName;
            [SerializeField] private DateTime purchaseTime;
            [SerializeField] private string userId;

            // Constructor
            public InAppPurchaseRecord(string purchaseId, string packageName, string userId)
            {
                this.purchaseId = purchaseId;
                this.packageName = packageName;
                this.userId = userId;
                this.purchaseTime = DateTime.UtcNow;
            }

            // Properties
            public string PurchaseId => purchaseId;
            public string PackageName => packageName;
            public DateTime PurchaseTime => purchaseTime;
            public string UserId => userId;

            public string GetRecordInfo()
            {
                return $"Purchase ID: {purchaseId} | Package: {packageName} | User: {userId} | Time: {purchaseTime}";
            }
        }
    }
}