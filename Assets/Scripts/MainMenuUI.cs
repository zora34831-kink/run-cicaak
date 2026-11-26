using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    private Canvas canvas;
    private RectTransform page;
    private Text statusText;
    private MainMenuController navigation;

    void Awake()
    {
        navigation = GetComponent<MainMenuController>();
        if (navigation == null) navigation = gameObject.AddComponent<MainMenuController>();

        EnsurePersistent<ProgressionManager>("Progression Manager");
        EnsurePersistent<GeckoShop>("Gecko Shop");
        EnsurePersistent<AchievementManager>("Achievement Manager");
        EnsurePersistent<FirebaseLeaderboardClient>("Firebase Leaderboard Client");
    }

    void Start()
    {
        BuildCanvas();
        ShowHome();
    }

    void EnsurePersistent<T>(string objectName) where T : Component
    {
        if (FindObjectOfType<T>() != null) return;
        new GameObject(objectName).AddComponent<T>();
    }

    void BuildCanvas()
    {
        GameObject canvasObject = new GameObject("Main Menu Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);
        canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;

        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));
    }

    void ShowHome()
    {
        ClearPage();
        CreateBackdrop();
        CreateText(page, "RUN CICAAK", 76, new Vector2(0, 560), new Vector2(900, 130), TextAnchor.MiddleCenter);
        CreateText(page, "THE LITTLE GECKO ESCAPE", 24, new Vector2(0, 485), new Vector2(900, 60), TextAnchor.MiddleCenter);

        CreateButton(page, "PLAY", new Vector2(0, 280), new Vector2(610, 100), navigation.Play);
        CreateButton(page, "CHARACTER", new Vector2(0, 145), new Vector2(610, 82), ShowCharacters);
        CreateButton(page, "SHOP", new Vector2(0, 35), new Vector2(610, 82), ShowShop);
        CreateButton(page, "HIGH SCORE", new Vector2(0, -75), new Vector2(610, 82), ShowLeaderboard);
        CreateButton(page, "SETTINGS", new Vector2(0, -185), new Vector2(610, 82), ShowSettings);

        int best = PlayerPrefs.GetInt("RUN_CICAAK_BEST", 0);
        int coins = PlayerPrefs.GetInt("RUN_CICAAK_WALLET", 0);
        CreateText(page, "BEST  " + best.ToString("N0") + "        COINS  " + coins.ToString("N0"),
            24, new Vector2(0, -360), new Vector2(850, 60), TextAnchor.MiddleCenter);
        CreateText(page, "INDONESIA LEADERBOARD", 18, new Vector2(0, -430), new Vector2(850, 50), TextAnchor.MiddleCenter);
    }

    void CreateBackdrop()
    {
        Camera camera = Camera.main;
        if (camera == null)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            camera = cameraObject.AddComponent<Camera>();
        }
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.055f, 0.085f, 0.11f);
    }

    void ShowCharacters() => ShowSkinPage(false);
    void ShowShop() => ShowSkinPage(true);

    void ShowSkinPage(bool purchaseMode)
    {
        ClearPage();
        string title = purchaseMode ? "GECKO SHOP" : "CHOOSE CHARACTER";
        CreateText(page, title, 52, new Vector2(0, 700), new Vector2(900, 90), TextAnchor.MiddleCenter);
        CreateText(page, "COINS  " + PlayerPrefs.GetInt("RUN_CICAAK_WALLET", 0).ToString("N0"),
            24, new Vector2(0, 625), new Vector2(800, 50), TextAnchor.MiddleCenter);

        GeckoShop shop = GeckoShop.Instance;
        if (shop != null)
        {
            for (int i = 0; i < shop.skins.Length; i++)
            {
                GeckoSkin skin = shop.skins[i];
                int index = i;
                string state = shop.IsOwned(skin.id) ? (shop.SelectedSkinId == skin.id ? "SELECTED" : "OWNED") : skin.price + " COINS";
                CreateButton(page, skin.displayName.ToUpper() + "  •  " + state,
                    new Vector2(0, 460 - i * 130), new Vector2(780, 94), () =>
                    {
                        bool success = shop.PurchaseOrSelect(skin.id);
                        statusText.text = success ? skin.displayName + " selected." : "Coins tidak cukup.";
                        ShowSkinPage(purchaseMode);
                    });
            }
        }

        statusText = CreateText(page, "", 22, new Vector2(0, -150), new Vector2(800, 50), TextAnchor.MiddleCenter);
        CreateButton(page, "BACK", new Vector2(0, -620), new Vector2(500, 82), ShowHome);
    }

    void ShowLeaderboard()
    {
        ClearPage();
        CreateText(page, "INDONESIA TOP 100", 50, new Vector2(0, 700), new Vector2(900, 90), TextAnchor.MiddleCenter);
        statusText = CreateText(page, "Loading leaderboard...", 22, new Vector2(0, 100), new Vector2(900, 900), TextAnchor.UpperCenter);
        CreateButton(page, "REFRESH", new Vector2(0, -560), new Vector2(420, 78), LoadLeaderboard);
        CreateButton(page, "BACK", new Vector2(0, -660), new Vector2(420, 78), ShowHome);
        LoadLeaderboard();
    }

    async void LoadLeaderboard()
    {
        if (statusText == null) return;
        FirebaseLeaderboardClient client = FirebaseLeaderboardClient.Instance;
        if (client == null || !client.IsReady)
        {
            statusText.text = "Leaderboard belum tersambung. Firebase perlu dikonfigurasi.";
            return;
        }

        IndonesiaLeaderboardSnapshot snapshot = await client.GetTopAsync(100);
        if (statusText == null) return;
        StringBuilder builder = new StringBuilder();
        if (snapshot.myRank.HasValue) builder.AppendLine("YOUR RANK  #" + snapshot.myRank.Value);
        builder.AppendLine();
        foreach (IndonesiaLeaderboardEntry entry in snapshot.entries)
            builder.AppendLine(entry.rank + ".  " + entry.displayName + "   " + entry.score.ToString("N0"));
        statusText.text = builder.Length > 0 ? builder.ToString() : "Belum ada skor. Jadilah yang pertama!";
    }

    void ShowSettings()
    {
        ClearPage();
        CreateText(page, "SETTINGS", 54, new Vector2(0, 650), new Vector2(900, 90), TextAnchor.MiddleCenter);
        bool haptics = PlayerPrefs.GetInt("RUN_CICAAK_HAPTICS", 1) == 1;
        CreateButton(page, "HAPTIC  " + (haptics ? "ON" : "OFF"), new Vector2(0, 300), new Vector2(620, 90), () =>
        {
            PlayerPrefs.SetInt("RUN_CICAAK_HAPTICS", haptics ? 0 : 1);
            PlayerPrefs.Save();
            ShowSettings();
        });
        CreateText(page, "Graphics: Optimized for Android", 22, new Vector2(0, 150), new Vector2(800, 60), TextAnchor.MiddleCenter);
        CreateButton(page, "BACK", new Vector2(0, -620), new Vector2(500, 82), ShowHome);
    }

    void ClearPage()
    {
        if (canvas == null) return;
        for (int i = canvas.transform.childCount - 1; i >= 0; i--)
            Destroy(canvas.transform.GetChild(i).gameObject);

        GameObject root = new GameObject("Menu Page", typeof(RectTransform));
        root.transform.SetParent(canvas.transform, false);
        page = root.GetComponent<RectTransform>();
        page.anchorMin = Vector2.zero;
        page.anchorMax = Vector2.one;
        page.offsetMin = page.offsetMax = Vector2.zero;
        statusText = null;
    }

    Text CreateText(Transform parent, string value, int size, Vector2 position, Vector2 dimensions, TextAnchor alignment)
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
        text.alignment = alignment;
        text.color = Color.white;
        text.text = value;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;
        return text;
    }

    void CreateButton(Transform parent, string label, Vector2 position, Vector2 dimensions, UnityEngine.Events.UnityAction action)
    {
        GameObject go = new GameObject(label + " Button", typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = dimensions;
        go.GetComponent<Image>().color = new Color(0.12f, 0.2f, 0.25f, 0.95f);
        go.GetComponent<Button>().onClick.AddListener(action);
        Text labelText = CreateText(go.transform, label, 25, Vector2.zero, dimensions - new Vector2(20, 10), TextAnchor.MiddleCenter);
        labelText.raycastTarget = false;
    }
}