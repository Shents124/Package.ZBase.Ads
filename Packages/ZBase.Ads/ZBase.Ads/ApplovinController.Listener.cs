using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ZBase.Ads
{
    public partial class ApplovinController
    {
        private string _bannerAdUnitId;
        private string _interstitialAdUnitId;
        private string _rewardedAdUnitId;

        private int _retryAttemptInterstitialAd;
        private int _retryAttemptRewardAd;
        private Action _onInitialized;
        private Action<bool> _onShowRewardedCallback;

        private void InitializeEvent(string rewardedAdUnitId, string bannerAdUnitId, string interstitialAdUnitId,
            Action onInitialized)
        {
            _bannerAdUnitId = bannerAdUnitId;
            _interstitialAdUnitId = interstitialAdUnitId;
            _rewardedAdUnitId = rewardedAdUnitId;
            _onInitialized = onInitialized;
            MaxSdkCallbacks.OnSdkInitializedEvent += OnSdkInitializedEvent;
            InitializeRewardedAds();
            InitializeInterstitialAds();
            InitializeBannerAds();
        }

        private void InitializeRewardedAds()
        {
            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnRewardedAdLoadedEvent;
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardedAdLoadFailedEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += OnRewardedAdDisplayedEvent;
            MaxSdkCallbacks.Rewarded.OnAdClickedEvent += OnRewardedAdClickedEvent;
            MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnRewardedAdRevenuePaidEvent;
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardedAdHiddenEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnRewardedAdFailedToDisplayEvent;
            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnRewardedAdReceivedRewardEvent;
        }

        private void InitializeInterstitialAds()
        {
            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterstitialLoadedEvent;
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnInterstitialLoadFailedEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += OnInterstitialDisplayedEvent;
            MaxSdkCallbacks.Interstitial.OnAdClickedEvent += OnInterstitialClickedEvent;
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnInterstitialHiddenEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += OnInterstitialAdFailedToDisplayEvent;
            MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += OnInterstitialAdRevenuePaidEvent;
        }

        private void InitializeBannerAds()
        {
            MaxSdkCallbacks.Banner.OnAdLoadedEvent += OnBannerAdLoadedEvent;
            MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += OnBannerAdLoadFailedEvent;
            MaxSdkCallbacks.Banner.OnAdClickedEvent += OnBannerAdClickedEvent;
            MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnBannerAdRevenuePaidEvent;
            MaxSdkCallbacks.Banner.OnAdExpandedEvent += OnBannerAdExpandedEvent;
            MaxSdkCallbacks.Banner.OnAdCollapsedEvent += OnBannerAdCollapsedEvent;
        }

        private void OnSdkInitializedEvent(MaxSdkBase.SdkConfiguration sdk)
        {
            _onInitialized?.Invoke();
        }

        public void LoadAds(MaxSdk.AdViewPosition bannerAdPosition, Color bannerColor)
        {
            LoadInterstitial();
            LoadBanner(bannerAdPosition, bannerColor);
            LoadRewardedAd();
        }
        
        #region Reward

        private void LoadRewardedAd()
        {
            MaxSdk.LoadRewardedAd(_rewardedAdUnitId);
        }

        private void InvokeShowRewardedCallback(bool isSuccess)
        {
            _onShowRewardedCallback?.Invoke(isSuccess);
            _onShowRewardedCallback = null;
        }

        private void OnRewardedAdLoadedEvent(string adUnitId, MaxSdk.AdInfo adInfo)
        {
            // Rewarded ad is ready for you to show. MaxSdk.IsRewardedAdReady(adUnitId) now returns 'true'.

            // Reset retry attempt
            _retryAttemptRewardAd = 0;
        }

        private void OnRewardedAdLoadFailedEvent(string adUnitId, MaxSdk.ErrorInfo errorInfo)
        {
            // Rewarded ad failed to load
            // AppLovin recommends that you retry with exponentially higher delays, up to a maximum delay (in this case 64 seconds).
            _retryAttemptRewardAd++;
            double retryDelay = Math.Pow(2, Math.Min(6, _retryAttemptRewardAd));

            TryLoadRewardAd((float)retryDelay).Forget();
        }

        private async UniTask TryLoadRewardAd(float delay)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(delay));
            LoadRewardedAd();
        }

        private void OnRewardedAdDisplayedEvent(string adUnitId, MaxSdk.AdInfo adInfo)
        {
            
        }

        private void OnRewardedAdFailedToDisplayEvent(string adUnitId, MaxSdk.ErrorInfo errorInfo, MaxSdk.AdInfo adInfo)
        {
            InvokeShowRewardedCallback(false);
            // Rewarded ad failed to display. AppLovin recommends that you load the next ad.
            LoadRewardedAd();
        }

        private void OnRewardedAdClickedEvent(string adUnitId, MaxSdk.AdInfo adInfo) { }

        private void OnRewardedAdHiddenEvent(string adUnitId, MaxSdk.AdInfo adInfo)
        {
            // Rewarded ad is hidden. Pre-load the next ad
            LoadRewardedAd();
        }

        private void OnRewardedAdReceivedRewardEvent(string adUnitId, MaxSdk.Reward reward, MaxSdk.AdInfo adInfo)
        {
            // The rewarded ad displayed and the user should receive the reward.
            InvokeShowRewardedCallback(true);
        }

        private void OnRewardedAdRevenuePaidEvent(string adUnitId, MaxSdk.AdInfo adInfo)
        {
            // Ad revenue paid. Use this callback to track user revenue.
            // LogHelper.Log("Rewarded ad revenue paid");
            // PlayerService.TrackingService.IncreaseRewarded();
            // FirebaseTracker.LogAdsImpression(adInfo);
            // Singleton.Of<AppsFlyerService>().LogEventAdComplete(AdType.rewarded, adInfo);
        }

        #endregion

        #region Interstitial

        private void LoadInterstitial()
        {
            MaxSdk.LoadInterstitial(_interstitialAdUnitId);
        }

        private void OnInterstitialLoadedEvent(string adUnitId, MaxSdk.AdInfo adInfo)
        {
            // Interstitial ad is ready for you to show. MaxSdk.IsInterstitialReady(adUnitId) now returns 'true'

            // Reset retry attempt
            _retryAttemptInterstitialAd = 0;
        }

        private void OnInterstitialLoadFailedEvent(string adUnitId, MaxSdk.ErrorInfo errorInfo)
        {
            // Interstitial ad failed to load
            // AppLovin recommends that you retry with exponentially higher delays, up to a maximum delay (in this case 64 seconds)

            _retryAttemptInterstitialAd++;
            double retryDelay = Math.Pow(2, Math.Min(6, _retryAttemptInterstitialAd));

            TryLoadInterstitial((float)retryDelay).Forget();
        }

        private async UniTask TryLoadInterstitial(float interstitialDelay)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(interstitialDelay));
            LoadInterstitial();
        }

        private void OnInterstitialDisplayedEvent(string adUnitId, MaxSdk.AdInfo adInfo)
        {
            
        }

        private void OnInterstitialAdFailedToDisplayEvent(string adUnitId, MaxSdk.ErrorInfo errorInfo,
            MaxSdk.AdInfo adInfo)
        {
            // Interstitial ad failed to display. AppLovin recommends that you load the next ad.
            LoadInterstitial();
        }

        private void OnInterstitialClickedEvent(string adUnitId, MaxSdk.AdInfo adInfo)
        {
            
        }

        private void OnInterstitialHiddenEvent(string adUnitId, MaxSdk.AdInfo adInfo)
        {
            // Interstitial ad is hidden. Pre-load the next ad.
            LoadInterstitial();
        }

        private void OnInterstitialAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            // PlayerService.TrackingService.IncreaseInter();
            // FirebaseTracker.LogAdsImpression(adInfo);
            // Singleton.Of<AppsFlyerService>().LogEventAdComplete(AdType.inter, adInfo);
            // LogHelper.Log("OnInterstitialAdRevenuePaidEvent");
        }

        #endregion

        #region Banner
        
