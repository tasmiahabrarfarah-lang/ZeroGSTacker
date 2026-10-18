using UnityEngine;
using UnityEngine.UI;
using ZeroGStacker.Core;
using ZeroGStacker.Gameplay;
using ZeroGStacker.Monetization;

namespace ZeroGStacker.UI
{
    public class DailyChestModal : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button openChestButton;
        [SerializeField] private Button closeButton;

        [Header("Feedback")]
        [SerializeField] private Text rewardDisplayText;

        private void Awake()
        {
            if (openChestButton != null) openChestButton.onClick.AddListener(OnOpenChestClicked);
            if (closeButton != null) closeButton.onClick.AddListener(OnCloseClicked);
        }

        private void OnOpenChestClicked()
        {
            AdMobMaxMediationManager.Instance?.ShowRewardedVideo("rewarded_daily_chest", () =>
            {
                int stardustReward = Random.Range(100, 300);
                string cosmeticMsg = "";

                if (GameManager.Instance != null)
                {
                    var player = GameManager.Instance.Player;
                    player.stardust += stardustReward;

                    // 35% chance to unlock a cosmetic skin
                    if (Random.value < 0.35f)
                    {
                        SkinID skinToUnlock = SkinID.CosmicHolo;
                        if (!player.unlockedSkins.Contains(skinToUnlock.ToString()))
                        {
                            player.unlockedSkins.Add(skinToUnlock.ToString());
                            cosmeticMsg = "\n+ Cosmetic Skin Unlocked: Cosmic Holo!";
                        }
                    }

                    GameManager.Instance.SavePlayerData();
                }

                if (rewardDisplayText != null)
                {
                    rewardDisplayText.text = $"Chest Opened!\n+{stardustReward} Stardust{cosmeticMsg}";
                }
                if (openChestButton != null) openChestButton.interactable = false;
            });
        }

        private void OnCloseClicked()
        {
            gameObject.SetActive(false);
        }
    }
}
