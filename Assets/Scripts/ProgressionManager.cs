using UnityEngine;
using UnityEngine.Events;

public class ProgressionManager : MonoBehaviour
{
    public static ProgressionManager Instance { get; private set; }

    [Header("Progress")]
    public int startingLevel = 1;
    public int xpPerLevel = 250;
    public UnityEvent<int, int, int> onProgressChanged;

    public int Level { get; private set; }
    public int TotalXP { get; private set; }

    private const string LevelKey = "RUN_CICAAK_LEVEL";
    private const string XPKey = "RUN_CICAAK_XP";

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Level = Mathf.Max(startingLevel, PlayerPrefs.GetInt(LevelKey, startingLevel));
        TotalXP = Mathf.Max(0, PlayerPrefs.GetInt(XPKey, 0));
        Notify();
    }

    public void AddXP(int amount)
    {
        if (amount <= 0) return;
        TotalXP += amount;
        while (TotalXP >= xpPerLevel * Level)
            Level++;

        PlayerPrefs.SetInt(LevelKey, Level);
        PlayerPrefs.SetInt(XPKey, TotalXP);
        PlayerPrefs.Save();
        Notify();
    }

    public int XPIntoCurrentLevel => TotalXP - XPRequiredBeforeLevel(Level);
    public int XPForNextLevel => xpPerLevel * Level;

    private int XPRequiredBeforeLevel(int level)
    {
        int total = 0;
        for (int i = 1; i < level; i++) total += xpPerLevel * i;
        return total;
    }

    private void Notify()
    {
        onProgressChanged?.Invoke(Level, XPIntoCurrentLevel, XPForNextLevel);
    }
}