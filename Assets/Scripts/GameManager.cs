using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int Score { get; private set; }
    public int Coins { get; private set; }
    public float SurvivalTime { get; private set; }
    public bool RunEnded { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Update()
    {
        if (!RunEnded) SurvivalTime += Time.deltaTime;
    }

    public void AddScore(int amount) => Score += Mathf.Max(0, amount);
    public void AddCoins(int amount) => Coins += Mathf.Max(0, amount);

    public void RegisterHide()
    {
        PlayerPrefs.SetInt("RUN_CICAAK_HIDE_COUNT", PlayerPrefs.GetInt("RUN_CICAAK_HIDE_COUNT", 0) + 1);
    }

    public void EndRun()
    {
        if (RunEnded) return;
        RunEnded = true;
        int best = PlayerPrefs.GetInt("RUN_CICAAK_BEST", 0);
        if (Score > best)
        {
            PlayerPrefs.SetInt("RUN_CICAAK_BEST", Score);
            PlayerPrefs.Save();
        }
        Debug.Log("RUN CICAAK selesai. Score: " + Score + " | Waktu: " + SurvivalTime.ToString("F1") + "s");
    }

    public void RestartRun()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}