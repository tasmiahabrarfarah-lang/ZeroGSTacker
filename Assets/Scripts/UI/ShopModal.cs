using UnityEngine;
using UnityEngine.UI;
using ZeroGStacker.Core;
using ZeroGStacker.Gameplay;
using ZeroGStacker.Monetization;

namespace ZeroGStacker.UI
{
    public class ShopModal : MonoBehaviour
    {
        [Header("IAP Buttons")]
        [SerializeField] private Button removeAdsButton;
        [SerializeField] private Button stardustSmallButton;
        [SerializeField] private Button stardustMediumButton;
        [SerializeField] private Button stardustLargeButton;
        [SerializeField] private Button seasonPassButton;

        [Header("Status")]
        [SerializeField] private Text removeAdsStatusText;
        [SerializeField] private Text seasonPassStatusText;
        [SerializeField] private Button closeButton;

        private void Awake()
        {
            if (removeAdsButton != null) removeAdsButton.onClick.AddListener(OnRemoveAdsClicked);
            if (stardustSmallButton != null) stardustSmallButton.onClick.AddListener(() => OnBuyStardust(500, IAPManager.ProductIdStardustSmall));
            if (stardustMediumButton != null) stardustMediumButton.onClick.AddListener(() => OnBuyStardust(1500, IAPManager.ProductIdStardustMedium));
            if (stardustLargeButton != null) stardustLargeButton.onClick.AddListener(() => OnBuyStardust(5000, IAPManager.ProductIdStardustLarge));
            if (seasonPassButton != null) seasonPassButton.onClick.AddListener(OnSeasonPassClicked);
            if (closeButton != null) closeButton.onClick.AddListener(OnCloseClicked);
        }

        public void RefreshView(PlayerData player)
        {
            if (removeAdsButton != null)
            {
                removeAdsButton.interactable = !player.noAdsPurchased;
                if (removeAdsStatusText != null)
                {
                    removeAdsStatusText.text = player.noAdsPurchased ? "PURCHASED" : "$2.99";
                }
            }

            if (seasonPassButton != null)
            {
                seasonPassButton.interactable = !player.seasonPassPurchased;
                if (seasonPassStatusText != null)
                {
                    seasonPassStatusText.text = player.seasonPassPurchased ? "ACTIVATED" : "$4.99";
                }
            }
        }

        private void OnRemoveAdsClicked()
        {
            if (GameManager.Instance != null && IAPManager.Instance != null)
            {
                IAPManager.Instance.BuyRemoveAds(GameManager.Instance.Player);
                RefreshView(GameManager.Instance.Player);
            }
        }

        private void OnBuyStardust(int amount, string productId)
        {
            if (GameManager.Instance != null && IAPManager.Instance != null)
            {
                IAPManager.Instance.BuyStardustBundle(GameManager.Instance.Player, amount, productId);
                RefreshView(GameManager.Instance.Player);
            }
        }

        private void OnSeasonPassClicked()
        {
            if (GameManager.Instance != null && IAPManager.Instance != null)
            {
                IAPManager.Instance.BuySeasonPass(GameManager.Instance.Player);
                RefreshView(GameManager.Instance.Player);
            }
        }

        private void OnCloseClicked()
        {
            gameObject.SetActive(false);
        }
    }
}
