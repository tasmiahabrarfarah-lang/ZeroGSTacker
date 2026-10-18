using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ZeroGStacker.Backend
{
    public interface IFirebaseService
    {
        bool IsInitialized { get; }
        string UserId { get; }

        Task<bool> InitializeAsync();
        Task<bool> SignInAnonymouslyAsync();
        Task<bool> FetchRemoteConfigAsync();
        float GetRemoteFloat(string key, float defaultValue);
        int GetRemoteInt(string key, int defaultValue);
        string GetRemoteString(string key, string defaultValue);
        Task<bool> SubmitLeaderboardScoreAsync(string leaderboardId, float height, int score);
        Task<List<LeaderboardEntry>> GetTopScoresAsync(string leaderboardId, int limit = 10);
    }

    [Serializable]
    public class LeaderboardEntry
    {
        public string userId;
        public string displayName;
        public float height;
        public int score;
        public int rank;
        public string timestamp;

        public LeaderboardEntry(string uid, string name, float h, int s, int r, string time)
        {
            userId = uid;
            displayName = name;
            height = h;
            score = s;
            rank = r;
            timestamp = time;
        }
    }
}