#if !UNITY_EDITOR
         public Rect GetRectBanner()
        {
            return MaxSdk.GetBannerLayout(_bannerAdUnitId);
        }
#endif
        
        private void LoadBanner(MaxSdk.AdViewPosition adViewPosition, Color bannerColor)
        {
            // Banners are automatically sized to 320×50 on phones and 728×90 on tablets
            // You may call the utility method MaxSdkUtils.isTablet() to help with view sizing adjustments
            var adViewConfiguration = new MaxSdk.AdViewConfiguration(adViewPosition);
            MaxSdk.CreateBanner(_bannerAdUnitId, adViewConfiguration);

            // Set background color for banners to be fully functional
            MaxSdk.SetBannerBackgroundColor(_bannerAdUnitId, bannerColor);
        }

        private void OnBannerAdLoadedEvent(string adUnitId, MaxSdk.AdInfo adInfo)
        {
            IsBannerLoaded = true;
        }
        
        private void OnBannerAdLoadFailedEvent(string adUnitId, MaxSdk.ErrorInfo errorInfo)
        {
            
        }

        private void OnBannerAdClickedEvent(string adUnitId, MaxSdk.AdInfo adInfo) { }

        private void OnBannerAdRevenuePaidEvent(string adUnitId, MaxSdk.AdInfo adInfo)
        {
            // FirebaseTracker.LogAdsImpression(adInfo);
            // Singleton.Of<AppsFlyerService>().LogEventAdComplete(AdType.banner, adInfo);
        }

        private void OnBannerAdExpandedEvent(string adUnitId, MaxSdk.AdInfo adInfo)
        {
            
        }

        private void OnBannerAdCollapsedEvent(string adUnitId, MaxSdk.AdInfo adInfo) { }

        #endregion
    }
}