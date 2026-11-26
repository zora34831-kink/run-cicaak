using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PrototypeHouseBootstrap : MonoBehaviour
{
    [Header("Prototype blockout")]
    public bool buildOnStart = true;
    public float roomSize = 10f;

    private Material floorMat, wallMat, furnitureMat, geckoMat, humanMat, coinMat;

    void Start()
    {
        if (buildOnStart) Build();
    }

    [ContextMenu("Build Prototype House")]
    public void Build()
    {
        if (FindObjectOfType<PlayerController>() != null)
        {
            Debug.LogWarning("RUN CICAAK: Player sudah ada. Bootstrap dilewati agar tidak membuat duplikat.");
            return;
        }

        floorMat = MakeMaterial(new Color(0.48f, 0.43f, 0.36f));
        wallMat = MakeMaterial(new Color(0.78f, 0.75f, 0.68f));
        furnitureMat = MakeMaterial(new Color(0.28f, 0.19f, 0.13f));
        geckoMat = MakeMaterial(new Color(0.25f, 0.72f, 0.28f));
        humanMat = MakeMaterial(new Color(0.2f, 0.24f, 0.3f));
        coinMat = MakeMaterial(new Color(1f, 0.72f, 0.08f));

        BuildRoom("RUANG TAMU", new Vector3(0, 0, 0));
        BuildRoom("DAPUR", new Vector3(12, 0, 0));
        BuildRoom("KAMAR", new Vector3(0, 0, 12));
        BuildRoom("GUDANG", new Vector3(12, 0, 12));
        CreateCorridor(new Vector3(6, -0.15f, 0), new Vector3(2.4f, 0.3f, 2.4f));
        CreateCorridor(new Vector3(6, -0.15f, 12), new Vector3(2.4f, 0.3f, 2.4f));
        CreateCorridor(new Vector3(0, -0.15f, 6), new Vector3(2.4f, 0.3f, 2.4f));
        CreateCorridor(new Vector3(12, -0.15f, 6), new Vector3(2.4f, 0.3f, 2.4f));

        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Gecko Player (Prototype)";
        player.transform.position = new Vector3(0, 1, 0);
        player.transform.localScale = new Vector3(0.55f, 0.45f, 0.55f);
        Destroy(player.GetComponent<CapsuleCollider>());
        player.AddComponent<CharacterController>();
        player.AddComponent<PlayerController>();
        player.AddComponent<CatchSequence>();
        player.AddComponent<GeckoSkinApplier>();
        player.GetComponent<Renderer>().material = geckoMat;
        PlayerController playerController = player.GetComponent<PlayerController>();
        foreach (HidingSpot spot in FindObjectsOfType<HidingSpot>())
            spot.SetPlayer(playerController);

        GameObject human = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        human.name = "Human Chaser (Prototype)";
        human.transform.position = new Vector3(-3, 1, -2);
        human.transform.localScale = new Vector3(0.9f, 1f, 0.9f);
        human.GetComponent<Renderer>().material = humanMat;
        HumanChaserAI ai = human.AddComponent<HumanChaserAI>();
        ai.target = player.transform;

        GameObject cameraObject = Camera.main != null ? Camera.main.gameObject : new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0, 4.2f, -6f);
        CameraFollow follow = cameraObject.GetComponent<CameraFollow>();
        if (follow == null) follow = cameraObject.AddComponent<CameraFollow>();
        follow.target = player.transform;

        CreateManager<GameManager>("Game Manager");
        CreateManager<ProgressionManager>("Progression Manager");
        CreateManager<GeckoShop>("Gecko Shop");
        CreateManager<AchievementManager>("Achievement Manager");

        CreateCoins(new Vector3(3, 0.7f, 1));
        CreateCoins(new Vector3(15, 0.7f, 2));
        CreateCoins(new Vector3(2, 0.7f, 14));
        CreateCoins(new Vector3(14, 0.7f, 14));
        CreateEscapeExit(new Vector3(16, 0.1f, 16));

        BuildTouchUI(playerController);
        CreateLighting();
        Debug.Log("RUN CICAAK: Prototype blockout dibuat. Ganti primitive dengan model 3D final sebelum rilis.");
    }

    void BuildRoom(string label, Vector3 center)
    {
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = label + " Floor";
        floor.transform.position = center + new Vector3(0, -0.15f, 0);
        floor.transform.localScale = new Vector3(roomSize, 0.3f, roomSize);
        floor.GetComponent<Renderer>().material = floorMat;

        CreateWallWithOpening(label + " Wall North", center, true, roomSize / 2, 2.4f);
        CreateWallWithOpening(label + " Wall South", center, true, -roomSize / 2, 2.4f);
        CreateWallWithOpening(label + " Wall East", center, false, roomSize / 2, 2.4f);
        CreateWallWithOpening(label + " Wall West", center, false, -roomSize / 2, 2.4f);

        Vector3 furniturePosition = center + new Vector3(2.4f, 0.65f, 1.8f);
        GameObject cabinet = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cabinet.name = label + " Hiding Cabinet";
        cabinet.transform.position = furniturePosition;
        cabinet.transform.localScale = new Vector3(1.7f, 1.3f, 1.1f);
        cabinet.GetComponent<Renderer>().material = furnitureMat;

        GameObject hideAnchor = new GameObject("Hide Position");
        hideAnchor.transform.position = furniturePosition + new Vector3(-1.1f, -0.3f, 0);
        HidingSpot hiding = cabinet.AddComponent<HidingSpot>();
        hiding.hidePosition = hideAnchor.transform;
        hiding.interactDistance = 2.5f;

        GameObject table = GameObject.CreatePrimitive(PrimitiveType.Cube);
        table.name = label + " Table";
        table.transform.position = center + new Vector3(-2.3f, 0.65f, -1.8f);
        table.transform.localScale = new Vector3(2.1f, 1.3f, 1.5f);
        table.GetComponent<Renderer>().material = furnitureMat;
    }

    void CreateCorridor(Vector3 position, Vector3 scale)
    {
        GameObject corridor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        corridor.name = "Room Connector Floor";
        corridor.transform.position = position;
        corridor.transform.localScale = scale;
        corridor.GetComponent<Renderer>().material = floorMat;
    }

    void CreateEscapeExit(Vector3 position)
    {
        GameObject exit = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        exit.name = "Escape Exit";
        exit.transform.position = position;
        exit.transform.localScale = new Vector3(1.8f, 0.12f, 1.8f);
        exit.GetComponent<Renderer>().material = coinMat;
        exit.AddComponent<EscapeZone>();
    }

    void CreateWallWithOpening(string label, Vector3 center, bool horizontal, float edgeOffset, float openingWidth)
    {
        float segmentLength = (roomSize - openingWidth) * 0.5f;
        if (horizontal)
        {
            float z = center.z + edgeOffset;
            CreateWall(label + " A", new Vector3(center.x - (openingWidth + segmentLength) * 0.5f, 1.5f, z), new Vector3(segmentLength, 3, 0.25f));
            CreateWall(label + " B", new Vector3(center.x + (openingWidth + segmentLength) * 0.5f, 1.5f, z), new Vector3(segmentLength, 3, 0.25f));
        }
        else
        {
            float x = center.x + edgeOffset;
            CreateWall(label + " A", new Vector3(x, 1.5f, center.z - (openingWidth + segmentLength) * 0.5f), new Vector3(0.25f, 3, segmentLength));
            CreateWall(label + " B", new Vector3(x, 1.5f, center.z + (openingWidth + segmentLength) * 0.5f), new Vector3(0.25f, 3, segmentLength));
        }
    }

    void CreateWall(string label, Vector3 position, Vector3 scale)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = label;
        wall.transform.position = position;
        wall.transform.localScale = scale;
        wall.GetComponent<Renderer>().material = wallMat;
    }

    void CreateCoins(Vector3 start)
    {
        for (int i = 0; i < 4; i++)
        {
            GameObject coin = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            coin.name = "Coin";
            coin.transform.position = start + new Vector3(i * 0.65f, 0, 0);
            coin.transform.localScale = Vector3.one * 0.35f;
            coin.GetComponent<Renderer>().material = coinMat;
            coin.AddComponent<CoinPickup>();
        }
    }

    void BuildTouchUI(PlayerController player)
    {
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject events = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        GameObject canvasObject = new GameObject("RUN CICAAK Touch UI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObject.AddComponent<RunEndOverlay>();
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        RectTransform joystick = MakePanel(canvasObject.transform, "Joystick Base", new Vector2(0.15f, 0.18f), new Vector2(0.15f, 0.18f), new Vector2(180, 180), new Color(1, 1, 1, 0.22f));
        RectTransform handle = MakePanel(joystick, "Joystick Handle", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(78, 78), new Color(1, 1, 1, 0.72f));
        MobileJoystick stick = joystick.gameObject.AddComponent<MobileJoystick>();
        stick.background = joystick;
        stick.handle = handle;
        stick.player = player;

        CreateActionButton(canvasObject.transform, "SPRINT", MobileActionButton.ActionType.Sprint, new Vector2(0.88f, 0.18f), player);
        CreateActionButton(canvasObject.transform, "JUMP", MobileActionButton.ActionType.Jump, new Vector2(0.92f, 0.34f), player);
        CreateActionButton(canvasObject.transform, "HIDE", MobileActionButton.ActionType.Hide, new Vector2(0.78f, 0.28f), player);

        GameObject hud = new GameObject("HUD");
        hud.transform.SetParent(canvasObject.transform, false);
        RunHUD binder = hud.AddComponent<RunHUD>();
        binder.scoreText = AddHudText(hud.transform, "SCORE", new Vector2(0.02f, 0.95f));
        binder.coinText = AddHudText(hud.transform, "COINS", new Vector2(0.02f, 0.90f));
        binder.timerText = AddHudText(hud.transform, "TIME", new Vector2(0.02f, 0.85f));
        binder.bestText = AddHudText(hud.transform, "BEST", new Vector2(0.02f, 0.80f));
        binder.levelText = AddHudText(hud.transform, "LEVEL", new Vector2(0.88f, 0.95f));
    }

    RectTransform MakePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 size, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = anchorMin;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = size;
        go.GetComponent<Image>().color = color;
        return rect;
    }

    void CreateActionButton(Transform parent, string label, MobileActionButton.ActionType type, Vector2 anchor, PlayerController player)
    {
        RectTransform rect = MakePanel(parent, label + " Button", anchor, anchor, new Vector2(132, 132), new Color(0.08f, 0.1f, 0.13f, 0.68f));
        MobileActionButton action = rect.gameObject.AddComponent<MobileActionButton>();
        action.actionType = type;
        action.player = player;

        GameObject textObject = new GameObject("Label", typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(rect, false);
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = textRect.offsetMax = Vector2.zero;
        Text text = textObject.GetComponent<Text>();
        text.text = label;
        text.alignment = TextAnchor.MiddleCenter;
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = 25;
        text.color = Color.white;
        text.raycastTarget = false;
    }

    Text AddHudText(Transform parent, string label, Vector2 anchor)
    {
        GameObject go = new GameObject(label + " Text", typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = anchor;
        rect.pivot = new Vector2(0, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(360, 48);
        Text text = go.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = 28;
        text.color = Color.white;
        text.text = label;
        text.raycastTarget = false;
        return text;
    }

    void CreateLighting()
    {
        if (FindObjectOfType<Light>() != null) return;
        GameObject lightObject = new GameObject("House Directional Light");
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.1f;
        lightObject.transform.rotation = Quaternion.Euler(50, -30, 0);
    }

    Material MakeMaterial(Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        Material material = new Material(shader);
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
        else if (material.HasProperty("_Color")) material.color = color;
        return material;
    }

    T CreateManager<T>(string objectName) where T : Component
    {
        GameObject go = new GameObject(objectName);
        return go.AddComponent<T>();
    }
}