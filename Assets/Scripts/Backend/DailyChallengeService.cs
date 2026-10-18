using System;
using UnityEngine;

namespace ZeroGStacker.Backend
{
    public class DailyChallengeService : MonoBehaviour
    {
        private static DailyChallengeService instance;
        public static DailyChallengeService Instance => instance;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
        }

        public string GetDailyLeaderboardId()
        {
            return "daily_" + DateTime.UtcNow.ToString("yyyyMMdd");
        }

        public int GetDailySeed()
        {
            string dateStr = DateTime.UtcNow.ToString("yyyy-MM-dd");
            int hash = 17;
            for (int i = 0; i < dateStr.Length; i++)
            {
                hash = hash * 31 + dateStr[i];
            }
            return Mathf.Abs(hash);
        }

        public TimeSpan GetTimeUntilUtcMidnight()
        {
            DateTime now = DateTime.UtcNow;
            DateTime midnight = now.Date.AddDays(1);
            return midnight - now;
        }
    }
}
