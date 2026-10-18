using System;
using UnityEngine;
using ZeroGStacker.Core;

namespace ZeroGStacker.Backend
{
    public class StreakCalendarService : MonoBehaviour
    {
        private static StreakCalendarService instance;
        public static StreakCalendarService Instance => instance;

        private const string PrefKeyLastLogin = "ZGS_LastLoginUtc";
        private const string PrefKeyStreak = "ZGS_StreakCount";
        private const string PrefKeyStreakFreeze = "ZGS_StreakFreezes";

        private readonly int[] streakRewards = new int[] { 50, 75, 100, 150, 200, 300, 500 };

        public event Action<int, int, bool> OnDailyRewardClaimed; // streakDay, stardustReward, freezeUsed

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
        }

        public void CheckDailyStreak(PlayerData playerData)
        {
            string lastDateStr = PlayerPrefs.GetString(PrefKeyLastLogin, "");
            string todayStr = DateTime.UtcNow.ToString("yyyy-MM-dd");

            if (lastDateStr == todayStr)
            {
                // Already logged in today
                return;
            }

            int currentStreak = PlayerPrefs.GetInt(PrefKeyStreak, 1);
            int freezes = PlayerPrefs.GetInt(PrefKeyStreakFreeze, playerData.streakFreezes);
            bool freezeUsed = false;

            if (!string.IsNullOrEmpty(lastDateStr))
            {
                if (DateTime.TryParse(lastDateStr, out DateTime lastDate))
                {
                    TimeSpan diff = DateTime.UtcNow.Date - lastDate.Date;
                    if (diff.Days == 1)
                    {
                        // Consecutive day
                        currentStreak++;
                    }
                    else if (diff.Days > 1)
                    {
                        // Missed days!
                        if (freezes > 0)
                        {
                            freezes--;
                            freezeUsed = true;
                            PlayerPrefs.SetInt(PrefKeyStreakFreeze, freezes);
                            playerData.streakFreezes = freezes;
                            // Streak preserved!
                        }
                        else
                        {
                            currentStreak = 1;
                        }
                    }
                }
            }

            PlayerPrefs.SetString(PrefKeyLastLogin, todayStr);
            PlayerPrefs.SetInt(PrefKeyStreak, currentStreak);
            PlayerPrefs.Save();

            playerData.streak = currentStreak;

            // Grant escalating reward
            int rewardIdx = Mathf.Clamp((currentStreak - 1) % 7, 0, streakRewards.Length - 1);
            int stardustReward = streakRewards[rewardIdx];
            playerData.stardust += stardustReward;

            OnDailyRewardClaimed?.Invoke(currentStreak, stardustReward, freezeUsed);
        }

        public int GetRewardForDay(int day)
        {
            int idx = Mathf.Clamp((day - 1) % 7, 0, streakRewards.Length - 1);
            return streakRewards[idx];
        }
    }
}
