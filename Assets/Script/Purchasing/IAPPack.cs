using System;
using Script.Controller;
using Script.Resources;
using UnityEngine;

namespace Script.Purchasing
{
    public class IAPPack : MonoBehaviour
    {
        [SerializeField] private string name; // Tên gói
        [SerializeField] private string type; // Loại gói (Gold hoặc Gem)
        [SerializeField] private int rewardGem; // Số lượng Gem thưởng
        [SerializeField] private int rewardCoin; // Số lượng Gold thưởng
        [SerializeField] private int rewardHelper; // k biết 
        [SerializeField] private float price; // Giá gói 

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