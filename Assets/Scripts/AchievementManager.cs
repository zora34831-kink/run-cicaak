using UnityEngine;
using System;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance { get; private set; }

    public event Action<string> AchievementUnlocked;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public bool IsUnlocked(string id) => PlayerPrefs.GetInt("ACH_" + id, 0) == 1;

    public void CheckRunAchievements(int score, float survivalSeconds, int totalCoins, int hideCount, bool escapedWithoutBeingCaught)
    {
        Unlock("FIRST_ESCAPE", escapedWithoutBeingCaught);
        Unlock("SURVIVOR", survivalSeconds >= 300f);
        Unlock("MASTER_HIDER", hideCount >= 20);
        Unlock("UNTOUCHABLE", escapedWithoutBeingCaught);
        Unlock("LEGEND_GECKO", score >= 100000);
        Unlock("COIN_HUNTER", totalCoins >= 1000);
    }

    public void Unlock(string id, bool condition)
    {
        if (!condition || IsUnlocked(id)) return;
        PlayerPrefs.SetInt("ACH_" + id, 1);
        PlayerPrefs.Save();
        AchievementUnlocked?.Invoke(id);
        Debug.Log("Achievement unlocked: " + id);
    }
}