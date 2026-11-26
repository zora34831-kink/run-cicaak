using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    public static PauseMenuController Instance { get; private set; }
    public bool IsPaused { get; private set; }

    private GameObject panel;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        BuildPanel();
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
        Time.timeScale = 1f;
    }

    public void Toggle()
    {
        if (GameManager.Instance != null && GameManager.Instance.RunEnded) return;
        if (IsPaused) Resume();
        else Pause();
    }

    public void Pause()
    {
        if (IsPaused) return;
        IsPaused = true;
        Time.timeScale = 0f;
        if (panel != null) panel.SetActive(true);
    }

    public void Resume()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        if (panel != null) panel.SetActive(false);
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        IsPaused = false;
        GameManager.Instance?.RestartRun();
    }

    public void ReturnToMenu()
    {
        Time.timeScale = 1f;
        IsPaused = false;
        if (Application.CanStreamedLevelBeLoaded("MainMenu"))
            SceneManager.LoadScene("MainMenu");
    }

    void BuildPanel()
    {
        panel = new GameObject("Pause Menu Panel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(transform, false);
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(620, 520);
        panel.GetComponent<Image>().color = new Color(0.025f, 0.04f, 0.055f, 0.96f);

        CreateText("PAUSED", new Vector2(0, 170), new Vector2(520, 80), 44);
        CreateButton("RESUME", new Vector2(0, 55), Resume);
        CreateButton("RESTART", new Vector2(0, -55), Restart);
        CreateButton("MAIN MENU", new Vector2(0, -165), ReturnToMenu);
        panel.SetActive(false);
    }

    void CreateText(string value, Vector2 position, Vector2 size, int fontSize)
    {
        GameObject go = new GameObject("Pause Title", typeof(RectTransform), typeof(Text));
        go.transform.SetParent(panel.transform, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        Text text = go.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = fontSize;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.text = value;
    }

    void CreateButton(string label, Vector2 position, UnityEngine.Events.UnityAction action)
    {
        GameObject go = new GameObject(label + " Button", typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(panel.transform, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(440, 76);
        go.GetComponent<Image>().color = new Color(0.12f, 0.2f, 0.25f, 1f);
        go.GetComponent<Button>().onClick.AddListener(action);

        GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(Text));
        labelObject.transform.SetParent(go.transform, false);
        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = labelRect.offsetMax = Vector2.zero;
        Text text = labelObject.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = 24;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.text = label;
        text.raycastTarget = false;
    }
}