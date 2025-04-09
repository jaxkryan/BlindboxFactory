using UnityEngine;
using System;
using Unity.Services.LevelPlay;
using TMPro;
public class LevelPlayAds : MonoBehaviour
{
   
    public void LoadInterstitialAd()
    {
       
    }
    public void ShowInterstitialAd()
    {
        //Show InterstitialAd, check if the ad is ready before showing
        if (IronSource.Agent.isInterstitialReady())
        {
            IronSource.Agent.showInterstitial();
        }
        else
        {
            Debug.Log("Ads not ready");
        }
    }

    void DestroyInterstitialAd()
    {
    }
    public void ShowRewardedAd()
    {
        if(IronSource.Agent.isRewardedVideoAvailable()){
            IronSource.Agent.showRewardedVideo();
        }
        else
        {
            Debug.Log("Reward Ads not ready");
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        IronSource.Agent.init("219b48145");
        IronSourceEvents.onSdkInitializationCompletedEvent += SdkInitializationCompletedEvent;

        //IronSource.Agent.setMetaData("is_test_suite", "enable");
        //IronSource.Agent.launchTestSuite();
        //IronSource.Agent.validateIntegration();
    }

    private void SdkInitializationCompletedEvent()
    {
    }

    private void OnEnable()
    {
        //IronSourceEvents.onSdkInitializationCompletedEvent += SdkInitializationCompletedEvent;
        //Add ImpressionSuccess Event
        IronSourceEvents.onImpressionDataReadyEvent += ImpressionDataReadyEvent;

        //Add AdInfo Rewarded Video Events
        IronSourceRewardedVideoEvents.onAdAvailableEvent += RewardedVideoOnAdAvailable;
        IronSourceRewardedVideoEvents.onAdUnavailableEvent += RewardedVideoOnAdUnavailable;
        IronSourceRewardedVideoEvents.onAdOpenedEvent += RewardedVideoOnAdOpenedEvent;
        IronSourceRewardedVideoEvents.onAdShowFailedEvent += RewardedVideoOnAdShowFailedEvent;
        IronSourceRewardedVideoEvents.onAdClickedEvent += RewardedVideoOnAdClickedEvent;
        IronSourceRewardedVideoEvents.onAdClosedEvent += RewardedVideoOnAdClosedEvent;
        IronSourceRewardedVideoEvents.onAdRewardedEvent += RewardedVideoOnAdRewardedEvent;

        IronSource.Agent.loadInterstitial();
        IronSourceInterstitialEvents.onAdReadyEvent += InterstitialOnAdReadyEvent;
        IronSourceInterstitialEvents.onAdLoadFailedEvent += InterstitialOnAdLoadFailed;
        IronSourceInterstitialEvents.onAdOpenedEvent += InterstitialOnAdOpenedEvent;
        IronSourceInterstitialEvents.onAdShowSucceededEvent += InterstitialOnAdShowSucceededEvent;
        IronSourceInterstitialEvents.onAdClickedEvent += InterstitialOnAdClickedEvent;
        IronSourceInterstitialEvents.onAdShowFailedEvent += InterstitialOnAdShowFailedEvent;
        IronSourceInterstitialEvents.onAdClosedEvent += InterstitialOnAdClosedEvent;

    }

    private void InterstitialOnAdClosedEvent(IronSourceAdInfo info)
    {
    }

    private void InterstitialOnAdShowFailedEvent(IronSourceError error, IronSourceAdInfo info)
    {
    }

    private void InterstitialOnAdClickedEvent(IronSourceAdInfo info)
    {
    }

    private void InterstitialOnAdShowSucceededEvent(IronSourceAdInfo info)
    {
    }

    private void InterstitialOnAdOpenedEvent(IronSourceAdInfo info)
    {
    }

    private void InterstitialOnAdLoadFailed(IronSourceError error)
    {
    }

    private void InterstitialOnAdReadyEvent(IronSourceAdInfo info)
    {
    }

    private void InterstitialAdReadyEvent(IronSourceAdInfo info)
    {
    }

    private void ImpressionDataReadyEvent(IronSourceImpressionData data)
    {
    }

    void OnApplicationPause(bool isPaused)
    {
        IronSource.Agent.onApplicationPause(isPaused);
    }

    /************* RewardedVideo AdInfo Delegates *************/
    // Indicates that there’s an available ad.
    // The adInfo object includes information about the ad that was loaded successfully
    // This replaces the RewardedVideoAvailabilityChangedEvent(true) event
    void RewardedVideoOnAdAvailable(IronSourceAdInfo adInfo)
    {
    }
    // Indicates that no ads are available to be displayed
    // This replaces the RewardedVideoAvailabilityChangedEvent(false) event
    void RewardedVideoOnAdUnavailable()
    {
    }
    // The Rewarded Video ad view has opened. Your activity will loose focus.
    void RewardedVideoOnAdOpenedEvent(IronSourceAdInfo adInfo)
    {
    }
    // The Rewarded Video ad view is about to be closed. Your activity will regain its focus.
    void RewardedVideoOnAdClosedEvent(IronSourceAdInfo adInfo)
    {
    }
    // The user completed to watch the video, and should be rewarded.
    // The placement parameter will include the reward data.
    // When using server-to-server callbacks, you may ignore this event and wait for the ironSource server callback.
    void RewardedVideoOnAdRewardedEvent(IronSourcePlacement placement, IronSourceAdInfo adInfo)
    {




        GameObject.Find("ScoreTxt").GetComponent<TextMeshProUGUI>().text = "Score 10000";

    }
    // The rewarded video ad was failed to show.
    void RewardedVideoOnAdShowFailedEvent(IronSourceError error, IronSourceAdInfo adInfo)
    {
    }
    // Invoked when the video ad was clicked.
    // This callback is not supported by all networks, and we recommend using it only if
    // it’s supported by all networks you included in your build.
    void RewardedVideoOnAdClickedEvent(IronSourcePlacement placement, IronSourceAdInfo adInfo)
    {
    }

    //reward callback

    //full size ads callback
}
