using System;
using UnityEngine;
using ZeroGStacker.Backend;
using ZeroGStacker.Core;

namespace ZeroGStacker.Monetization
{
    public class AdMobMaxMediationManager : MonoBehaviour, IAdManager
    {
        private static AdMobMaxMediationManager instance;
        public static AdMobMaxMediationManager Instance => instance;

        [Header("Ad Unit IDs (Test IDs)")]
        [SerializeField] private string adMobRewardedUnitId = "ca-app-pub-3940256099942544/5224354917";
        [SerializeField] private string adMobInterstitialUnitId = "ca-app-pub-3940256099942544/1033173712";
        [SerializeField] private string maxRewardedUnitId = "MAX_REWARDED_TEST";
        [SerializeField] private string maxInterstitialUnitId = "MAX_INTERSTITIAL_TEST";

        [Header("Frequency Capping")]
        [SerializeField] private int runsBetweenInterstitials = 3;
        private int completedRunsSinceLastAd = 0;

        [Header("Runtime State")]
        [SerializeField] private bool isRewardedLoaded = true;
        [SerializeField] private bool isInterstitialLoaded = true;

        public bool IsRewardedAdReady => isRewardedLoaded;
        public bool IsInterstitialReady => isInterstitialLoaded;

        public event Action<string> OnAdRewardedSuccess;
        public event Action<string> OnAdShown;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);

            UpdateFrequencyCappingFromRemoteConfig();
        }

        private void Start()
        {
            InitializeMediationSDKs();
        }

        public void UpdateFrequencyCappingFromRemoteConfig()
        {
            if (FirebaseManager.Instance != null)
            {
                runsBetweenInterstitials = FirebaseManager.Instance.GetRemoteInt(GameConstants.KeyAdInterstitialInterval, 3);
            }
        }

        private void InitializeMediationSDKs()
        {
#if ADMOB_ENABLED
            // AdMob primary initialization
            GoogleMobileAds.Api.MobileAds.Initialize(initStatus => {
                Debug.Log("[AdMob] Initialized successfully");
                LoadAdMobRewarded();
                LoadAdMobInterstitial();
            });
#elif APPLOVIN_MAX_ENABLED
            // AppLovin MAX mediation initialization
            MaxSdkCallbacks.OnSdkInitializedEvent += (MaxSdkBase.SdkConfiguration sdkConfiguration) => {
                Debug.Log("[AppLovin MAX] Initialized successfully");
                LoadMaxRewarded();
                LoadMaxInterstitial();
            };
            MaxSdk.SetSdkKey("YOUR_MAX_SDK_KEY");
            MaxSdk.InitializeSdk();
#else
            // In-Editor / Dev Mock Mode
            Debug.Log("[Mediation] Running in Editor / Simulation Mode (AdMob primary + AppLovin MAX configured)");
            isRewardedLoaded = true;
            isInterstitialLoaded = true;
#endif
        }

        public bool CanShowInterstitial()
        {
            // Golden Rule: Never interrupt an active run.
            // Also check frequency capping (every 2-3 runs).
            UpdateFrequencyCappingFromRemoteConfig();
            return completedRunsSinceLastAd >= runsBetweenInterstitials;
        }

        public void RecordRunCompleted()
        {
            completedRunsSinceLastAd++;
        }

        public void ShowRewardedVideo(string placementTag, Action onRewarded, Action onSkippedOrFailed = null)
        {
            Debug.Log($"[Mediation] Request to show Rewarded Video: {placementTag}");

#if ADMOB_ENABLED || APPLOVIN_MAX_ENABLED
            // Production Ad Presentation Logic
            // If primary AdMob is ready, show AdMob; else fallback to AppLovin MAX
#else
            // Mock immediate reward presentation for testing
            Debug.Log($"[Mediation Simulated] Rewarded Video watched successfully: {placementTag}");
            onRewarded?.Invoke();
            OnAdRewardedSuccess?.Invoke(placementTag);
#endif
        }

        public void ShowInterstitial(string placementTag, Action onClosed = null)
        {
            if (!CanShowInterstitial())
            {
                Debug.Log($"[Mediation] Interstitial skipped due to frequency capping ({completedRunsSinceLastAd}/{runsBetweenInterstitials})");
                onClosed?.Invoke();
                return;
            }

            Debug.Log($"[Mediation] Presenting Interstitial at natural break: {placementTag}");
            completedRunsSinceLastAd = 0;

#if ADMOB_ENABLED || APPLOVIN_MAX_ENABLED
            // Production Interstitial show call
#else
            Debug.Log($"[Mediation Simulated] Interstitial completed: {placementTag}");
            OnAdShown?.Invoke(placementTag);
            onClosed?.Invoke();
#endif
        }
    }
}
