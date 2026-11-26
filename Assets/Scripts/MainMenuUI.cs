using System.Text;
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
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));
    }

    void ShowHome()
    {
        ClearPage();
        CreateBackdrop();
        CreateText(page, "RUN CICAAK", 70, new Vector2(0, 410), new Vector2(1100, 110), TextAnchor.MiddleCenter);
        CreateText(page, "THE LITTLE GECKO ESCAPE", 22, new Vector2(0, 345), new Vector2(1000, 50), TextAnchor.MiddleCenter);

        CreateButton(page, "PLAY", new Vector2(0, 245), new Vector2(610, 78), navigation.Play);
        CreateButton(page, "CHARACTER", new Vector2(0, 150), new Vector2(610, 72), ShowCharacters);
        CreateButton(page, "SHOP", new Vector2(0, 62), new Vector2(610, 72), ShowShop);
        CreateButton(page, "HIGH SCORE", new Vector2(0, -26), new Vector2(610, 72), ShowLeaderboard);
        CreateButton(page, "ACHIEVEMENTS", new Vector2(0, -114), new Vector2(610, 72), ShowAchievements);
        CreateButton(page, "SETTINGS", new Vector2(0, -202), new Vector2(610, 72), ShowSettings);

        int best = PlayerPrefs.GetInt("RUN_CICAAK_BEST", 0);
        int coins = PlayerPrefs.GetInt("RUN_CICAAK_WALLET", 0);
        CreateText(page, "BEST  " + best.ToString("N0") + "        COINS  " + coins.ToString("N0"),
            20, new Vector2(0, -345), new Vector2(850, 45), TextAnchor.MiddleCenter);
        CreateText(page, "INDONESIA LEADERBOARD", 16, new Vector2(0, -395), new Vector2(850, 35), TextAnchor.MiddleCenter);
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
        CreateText(page, title, 48, new Vector2(0, 420), new Vector2(900, 80), TextAnchor.MiddleCenter);
        CreateText(page, "COINS  " + PlayerPrefs.GetInt("RUN_CICAAK_WALLET", 0).ToString("N0"),
            22, new Vector2(0, 350), new Vector2(800, 45), TextAnchor.MiddleCenter);

        GeckoShop shop = GeckoShop.Instance;
        if (shop != null)
        {
            for (int i = 0; i < shop.skins.Length; i++)
            {
                GeckoSkin skin = shop.skins[i];
                string state = shop.IsOwned(skin.id) ? (shop.SelectedSkinId == skin.id ? "SELECTED" : "OWNED") : skin.price + " COINS";
                CreateButton(page, skin.displayName.ToUpper() + "  •  " + state,
                    new Vector2(0, 230 - i * 105), new Vector2(760, 76), () =>
                    {
                        shop.PurchaseOrSelect(skin.id);
                        ShowSkinPage(purchaseMode);
                    });
            }
        }

        CreateButton(page, "BACK", new Vector2(0, -425), new Vector2(460, 68), ShowHome);
    }

    void ShowLeaderboard()
    {
        ClearPage();
        CreateText(page, "INDONESIA TOP 100", 46, new Vector2(0, 420), new Vector2(900, 80), TextAnchor.MiddleCenter);
        statusText = CreateText(page, "Loading leaderboard...", 20, new Vector2(0, 20), new Vector2(1000, 650), TextAnchor.UpperCenter);
        CreateButton(page, "REFRESH", new Vector2(-130, -410), new Vector2(360, 68), LoadLeaderboard);
        CreateButton(page, "BACK", new Vector2(230, -410), new Vector2(360, 68), ShowHome);
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

    void ShowAchievements()
    {
        ClearPage();
        CreateText(page, "ACHIEVEMENTS", 48, new Vector2(0, 420), new Vector2(900, 80), TextAnchor.MiddleCenter);

        string[] ids = { "FIRST_ESCAPE", "SURVIVOR", "MASTER_HIDER", "UNTOUCHABLE", "LEGEND_GECKO", "COIN_HUNTER" };
        string[] names = { "First Escape — escape once", "Survivor — survive 5 minutes", "Master Hider — hide 20 times", "Untouchable — escape uncaught", "Legend Gecko — score 100,000", "Coin Hunter — collect 1,000 coins" };
        AchievementManager manager = AchievementManager.Instance;
        for (int i = 0; i < ids.Length; i++)
        {
            bool unlocked = manager != null && manager.IsUnlocked(ids[i]);
            CreateText(page, (unlocked ? "[UNLOCKED]  " : "[LOCKED]  ") + names[i],
                22, new Vector2(0, 245 - i * 76), new Vector2(1050, 58), TextAnchor.MiddleCenter);
        }

        CreateButton(page, "BACK", new Vector2(0, -425), new Vector2(460, 68), ShowHome);
    }

    void ShowSettings()
    {
        ClearPage();
        CreateText(page, "SETTINGS", 50, new Vector2(0, 420), new Vector2(900, 80), TextAnchor.MiddleCenter);
        CreateText(page, "PLAYER NICKNAME", 20, new Vector2(0, 290), new Vector2(700, 40), TextAnchor.MiddleCenter);
        CreateInputField(page, new Vector2(0, 220));

        bool haptics = PlayerPrefs.GetInt("RUN_CICAAK_HAPTICS", 1) == 1;
        CreateButton(page, "HAPTIC  " + (haptics ? "ON" : "OFF"), new Vector2(0, 80), new Vector2(620, 78), () =>
        {
            PlayerPrefs.SetInt("RUN_CICAAK_HAPTICS", haptics ? 0 : 1);
            PlayerPrefs.Save();
            ShowSettings();
        });
        CreateText(page, "Graphics: Optimized for Android", 20, new Vector2(0, -35), new Vector2(800, 55), TextAnchor.MiddleCenter);
        CreateButton(page, "BACK", new Vector2(0, -425), new Vector2(460, 68), ShowHome);
    }

    void CreateInputField(Transform parent, Vector2 position)
    {
        GameObject go = new GameObject("Player Nickname Input", typeof(RectTransform), typeof(Image), typeof(InputField));
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(650, 64);

        Image background = go.GetComponent<Image>();
        background.color = new Color(0.13f, 0.18f, 0.22f, 1f);

        Text valueText = CreateText(go.transform, "", 24, Vector2.zero, new Vector2(610, 54), TextAnchor.MiddleCenter);
        Text placeholder = CreateText(go.transform, "Enter a game nickname", 22, Vector2.zero, new Vector2(610, 54), TextAnchor.MiddleCenter);
        placeholder.color = new Color(0.65f, 0.7f, 0.74f, 0.8f);

        InputField input = go.GetComponent<InputField>();
        input.targetGraphic = background;
        input.textComponent = valueText;
        input.placeholder = placeholder;
        input.characterLimit = 16;
        input.text = PlayerPrefs.GetString("RUN_CICAAK_PLAYER_NAME", "Gecko");
        input.onEndEdit.AddListener(value =>
        {
            StringBuilder safeName = new StringBuilder();
            foreach (char ch in value)
                if (char.IsLetterOrDigit(ch) || ch == ' ' || ch == '_' || ch == '-') safeName.Append(ch);
            string name = safeName.ToString().Trim();
            if (name.Length > 16) name = name.Substring(0, 16);
            if (string.IsNullOrEmpty(name)) name = "Gecko";
            PlayerPrefs.SetString("RUN_CICAAK_PLAYER_NAME", name);
            PlayerPrefs.Save();
        });
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
        Text labelText = CreateText(go.transform, label, 24, Vector2.zero, dimensions - new Vector2(20, 10), TextAnchor.MiddleCenter);
        labelText.raycastTarget = false;
    }
}