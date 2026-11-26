using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

#if FIREBASE_UNITY_SDK
using Firebase;
using Firebase.Auth;
using Firebase.Functions;
#endif

public class FirebaseLeaderboardClient : MonoBehaviour
{
    public static FirebaseLeaderboardClient Instance { get; private set; }
    public bool IsReady { get; private set; }

#if FIREBASE_UNITY_SDK
    private FirebaseFunctions functions;

    async void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (dependencyStatus != DependencyStatus.Available)
        {
            Debug.LogError("Firebase dependency error: " + dependencyStatus);
            return;
        }

        if (FirebaseAuth.DefaultInstance.CurrentUser == null)
            await FirebaseAuth.DefaultInstance.SignInAnonymouslyAsync();

        functions = FirebaseFunctions.GetInstance("asia-southeast2");
        IsReady = true;
    }

    public async Task<bool> SubmitRunAsync(int score, float survivalSeconds, string displayName, string avatarId)
    {
        if (!IsReady || functions == null) return false;

        var payload = new Dictionary<string, object>
        {
            { "score", score },
            { "survivalSeconds", survivalSeconds },
            { "displayName", displayName },
            { "avatarId", avatarId }
        };

        try
        {
            var callable = functions.GetHttpsCallable("submitRun");
            await callable.CallAsync(payload);
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogWarning("Leaderboard submit failed: " + exception.Message);
            return false;
        }
    }

    public async Task<bool> SubmitRunWhenReadyAsync(int score, float survivalSeconds, string displayName, string avatarId)
    {
        float waited = 0f;
        while (!IsReady && waited < 20f)
        {
            await Task.Delay(250);
            waited += 0.25f;
        }
        return await SubmitRunAsync(score, survivalSeconds, displayName, avatarId);
    }

    public async Task<IndonesiaLeaderboardSnapshot> GetTopAsync(int limit = 100)
    {
        if (!IsReady || functions == null) return new IndonesiaLeaderboardSnapshot();

        try
        {
            var callable = functions.GetHttpsCallable("getIndonesiaLeaderboard");
            var result = await callable.CallAsync(new Dictionary<string, object> { { "limit", limit } });
            var root = result.Data as IDictionary<string, object>;
            if (root == null) return new IndonesiaLeaderboardSnapshot();

            var snapshot = new IndonesiaLeaderboardSnapshot();
            if (root.TryGetValue("myRank", out object rankValue) && rankValue != null)
                snapshot.myRank = Convert.ToInt32(rankValue);

            if (root.TryGetValue("entries", out object entriesValue) && entriesValue is IEnumerable<object> rawEntries)
            {
                var entries = new List<IndonesiaLeaderboardEntry>();
                foreach (object raw in rawEntries)
                {
                    if (!(raw is IDictionary<string, object> item)) continue;
                    entries.Add(new IndonesiaLeaderboardEntry
                    {
                        rank = ReadInt(item, "rank"),
                        displayName = ReadString(item, "displayName"),
                        avatarId = ReadString(item, "avatarId"),
                        score = ReadInt(item, "score"),
                        survivalSeconds = ReadFloat(item, "survivalSeconds")
                    });
                }
                snapshot.entries = entries.ToArray();
            }

            return snapshot;
        }
        catch (Exception exception)
        {
            Debug.LogWarning("Leaderboard load failed: " + exception.Message);
            return new IndonesiaLeaderboardSnapshot();
        }
    }

    int ReadInt(IDictionary<string, object> data, string key) =>
        data.TryGetValue(key, out object value) && value != null ? Convert.ToInt32(value) : 0;

    float ReadFloat(IDictionary<string, object> data, string key) =>
        data.TryGetValue(key, out object value) && value != null ? Convert.ToSingle(value) : 0f;

    string ReadString(IDictionary<string, object> data, string key) =>
        data.TryGetValue(key, out object value) && value != null ? value.ToString() : "";
#else
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log("Firebase Unity SDK belum dipasang. Adapter leaderboard menunggu konfigurasi Firebase.");
    }

    public Task<bool> SubmitRunAsync(int score, float survivalSeconds, string displayName, string avatarId) =>
        Task.FromResult(false);

    public Task<bool> SubmitRunWhenReadyAsync(int score, float survivalSeconds, string displayName, string avatarId) =>
        Task.FromResult(false);

    public Task<IndonesiaLeaderboardSnapshot> GetTopAsync(int limit = 100) =>
        Task.FromResult(new IndonesiaLeaderboardSnapshot());
#endif
}