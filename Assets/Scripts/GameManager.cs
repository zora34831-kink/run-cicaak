using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int Score { get; private set; }
    public int Coins { get; private set; }
    public float SurvivalTime { get; private set; }
    public bool RunEnded { get; private set; }
    public bool Escaped { get; private set; }
    public event System.Action<bool> RunFinished;

    private float scoreAccumulator;
    private float xpAccumulator;
    private const string CoinsKey = "RUN_CICAAK_WALLET";

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        Coins = Mathf.Max(0, PlayerPrefs.GetInt(CoinsKey, 0));
    }

    void Update()
    {
        if (RunEnded) return;

        SurvivalTime += Time.deltaTime;
        scoreAccumulator += Time.deltaTime;
        xpAccumulator += Time.deltaTime;

        while (scoreAccumulator >= 1f)
        {
            scoreAccumulator -= 1f;
            Score += 10;
        }

        while (xpAccumulator >= 30f)
        {
            xpAccumulator -= 30f;
            ProgressionManager.Instance?.AddXP(5);
        }
    }

    public void AddScore(int amount) => Score += Mathf.Max(0, amount);

    public void AddCoins(int amount)
    {
        if (amount <= 0) return;
        Coins += amount;
        int lifetimeCoins = PlayerPrefs.GetInt("RUN_CICAAK_LIFETIME_COINS", 0) + amount;
        PlayerPrefs.SetInt("RUN_CICAAK_LIFETIME_COINS", lifetimeCoins);
        PlayerPrefs.SetInt(CoinsKey, Coins);
        PlayerPrefs.Save();
    }

    public bool SpendCoins(int amount)
    {
        if (amount < 0 || Coins < amount) return false;
        Coins -= amount;
        PlayerPrefs.SetInt(CoinsKey, Coins);
        PlayerPrefs.Save();
        return true;
    }

    public void RegisterHide()
    {
        PlayerPrefs.SetInt("RUN_CICAAK_HIDE_COUNT", PlayerPrefs.GetInt("RUN_CICAAK_HIDE_COUNT", 0) + 1);
        PlayerPrefs.Save();
    }

    public void EndRun() => FinishRun(false);

    public void CompleteEscape() => FinishRun(true);

    private void FinishRun(bool escaped)
    {
        if (RunEnded) return;
        RunEnded = true;
        Escaped = escaped;
        Time.timeScale = 1f;
        if (escaped) RunCicaakAudio.PlayEscape(); else RunCicaakAudio.PlayCaught();

        int best = PlayerPrefs.GetInt("RUN_CICAAK_BEST", 0);
        if (Score > best)
            PlayerPrefs.SetInt("RUN_CICAAK_BEST", Score);

        int lifetimeCoins = PlayerPrefs.GetInt("RUN_CICAAK_LIFETIME_COINS", 0);
        int hideCount = PlayerPrefs.GetInt("RUN_CICAAK_HIDE_COUNT", 0);
        AchievementManager.Instance?.CheckRunAchievements(
            Score, SurvivalTime, lifetimeCoins, hideCount, escaped
        );

        PlayerPrefs.Save();

        FirebaseLeaderboardClient leaderboard = FirebaseLeaderboardClient.Instance;
        if (leaderboard != null)
        {
            string playerName = PlayerPrefs.GetString("RUN_CICAAK_PLAYER_NAME", "Gecko");
            string avatarId = GeckoShop.Instance != null ? GeckoShop.Instance.SelectedSkinId : "classic";
            _ = leaderboard.SubmitRunWhenReadyAsync(Score, SurvivalTime, playerName, avatarId);
        }

        Debug.Log("RUN CICAAK selesai. Score: " + Score + " | Waktu: " + SurvivalTime.ToString("F1") + "s | Escape: " + escaped);
        RunFinished?.Invoke(escaped);
    }

    public void RestartRun()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}