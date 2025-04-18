using UnityEngine;
using System;
using Unity.Services.LevelPlay;
using System.Collections.Generic;
using Script.Controller;
using Script.Resources;

public class LevelPlayAds : MonoBehaviour
{
    [Serializable]
    public struct Reward
    {
        public Resource resourceType;
        public long amount;
    }

    [SerializeField]
    private List<Reward> adRewards = new List<Reward>(); // Gems at index 0, Gold at index 1

    [SerializeField, Tooltip("Enable to simulate ad rewards in Editor without real ads")]
    private bool editorTestMode = false;

    private int _selectedRewardIndex = -1;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void ShowGemAd()
    {
        ShowRewardedAd(0); // Gems at index 0
    }

    public void ShowGoldAd()
    {
        ShowRewardedAd(1); // Gold at index 1
    }

    private void ShowRewardedAd(int rewardIndex)
    {
        if (rewardIndex < 0 || rewardIndex >= adRewards.Count)
        {
            Debug.LogWarning($"Invalid reward index: {rewardIndex}. Configure adRewards in Inspector.");
            return;
        }

        if (Application.isEditor && editorTestMode)
        {
            Debug.Log($"Editor test mode: Simulating ad for {adRewards[rewardIndex].resourceType}");
            _selectedRewardIndex = rewardIndex;
            RewardedVideoOnAdRewardedEvent(null, null); // Simulate reward
            return;
        }

        if (IronSource.Agent.isRewardedVideoAvailable())
        {
            _selectedRewardIndex = rewardIndex;
            IronSource.Agent.showRewardedVideo();
            Debug.Log($"Showing ad for reward: {adRewards[rewardIndex].resourceType}");
        }
        else
        {
            Debug.Log("Rewarded ad not ready.");
        }
    }

    [ContextMenu("Test Gem Reward")]
    private void TestGemReward()
    {
        ShowRewardedAd(0); // For Inspector testing
    }

    [ContextMenu("Test Gold Reward")]
    private void TestGoldReward()
    {
        ShowRewardedAd(1); // For Inspector testing
    }

    void Start()
    {
        IronSource.Agent.init("219b48145"); // Replace with your IronSource App Key
        IronSourceEvents.onSdkInitializationCompletedEvent += SdkInitializationCompletedEvent;

        // Enable test suite in Editor
        if (Application.isEditor)
        {
            IronSource.Agent.setMetaData("is_test_suite", "enable");
            IronSource.Agent.launchTestSuite();
            Debug.Log("IronSource test suite enabled for Editor.");
        }
    }

    private void SdkInitializationCompletedEvent()
    {
        Debug.Log("IronSource SDK initialized.");
    }

    private void OnEnable()
    {
        IronSourceRewardedVideoEvents.onAdAvailableEvent += RewardedVideoOnAdAvailable;
        IronSourceRewardedVideoEvents.onAdUnavailableEvent += RewardedVideoOnAdUnavailable;
        IronSourceRewardedVideoEvents.onAdOpenedEvent += RewardedVideoOnAdOpenedEvent;
        IronSourceRewardedVideoEvents.onAdShowFailedEvent += RewardedVideoOnAdShowFailedEvent;
        IronSourceRewardedVideoEvents.onAdClickedEvent += RewardedVideoOnAdClickedEvent;
        IronSourceRewardedVideoEvents.onAdClosedEvent += RewardedVideoOnAdClosedEvent;
        IronSourceRewardedVideoEvents.onAdRewardedEvent += RewardedVideoOnAdRewardedEvent;
    }

    private void OnDisable()
    {
        IronSourceRewardedVideoEvents.onAdAvailableEvent -= RewardedVideoOnAdAvailable;
        IronSourceRewardedVideoEvents.onAdUnavailableEvent -= RewardedVideoOnAdUnavailable;
        IronSourceRewardedVideoEvents.onAdOpenedEvent -= RewardedVideoOnAdOpenedEvent;
        IronSourceRewardedVideoEvents.onAdShowFailedEvent -= RewardedVideoOnAdShowFailedEvent;
        IronSourceRewardedVideoEvents.onAdClickedEvent -= RewardedVideoOnAdClickedEvent;
        IronSourceRewardedVideoEvents.onAdClosedEvent -= RewardedVideoOnAdClosedEvent;
        IronSourceRewardedVideoEvents.onAdRewardedEvent -= RewardedVideoOnAdRewardedEvent;
    }

    void OnApplicationPause(bool isPaused)
    {
        IronSource.Agent.onApplicationPause(isPaused);
    }

    /************* RewardedVideo AdInfo Delegates *************/
    void RewardedVideoOnAdAvailable(IronSourceAdInfo adInfo)
    {
        Debug.Log($"Rewarded ad available: {adInfo}");
    }

    void RewardedVideoOnAdUnavailable()
    {
        Debug.Log("Rewarded ad unavailable");
    }

    void RewardedVideoOnAdOpenedEvent(IronSourceAdInfo adInfo)
    {
        Debug.Log($"Rewarded ad opened: {adInfo}");
    }

    void RewardedVideoOnAdClosedEvent(IronSourceAdInfo adInfo)
    {
        Debug.Log($"Rewarded ad closed: {adInfo}");
    }

    void RewardedVideoOnAdRewardedEvent(IronSourcePlacement placement, IronSourceAdInfo adInfo)
    {
        ResourceController resourceController = GameController.Instance.ResourceController;
        if (resourceController == null)
        {
            Debug.LogError("ResourceController not found. Cannot award rewards.");
            _selectedRewardIndex = -1;
            return;
        }

        if (_selectedRewardIndex < 0 || _selectedRewardIndex >= adRewards.Count)
        {
            Debug.LogWarning($"Invalid reward index: {_selectedRewardIndex}.");
            _selectedRewardIndex = -1;
            return;
        }

        var reward = adRewards[_selectedRewardIndex];
        if (resourceController.TryGetData(reward.resourceType, out var resourceData, out long currentAmount))
        {
            // Update MaxAmount if necessary
            if (resourceData.MaxAmount < currentAmount + reward.amount)
            {
                resourceData.MaxAmount = currentAmount + reward.amount;
                if (!resourceController.TryUpdateData(reward.resourceType, resourceData))
                {
                    Debug.LogWarning($"Failed to update MaxAmount for {reward.resourceType}.");
                }
            }

            long newAmount = currentAmount + reward.amount;
            if (resourceController.TrySetAmount(reward.resourceType, newAmount))
            {
                Debug.Log($"Awarded {reward.amount} {reward.resourceType}. New amount: {newAmount}");
                resourceController.Save(GameController.Instance.SaveManager);
            }
            else
            {
                Debug.LogWarning($"Failed to award {reward.amount} {reward.resourceType}.");
            }
        }
        else
        {
            Debug.LogWarning($"Failed to get data for {reward.resourceType}.");
        }

        _selectedRewardIndex = -1;
    }

    void RewardedVideoOnAdShowFailedEvent(IronSourceError error, IronSourceAdInfo adInfo)
    {
        Debug.LogError($"Rewarded ad failed to show: {error}");
        _selectedRewardIndex = -1;
    }

    void RewardedVideoOnAdClickedEvent(IronSourcePlacement placement, IronSourceAdInfo adInfo)
    {
        Debug.Log($"Rewarded ad clicked: {adInfo}");
    }
}