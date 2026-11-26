using UnityEngine;
using UnityEngine.UI;

public class RunEndOverlay : MonoBehaviour
{
    private GameObject panel;
    private Text titleText;
    private Text detailText;

    void Start()
    {
        BuildOverlay();
        if (GameManager.Instance != null)
            GameManager.Instance.RunFinished += Show;
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.RunFinished -= Show;
    }

    void BuildOverlay()
    {
        Transform existing = transform.Find("Run End Panel");
        if (existing != null) { panel = existing.gameObject; panel.SetActive(false); return; }

        panel = new GameObject("Run End Panel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(transform, false);
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(620, 390);
        panel.GetComponent<Image>().color = new Color(0.035f, 0.05f, 0.07f, 0.94f);

        titleText = CreateText(panel.transform, "RESULT", 42, new Vector2(0, 105), new Vector2(560, 80));
        detailText = CreateText(panel.transform, "", 25, new Vector2(0, 25), new Vector2(560, 100));
        CreateButton(panel.transform, "MAIN MENU", new Vector2(-135, -125), () =>
        {
            if (Application.CanStreamedLevelBeLoaded("MainMenu"))
                UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
            else
                Debug.LogWarning("Scene MainMenu belum dibuat atau belum masuk Build Settings.");
        });
        CreateButton(panel.transform, "RETRY", new Vector2(135, -125), () =>
        {
            if (GameManager.Instance != null) GameManager.Instance.RestartRun();
        });
        panel.SetActive(false);
    }

    void Show(bool escaped)
    {
        if (panel == null) BuildOverlay();
        if (titleText != null) titleText.text = escaped ? "ESCAPE SUCCESS" : "CAUGHT!";
        if (detailText != null)
        {
            GameManager game = GameManager.Instance;
            detailText.text = "SCORE  " + (game != null ? game.Score.ToString("N0") : "0") +
                              "\nTIME  " + (game != null ? game.SurvivalTime.ToString("F1") : "0") + "s";
        }
        panel.transform.SetAsLastSibling();
        panel.SetActive(true);
    }

    Text CreateText(Transform parent, string value, int size, Vector2 position, Vector2 dimensions)
    {
        GameObject go = new GameObject("Text", typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = dimensions;
        Text text = go.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = size;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.text = value;
        return text;
    }

    void CreateButton(Transform parent, string label, Vector2 position, UnityEngine.Events.UnityAction action)
    {
        GameObject go = new GameObject(label + " Button", typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(220, 66);
        go.GetComponent<Image>().color = new Color(0.16f, 0.25f, 0.3f, 1f);
        go.GetComponent<Button>().onClick.AddListener(action);

        Text labelText = CreateText(go.transform, label, 20, Vector2.zero, new Vector2(210, 60));
        labelText.raycastTarget = false;
    }
}