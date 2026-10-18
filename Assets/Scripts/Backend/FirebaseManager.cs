using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using ZeroGStacker.Core;

namespace ZeroGStacker.Backend
{
    public class FirebaseManager : MonoBehaviour, IFirebaseService
    {
        private static FirebaseManager instance;
        public static FirebaseManager Instance => instance;

        [Header("Status")]
        [SerializeField] private bool isInitialized = false;
        [SerializeField] private string currentUserId = "";

        private Dictionary<string, string> localRemoteConfigCache = new Dictionary<string, string>();
        private Dictionary<string, List<LeaderboardEntry>> cachedLeaderboards = new Dictionary<string, List<LeaderboardEntry>>();

        public bool IsInitialized => isInitialized;
        public string UserId => currentUserId;

        public event Action OnRemoteConfigLoaded;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeDefaults();
        }

        private void Start()
        {
            _ = InitializeAsync();
        }

        private void InitializeDefaults()
        {
            // Seed default remote config values from Section 6 & 7
            localRemoteConfigCache[GameConstants.KeyDiffSpeedBase] = "2.0";
            localRemoteConfigCache[GameConstants.KeyDiffSpeedMult10_25] = "1.15";
            localRemoteConfigCache[GameConstants.KeyDiffSpeedMult25_50] = "1.30";
            localRemoteConfigCache[GameConstants.KeyDiffSpeedMult50_100] = "1.50";
            localRemoteConfigCache[GameConstants.KeyDiffSpeedMult100Plus] = "1.70";
            localRemoteConfigCache[GameConstants.KeyDiffScalingPer10Units] = "0.05";
            localRemoteConfigCache[GameConstants.KeyHazardAsteroidHeight] = "25.0";
            localRemoteConfigCache[GameConstants.KeyHazardGravityFlipHeight] = "50.0";
            localRemoteConfigCache[GameConstants.KeyHazardSolarWindHeight] = "100.0";
            localRemoteConfigCache[GameConstants.KeyStabilityGravity] = "9.81";
            localRemoteConfigCache[GameConstants.KeyStabilityCollapseThreshold] = "22.0";
            localRemoteConfigCache[GameConstants.KeyStabilityGraceDuration] = "0.60";
            localRemoteConfigCache[GameConstants.KeyStabilityPerfectThreshold] = "0.22";
            localRemoteConfigCache[GameConstants.KeyAdInterstitialInterval] = "3";
        }

        public async Task<bool> InitializeAsync()
        {
#if FIREBASE_ENABLED
            try
            {
                var dependencyStatus = await Firebase.FirebaseApp.CheckAndFixDependenciesAsync();
                if (dependencyStatus == Firebase.DependencyStatus.Available)
                {
                    isInitialized = true;
                    await SignInAnonymouslyAsync();
                    await FetchRemoteConfigAsync();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Firebase] Native SDK init failed, falling back to local service: {ex.Message}");
            }
#endif
            // Fallback / local simulation mode
            await Task.Yield();
            if (string.IsNullOrEmpty(currentUserId))
            {
                currentUserId = PlayerPrefs.GetString("LocalUserId", "");
                if (string.IsNullOrEmpty(currentUserId))
                {
                    currentUserId = "anon_" + Guid.NewGuid().ToString().Substring(0, 8);
                    PlayerPrefs.SetString("LocalUserId", currentUserId);
                    PlayerPrefs.Save();
                }
            }
            isInitialized = true;
            OnRemoteConfigLoaded?.Invoke();
            return true;
        }

