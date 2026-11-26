using UnityEngine;
using UnityEngine.UI;

public class RunHUD : MonoBehaviour
{
    public Text scoreText;
    public Text coinText;
    public Text timerText;
    public Text bestText;
    public Text levelText;
    public Text xpText;

    void Update()
    {
        GameManager game = GameManager.Instance;
        if (game != null)
        {
            if (scoreText != null) scoreText.text = "SCORE  " + game.Score.ToString("N0");
            if (coinText != null) coinText.text = "COINS  " + game.Coins.ToString("N0");
            if (timerText != null)
            {
                int seconds = Mathf.FloorToInt(game.SurvivalTime);
                timerText.text = string.Format("{0:00}:{1:00}", seconds / 60, seconds % 60);
            }
            if (bestText != null) bestText.text = "BEST  " + PlayerPrefs.GetInt("RUN_CICAAK_BEST", 0).ToString("N0");
        }

        ProgressionManager progress = ProgressionManager.Instance;
        if (progress != null)
        {
            if (levelText != null) levelText.text = "LV " + progress.Level;
            if (xpText != null) xpText.text = progress.XPIntoCurrentLevel + " / " + progress.XPForNextLevel + " XP";
        }
    }
}