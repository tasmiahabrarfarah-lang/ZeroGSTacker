using UnityEngine;
using UnityEngine.UI;
using ZeroGStacker.Backend;
using ZeroGStacker.Core;
using ZeroGStacker.Gameplay;

namespace ZeroGStacker.UI
{
    public class UIManager : MonoBehaviour
    {
        private static UIManager instance;
        public static UIManager Instance => instance;

        [Header("Screens")]
        [SerializeField] private GameObject mainMenuScreen;
        [SerializeField] private GameObject hudScreen;
        [SerializeField] private GameObject gameOverScreen;
        [SerializeField] private GameObject shopModal;
        [SerializeField] private GameObject dailyChestModal;
        [SerializeField] private GameObject leaderboardModal;

        [Header("Views")]
        [SerializeField] private HUDView hudView;
        [SerializeField] private GameOverView gameOverView;
        [SerializeField] private ShopModal shopView;

        [Header("Main Menu Controls")]
        [SerializeField] private Button playEndlessButton;
        [SerializeField] private Button playDailyButton;
        [SerializeField] private Button openShopButton;
        [SerializeField] private Button openChestButton;
        [SerializeField] private Button openLeaderboardButton;
        [SerializeField] private Text menuStardustText;
        [SerializeField] private Text menuStreakText;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;

            WireButtons();
        }

        private void WireButtons()
        {
            if (playEndlessButton != null)
            {
                playEndlessButton.onClick.AddListener(() =>
                {
                    Debug.Log("[UI] 'ENDLESS RUN' clicked! Starting new run...");
                    GameManager.Instance?.StartNewRun(false);
                });
            }
            if (playDailyButton != null)
            {
                playDailyButton.onClick.AddListener(() =>
                {
                    Debug.Log("[UI] 'DAILY SEEDED CHALLENGE' clicked! Starting daily challenge...");
                    GameManager.Instance?.StartNewRun(true);
                });
            }
            if (openShopButton != null) openShopButton.onClick.AddListener(() =>
            {
                Debug.Log("[UI] 'COSMETICS & IAP' clicked!");
                ShowShop();
            });
            if (openChestButton != null) openChestButton.onClick.AddListener(() =>
            {
                Debug.Log("[UI] 'DAILY CHEST' clicked!");
                ShowDailyChest();
            });
            if (openLeaderboardButton != null) openLeaderboardButton.onClick.AddListener(() =>
            {
                Debug.Log("[UI] 'LEADERBOARDS' clicked!");
                ShowLeaderboard();
            });
        }

        private void Update()
        {
            if (hudScreen != null && hudScreen.activeSelf && hudView != null && GameManager.Instance != null)
            {
                hudView.UpdateHUD(GameManager.Instance.Run, GameManager.Instance.Player, GameManager.Instance.Tower);
            }
        }

        public void ShowMainMenu()
        {
            if (mainMenuScreen != null) mainMenuScreen.SetActive(true);
            if (hudScreen != null) hudScreen.SetActive(false);
            if (gameOverScreen != null) gameOverScreen.SetActive(false);
            if (shopModal != null) shopModal.SetActive(false);
            if (dailyChestModal != null) dailyChestModal.SetActive(false);
            if (leaderboardModal != null) leaderboardModal.SetActive(false);

            if (GameManager.Instance != null)
            {
                var p = GameManager.Instance.Player;
                if (menuStardustText != null) menuStardustText.text = $"★ {p.stardust}";
                if (menuStreakText != null) menuStreakText.text = $"🔥 Day {p.streak}";
            }
        }

        public void ShowHUD()
        {
            if (mainMenuScreen != null) mainMenuScreen.SetActive(false);
            if (hudScreen != null) hudScreen.SetActive(true);
            if (gameOverScreen != null) gameOverScreen.SetActive(false);
            if (shopModal != null) shopModal.SetActive(false);
            if (dailyChestModal != null) dailyChestModal.SetActive(false);
            if (leaderboardModal != null) leaderboardModal.SetActive(false);
        }

        public void ShowGameOver(RunState run)
        {
            if (mainMenuScreen != null) mainMenuScreen.SetActive(false);
            if (hudScreen != null) hudScreen.SetActive(false);
            if (gameOverScreen != null)
            {
                gameOverScreen.SetActive(true);
                if (gameOverView != null) gameOverView.Setup(run);
            }
        }

        public void ShowShop()
        {
            if (shopModal != null)
            {
                shopModal.SetActive(true);
                if (shopView != null && GameManager.Instance != null)
                {
                    shopView.RefreshView(GameManager.Instance.Player);
                }
            }
        }

        public void ShowDailyChest()
        {
            if (dailyChestModal != null)
            {
                dailyChestModal.SetActive(true);
            }
        }

        public void ShowLeaderboard()
        {
            if (leaderboardModal != null)
            {
                leaderboardModal.SetActive(true);
            }
        }
    }
}
