using System;

namespace ZeroGStacker.Monetization
{
    public interface IAdManager
    {
        bool IsRewardedAdReady { get; }
        bool IsInterstitialReady { get; }

        void ShowRewardedVideo(string placementTag, Action onRewarded, Action onSkippedOrFailed = null);
        void ShowInterstitial(string placementTag, Action onClosed = null);
        bool CanShowInterstitial();
        void RecordRunCompleted();
    }
}
