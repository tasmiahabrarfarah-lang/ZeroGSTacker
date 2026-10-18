using System;
using UnityEngine;
using ZeroGStacker.Core;

namespace ZeroGStacker.Monetization
{
    public class IAPManager : MonoBehaviour
    {
        private static IAPManager instance;
        public static IAPManager Instance => instance;

        public const string ProductIdRemoveAds = "com.antigravity.zerogstacker.removeads";
        public const string ProductIdStardustSmall = "com.antigravity.zerogstacker.stardust500";
        public const string ProductIdStardustMedium = "com.antigravity.zerogstacker.stardust1500";
        public const string ProductIdStardustLarge = "com.antigravity.zerogstacker.stardust5000";
        public const string ProductIdSeasonPass = "com.antigravity.zerogstacker.seasonpass";

        public event Action<string> OnPurchaseSuccess;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void BuyRemoveAds(PlayerData player)
        {
            Debug.Log("[IAP] Purchase initiated: Remove Ads ($2.99)");
            player.noAdsPurchased = true;
            PlayerPrefs.SetInt("ZGS_NoAds", 1);
            PlayerPrefs.Save();
            OnPurchaseSuccess?.Invoke(ProductIdRemoveAds);
        }

        public void BuyStardustBundle(PlayerData player, int amount, string productId)
        {
            Debug.Log($"[IAP] Purchase initiated: {productId} (+{amount} Stardust)");
            player.stardust += amount;
            PlayerPrefs.SetInt("ZGS_Stardust", player.stardust);
            PlayerPrefs.Save();
            OnPurchaseSuccess?.Invoke(productId);
        }

        public void BuySeasonPass(PlayerData player)
        {
            Debug.Log("[IAP] Purchase initiated: Cosmetic Season Pass");
            player.seasonPassPurchased = true;
            PlayerPrefs.SetInt("ZGS_SeasonPass", 1);
            PlayerPrefs.Save();
            OnPurchaseSuccess?.Invoke(ProductIdSeasonPass);
        }
    }
}