        public async Task<bool> SignInAnonymouslyAsync()
        {
#if FIREBASE_ENABLED
            try
            {
                var auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
                var userCredential = await auth.SignInAnonymouslyAsync();
                currentUserId = userCredential.User.UserId;
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Firebase] Anonymous sign-in exception: {ex.Message}");
            }
#endif
            await Task.Yield();
            return true;
        }

        public async Task<bool> FetchRemoteConfigAsync()
        {
#if FIREBASE_ENABLED
            try
            {
                var rc = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance;
                await rc.FetchAsync(TimeSpan.Zero);
                await rc.ActivateAsync();

                foreach (var key in localRemoteConfigCache.Keys)
                {
                    var val = rc.GetValue(key);
                    if (val != null && !string.IsNullOrEmpty(val.StringValue))
                    {
                        localRemoteConfigCache[key] = val.StringValue;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Firebase] Remote Config fetch error: {ex.Message}");
            }
#endif
            await Task.Yield();
            OnRemoteConfigLoaded?.Invoke();
            return true;
        }

        public float GetRemoteFloat(string key, float defaultValue)
        {
            if (localRemoteConfigCache.TryGetValue(key, out string strVal))
            {
                if (float.TryParse(strVal, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float result))
                {
                    return result;
                }
            }
            return defaultValue;
        }

        public int GetRemoteInt(string key, int defaultValue)
        {
            if (localRemoteConfigCache.TryGetValue(key, out string strVal))
            {
                if (int.TryParse(strVal, out int result))
                {
                    return result;
                }
            }
            return defaultValue;
        }

        public string GetRemoteString(string key, string defaultValue)
        {
            if (localRemoteConfigCache.TryGetValue(key, out string strVal))
            {
                return strVal;
            }
            return defaultValue;
        }

        public void SetRemoteOverride(string key, string value)
        {
            localRemoteConfigCache[key] = value;
            OnRemoteConfigLoaded?.Invoke();
        }

        public async Task<bool> SubmitLeaderboardScoreAsync(string leaderboardId, float height, int score)
        {
            await Task.Yield();

            if (!cachedLeaderboards.ContainsKey(leaderboardId))
            {
                cachedLeaderboards[leaderboardId] = new List<LeaderboardEntry>();
            }

            var list = cachedLeaderboards[leaderboardId];
            var existing = list.Find(x => x.userId == currentUserId);
            if (existing != null)
            {
                if (height > existing.height)
                {
                    existing.height = height;
                    existing.score = score;
                    existing.timestamp = DateTime.UtcNow.ToString("o");
                }
            }
            else
            {
                list.Add(new LeaderboardEntry(currentUserId, "Pilot_" + currentUserId.Substring(Math.Max(0, currentUserId.Length - 4)), height, score, 0, DateTime.UtcNow.ToString("o")));
            }

            // Sort descending by height
            list.Sort((a, b) => b.height.CompareTo(a.height));
            for (int i = 0; i < list.Count; i++)
            {
                list[i].rank = i + 1;
            }

            return true;
        }

        public async Task<List<LeaderboardEntry>> GetTopScoresAsync(string leaderboardId, int limit = 10)
        {
            await Task.Yield();

            if (!cachedLeaderboards.ContainsKey(leaderboardId) || cachedLeaderboards[leaderboardId].Count == 0)
            {
                // Generate mock competitive leaderboard entries for seamless UX
                var mockList = new List<LeaderboardEntry>
                {
                    new LeaderboardEntry("bot_1", "NovaStacker", 142.5f, 2850, 1, DateTime.UtcNow.ToString("o")),
                    new LeaderboardEntry("bot_2", "AstroPulse", 118.2f, 2364, 2, DateTime.UtcNow.ToString("o")),
                    new LeaderboardEntry("bot_3", "ZeroGravityKing", 94.7f, 1890, 3, DateTime.UtcNow.ToString("o")),
                    new LeaderboardEntry("bot_4", "CosmicBuilder", 76.1f, 1520, 4, DateTime.UtcNow.ToString("o")),
                    new LeaderboardEntry("bot_5", "NebulaPilot", 54.0f, 1080, 5, DateTime.UtcNow.ToString("o"))
                };
                cachedLeaderboards[leaderboardId] = mockList;
            }

            var list = cachedLeaderboards[leaderboardId];
            int count = Mathf.Min(limit, list.Count);
            return list.GetRange(0, count);
        }
    }
}
