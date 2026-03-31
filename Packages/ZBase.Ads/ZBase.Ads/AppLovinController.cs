using System;
using UnityEngine.Device;

namespace ZBase.Ads
{
    public partial class ApplovinController
    {
        public bool IsShowingBanner { get; private set; }
        
        public bool IsBannerLoaded { get; private set; }

        public void Initialize(string rewardedId, string bannerId, string interstitialId, Action onFinish)
        {
            InitializeEvent(rewardedId, bannerId, interstitialId, onFinish);
            SetAdsPolicy();
#if BUILD_PRODUCTION
            MaxSdk.SetVerboseLogging(false);
            MaxSdk.SetExtraParameter("disable_all_logs", "true");
#else
            MaxSdk.SetVerboseLogging(true);
#endif
            
            MaxSdk.InitializeSdk();
        }
        
        private void SetAdsPolicy()
        {
            // https://dash.applovin.com/documentation/mediation/unity/getting-started/privacy
            MaxSdk.SetHasUserConsent(true);
            MaxSdk.SetDoNotSell(false);
        }
        
        #region Rewarded Ads

        public void ShowRewardedAd(string adPlacement, Action<bool> callback)
        {
            if (Application.isEditor)
            {
                callback?.Invoke(true);
                return;
            }

            _onShowRewardedCallback = callback;
            if (MaxSdk.IsRewardedAdReady(_rewardedAdUnitId))
            {
                HideBanner();
                MaxSdk.ShowRewardedAd(_rewardedAdUnitId, adPlacement);
            }
            else
            {
                InvokeShowRewardedCallback(false);
            }
        }

        #endregion

        #region Interstitial Ads
        public bool ShowInterstitialAd(string adPlacement, string source = null)
        {
            if (MaxSdk.IsInterstitialReady(_interstitialAdUnitId))
            {
                HideBanner();
                MaxSdk.ShowInterstitial(_interstitialAdUnitId, adPlacement);
                return true;
            }
            return false;
        }

        #endregion

        #region Banner Ads

        public void ShowBannerAds()
        {
            if (IsBannerLoaded == false)
                return;
            
            if (IsShowingBanner)
                return;
            
            MaxSdk.ShowBanner(_bannerAdUnitId);
            IsShowingBanner = true;
        }

        public void HideBanner()
        {
            if (!IsShowingBanner)
                return;

            MaxSdk.HideBanner(_bannerAdUnitId);
            IsShowingBanner = false;
        }

        #endregion
    }
}