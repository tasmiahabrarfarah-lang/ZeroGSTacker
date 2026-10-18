using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZeroGStacker.Backend;

namespace ZeroGStacker.UI
{
    public class LeaderboardView : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Text titleText;
        [SerializeField] private Text listContentText;
        [SerializeField] private Button tabGlobalButton;
        [SerializeField] private Button tabDailyButton;
        [SerializeField] private Button closeButton;

        private bool showingDaily = false;

        private void Awake()
        {
            if (tabGlobalButton != null) tabGlobalButton.onClick.AddListener(() => SwitchTab(false));
            if (tabDailyButton != null) tabDailyButton.onClick.AddListener(() => SwitchTab(true));
            if (closeButton != null) closeButton.onClick.AddListener(OnCloseClicked);
        }

        private void OnEnable()
        {
            SwitchTab(showingDaily);
        }

        public async void SwitchTab(bool isDaily)
        {
            showingDaily = isDaily;
            if (titleText != null)
            {
                titleText.text = isDaily ? "DAILY SEEDED LEADERBOARD" : "GLOBAL ALL-TIME HEIGHT";
            }

            if (listContentText != null)
            {
                listContentText.text = "Loading cosmic records...";
            }

            string lbId = isDaily && DailyChallengeService.Instance != null
                ? DailyChallengeService.Instance.GetDailyLeaderboardId()
                : "global_height";

            if (FirebaseManager.Instance != null)
            {
                var entries = await FirebaseManager.Instance.GetTopScoresAsync(lbId, 10);
                DisplayEntries(entries);
            }
        }

        private void DisplayEntries(List<LeaderboardEntry> entries)
        {
            if (listContentText == null) return;

            if (entries == null || entries.Count == 0)
            {
                listContentText.text = "No records yet. Be the first to reach the stars!";
                return;
            }

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < entries.Count; i++)
            {
                var e = entries[i];
                sb.AppendLine($"#{e.rank}  {e.displayName,-15}  {e.height:F1}m  ({e.score} pts)");
            }
            listContentText.text = sb.ToString();
        }

        private void OnCloseClicked()
        {
            gameObject.SetActive(false);
        }
    }
}
