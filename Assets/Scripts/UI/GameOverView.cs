using UnityEngine;
using UnityEngine.UI;
using ZeroGStacker.Core;
using ZeroGStacker.Gameplay;
using ZeroGStacker.Monetization;

namespace ZeroGStacker.UI
{
    public class GameOverView : MonoBehaviour
    {
        [Header("Text Displays")]
        [SerializeField] private Text finalHeightText;
        [SerializeField] private Text finalScoreText;
        [SerializeField] private Text stardustEarnedText;

        [Header("Rewarded Video Buttons")]
        [SerializeField] private Button reviveButton;
        [SerializeField] private Text reviveButtonText;
        [SerializeField] private Button doubleStardustButton;
        [SerializeField] private Text doubleStardustButtonText;

        [Header("Navigation Buttons")]
        [SerializeField] private Button restartButton;
        [SerializeField] private Button menuButton;

        private RunState currentRun;

        private void Awake()
        {
            if (reviveButton != null) reviveButton.onClick.AddListener(OnReviveClicked);
            if (doubleStardustButton != null) doubleStardustButton.onClick.AddListener(OnDoubleStardustClicked);
            if (restartButton != null) restartButton.onClick.AddListener(OnRestartClicked);
            if (menuButton != null) menuButton.onClick.AddListener(OnMenuClicked);
        }

        public void Setup(RunState run)
        {
            currentRun = run;

            if (finalHeightText != null) finalHeightText.text = $"{run.height:F1} m";
            if (finalScoreText != null) finalScoreText.text = run.score.ToString();
            if (stardustEarnedText != null) stardustEarnedText.text = $"+{run.stardustEarnedThisRun} Stardust";

            // Section 7 Ad Triggers:
            // 1) Revive with stack intact (once/run)
            // 2) After revive used: 2x Stardust for this run
            if (!run.reviveUsed)
            {
                if (reviveButton != null)
                {
                    reviveButton.gameObject.SetActive(true);
                    reviveButton.interactable = true;
                }
                if (doubleStardustButton != null) doubleStardustButton.gameObject.SetActive(false);
            }
            else
            {
                if (reviveButton != null) reviveButton.gameObject.SetActive(false);
                if (doubleStardustButton != null)
                {
                    doubleStardustButton.gameObject.SetActive(true);
                    doubleStardustButton.interactable = true;
                }
            }
        }

        private void OnReviveClicked()
        {
            AdMobMaxMediationManager.Instance?.ShowRewardedVideo("rewarded_revive", () =>
            {
                GameManager.Instance?.ReviveWithStack();
            });
        }

        private void OnDoubleStardustClicked()
        {
            AdMobMaxMediationManager.Instance?.ShowRewardedVideo("rewarded_double_stardust", () =>
            {
                GameManager.Instance?.ApplyDoubleStardustReward();
                if (doubleStardustButton != null) doubleStardustButton.interactable = false;
                if (stardustEarnedText != null && currentRun != null)
                {
                    stardustEarnedText.text = $"+{currentRun.stardustEarnedThisRun} Stardust (2X)";
                }
            });
        }

        private void OnRestartClicked()
        {
            // Trigger interstitial at natural break if eligible
            AdMobMaxMediationManager.Instance?.ShowInterstitial("interstitial_restart", () =>
            {
                GameManager.Instance?.StartNewRun(currentRun != null && currentRun.isDailyChallenge);
            });
        }

        private void OnMenuClicked()
        {
            AdMobMaxMediationManager.Instance?.ShowInterstitial("interstitial_menu", () =>
            {
                UIManager.Instance?.ShowMainMenu();
            });
        }
    }
}
