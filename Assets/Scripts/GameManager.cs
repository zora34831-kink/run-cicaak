using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int Score { get; private set; }
    public int Coins { get; private set; }
    public float SurvivalTime { get; private set; }
    public bool RunEnded { get; private set; }

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

    public void EndRun()
    {
        if (RunEnded) return;
        RunEnded = true;
        int best = PlayerPrefs.GetInt("RUN_CICAAK_BEST", 0);
        if (Score > best)
            PlayerPrefs.SetInt("RUN_CICAAK_BEST", Score);

        PlayerPrefs.Save();
        Debug.Log("RUN CICAAK selesai. Score: " + Score + " | Waktu: " + SurvivalTime.ToString("F1") + "s");
    }

    public void RestartRun()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}