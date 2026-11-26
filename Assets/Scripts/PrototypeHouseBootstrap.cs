using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class PrototypeHouseBootstrap : MonoBehaviour
{
    [Header("Prototype blockout")]
    public bool buildOnStart = true;
    public float roomSize = 10f;

    private Material floorMat, wallMat, furnitureMat, geckoMat, coinMat;
    private Material eyeMat, pupilMat, humanSkinMat, shirtMat, pantsMat, hairMat;
    private Material fabricMat, applianceMat, mattressMat, boxMat;

    void Start()
    {
        if (buildOnStart) Build();
    }

    [ContextMenu("Build Prototype House")]
    public void Build()
    {
        ConfigureMobilePerformance();
        if (FindObjectOfType<PlayerController>() != null)
        {
            Debug.LogWarning("RUN CICAAK: Player sudah ada. Bootstrap dilewati agar tidak membuat duplikat.");
            return;
        }

        floorMat = MakeMaterial(new Color(0.48f, 0.43f, 0.36f));
        wallMat = MakeMaterial(new Color(0.78f, 0.75f, 0.68f));
        furnitureMat = MakeMaterial(new Color(0.28f, 0.19f, 0.13f));
        geckoMat = MakeMaterial(new Color(0.25f, 0.72f, 0.28f));
        coinMat = MakeMaterial(new Color(1f, 0.72f, 0.08f));
        eyeMat = MakeMaterial(new Color(0.96f, 0.98f, 1f));
        pupilMat = MakeMaterial(new Color(0.025f, 0.035f, 0.04f));
        humanSkinMat = MakeMaterial(new Color(0.78f, 0.55f, 0.39f));
        shirtMat = MakeMaterial(new Color(0.16f, 0.3f, 0.62f));
        pantsMat = MakeMaterial(new Color(0.12f, 0.15f, 0.2f));
        hairMat = MakeMaterial(new Color(0.08f, 0.055f, 0.04f));
        fabricMat = MakeMaterial(new Color(0.22f, 0.3f, 0.38f));
        applianceMat = MakeMaterial(new Color(0.72f, 0.76f, 0.78f));
        mattressMat = MakeMaterial(new Color(0.83f, 0.82f, 0.75f));
        boxMat = MakeMaterial(new Color(0.42f, 0.29f, 0.18f));

        BuildRoom("RUANG TAMU", new Vector3(0, 0, 0));
        BuildRoom("DAPUR", new Vector3(12, 0, 0));
        BuildRoom("KAMAR", new Vector3(0, 0, 12));
        BuildRoom("GUDANG", new Vector3(12, 0, 12));
        CreateCorridor(new Vector3(6, -0.15f, 0), new Vector3(2.4f, 0.3f, 2.4f));
        CreateCorridor(new Vector3(6, -0.15f, 12), new Vector3(2.4f, 0.3f, 2.4f));
        CreateCorridor(new Vector3(0, -0.15f, 6), new Vector3(2.4f, 0.3f, 2.4f));
        CreateCorridor(new Vector3(12, -0.15f, 6), new Vector3(2.4f, 0.3f, 2.4f));

        GameObject player = new GameObject("Gecko Player (Prototype)");
        player.transform.position = Vector3.zero;
        CharacterController playerMotor = player.AddComponent<CharacterController>();
        playerMotor.height = 1.1f;
        playerMotor.radius = 0.34f;
        playerMotor.center = new Vector3(0f, 0.55f, 0f);
        PlayerController playerController = player.AddComponent<PlayerController>();
        player.AddComponent<CatchSequence>();
        GeckoSkinApplier skinApplier = player.AddComponent<GeckoSkinApplier>();
        skinApplier.targetRenderers = BuildGeckoModel(player.transform);

        GeckoProceduralAnimation geckoAnimation = player.AddComponent<GeckoProceduralAnimation>();
        geckoAnimation.player = playerController;
        geckoAnimation.body = player.transform.Find("Gecko Body");
        geckoAnimation.frontLeftLeg = player.transform.Find("Front Left Leg");
        geckoAnimation.frontRightLeg = player.transform.Find("Front Right Leg");
        geckoAnimation.backLeftLeg = player.transform.Find("Back Left Leg");
        geckoAnimation.backRightLeg = player.transform.Find("Back Right Leg");
        geckoAnimation.tailSegments = new Transform[]
        {
            player.transform.Find("Tail Base"),
            player.transform.Find("Tail Mid"),
            player.transform.Find("Tail Tip")
        };

        foreach (HidingSpot spot in FindObjectsOfType<HidingSpot>())
            spot.SetPlayer(playerController);

        GameObject human = new GameObject("Human Chaser (Prototype)");
        human.transform.position = new Vector3(16, 0, -3);
        CharacterController humanMotor = human.AddComponent<CharacterController>();
        humanMotor.height = 2f;
        humanMotor.radius = 0.42f;
        humanMotor.center = new Vector3(0f, 1f, 0f);
        BuildHumanModel(human.transform);
        HumanChaserAI ai = human.AddComponent<HumanChaserAI>();
        ai.target = player.transform;
        ai.walkSpeed = 4.0f;

        HumanProceduralAnimation humanAnimation = human.AddComponent<HumanProceduralAnimation>();
        humanAnimation.chaser = ai;
        humanAnimation.leftArm = human.transform.Find("Human Left Arm");
        humanAnimation.rightArm = human.transform.Find("Human Right Arm");
        humanAnimation.torso = human.transform.Find("Human Torso");

        GameObject cameraObject = Camera.main != null ? Camera.main.gameObject : new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0, 4.2f, -6f);
        CameraFollow follow = cameraObject.GetComponent<CameraFollow>();
        if (follow == null) follow = cameraObject.AddComponent<CameraFollow>();
        follow.target = player.transform;
        playerController.cameraTransform = cameraObject.transform;

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

    Renderer[] BuildGeckoModel(Transform root)
    {
        List<Renderer> skinParts = new List<Renderer>();
        skinParts.Add(CreatePart(root, "Gecko Body", PrimitiveType.Sphere, new Vector3(0, 0.43f, 0), new Vector3(0.58f, 0.34f, 0.82f), geckoMat));
        skinParts.Add(CreatePart(root, "Gecko Head", PrimitiveType.Sphere, new Vector3(0, 0.62f, 0.48f), new Vector3(0.44f, 0.34f, 0.42f), geckoMat));
        skinParts.Add(CreatePart(root, "Gecko Muzzle", PrimitiveType.Sphere, new Vector3(0, 0.53f, 0.72f), new Vector3(0.3f, 0.16f, 0.25f), geckoMat));

        skinParts.Add(CreatePart(root, "Tail Base", PrimitiveType.Sphere, new Vector3(0, 0.36f, -0.48f), new Vector3(0.34f, 0.25f, 0.48f), geckoMat));
        skinParts.Add(CreatePart(root, "Tail Mid", PrimitiveType.Sphere, new Vector3(0, 0.29f, -0.82f), new Vector3(0.24f, 0.18f, 0.42f), geckoMat));
        skinParts.Add(CreatePart(root, "Tail Tip", PrimitiveType.Sphere, new Vector3(0, 0.22f, -1.13f), new Vector3(0.14f, 0.12f, 0.35f), geckoMat));

        skinParts.Add(CreatePart(root, "Front Left Leg", PrimitiveType.Capsule, new Vector3(-0.29f, 0.22f, 0.34f), new Vector3(0.17f, 0.22f, 0.18f), geckoMat));
        skinParts.Add(CreatePart(root, "Front Right Leg", PrimitiveType.Capsule, new Vector3(0.29f, 0.22f, 0.34f), new Vector3(0.17f, 0.22f, 0.18f), geckoMat));
        skinParts.Add(CreatePart(root, "Back Left Leg", PrimitiveType.Capsule, new Vector3(-0.29f, 0.2f, -0.36f), new Vector3(0.18f, 0.23f, 0.19f), geckoMat));
        skinParts.Add(CreatePart(root, "Back Right Leg", PrimitiveType.Capsule, new Vector3(0.29f, 0.2f, -0.36f), new Vector3(0.18f, 0.23f, 0.19f), geckoMat));

        CreatePart(root, "Left Eye", PrimitiveType.Sphere, new Vector3(-0.16f, 0.78f, 0.65f), new Vector3(0.14f, 0.15f, 0.12f), eyeMat);
        CreatePart(root, "Right Eye", PrimitiveType.Sphere, new Vector3(0.16f, 0.78f, 0.65f), new Vector3(0.14f, 0.15f, 0.12f), eyeMat);
        CreatePart(root, "Left Pupil", PrimitiveType.Sphere, new Vector3(-0.16f, 0.78f, 0.75f), new Vector3(0.065f, 0.075f, 0.055f), pupilMat);
        CreatePart(root, "Right Pupil", PrimitiveType.Sphere, new Vector3(0.16f, 0.78f, 0.75f), new Vector3(0.065f, 0.075f, 0.055f), pupilMat);

        return skinParts.ToArray();
    }

    void BuildHumanModel(Transform root)
    {
        CreatePart(root, "Human Torso", PrimitiveType.Capsule, new Vector3(0, 1.15f, 0), new Vector3(0.42f, 0.48f, 0.3f), shirtMat);
        CreatePart(root, "Human Head", PrimitiveType.Sphere, new Vector3(0, 1.95f, 0), new Vector3(0.38f, 0.4f, 0.36f), humanSkinMat);
        CreatePart(root, "Human Hair", PrimitiveType.Sphere, new Vector3(0, 2.12f, -0.02f), new Vector3(0.39f, 0.2f, 0.37f), hairMat);
        CreatePart(root, "Human Left Arm", PrimitiveType.Capsule, new Vector3(-0.49f, 1.2f, 0), new Vector3(0.18f, 0.43f, 0.18f), humanSkinMat);
        CreatePart(root, "Human Right Arm", PrimitiveType.Capsule, new Vector3(0.49f, 1.2f, 0), new Vector3(0.18f, 0.43f, 0.18f), humanSkinMat);
        CreatePart(root, "Human Left Leg", PrimitiveType.Capsule, new Vector3(-0.2f, 0.43f, 0), new Vector3(0.2f, 0.4f, 0.22f), pantsMat);
        CreatePart(root, "Human Right Leg", PrimitiveType.Capsule, new Vector3(0.2f, 0.43f, 0), new Vector3(0.2f, 0.4f, 0.22f), pantsMat);
    }

    Renderer CreatePart(Transform parent, string partName, PrimitiveType primitive, Vector3 localPosition, Vector3 localScale, Material material)
    {
        GameObject part = GameObject.CreatePrimitive(primitive);
        part.name = partName;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = localPosition;
        part.transform.localScale = localScale;
        Collider partCollider = part.GetComponent<Collider>();
        if (partCollider != null) Destroy(partCollider);
        Renderer renderer = part.GetComponent<Renderer>();
        renderer.sharedMaterial = material;
        return renderer;
    }

    void BuildRoom(string label, Vector3 center)
    {
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = label + " Floor";
        floor.transform.position = center + new Vector3(0, -0.15f, 0);
        floor.transform.localScale = new Vector3(roomSize, 0.3f, roomSize);
        floor.GetComponent<Renderer>().sharedMaterial = floorMat;

        CreateWallWithOpening(label + " Wall North", center, true, roomSize / 2, 2.4f);
        CreateWallWithOpening(label + " Wall South", center, true, -roomSize / 2, 2.4f);
        CreateWallWithOpening(label + " Wall East", center, false, roomSize / 2, 2.4f);
        CreateWallWithOpening(label + " Wall West", center, false, -roomSize / 2, 2.4f);

        Vector3 furniturePosition = center + new Vector3(2.4f, 0.65f, 1.8f);
        GameObject cabinet = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cabinet.name = label + " Hiding Cabinet";
        cabinet.transform.position = furniturePosition;
        cabinet.transform.localScale = new Vector3(1.7f, 1.3f, 1.1f);
        cabinet.GetComponent<Renderer>().sharedMaterial = furnitureMat;

        GameObject hideAnchor = new GameObject("Hide Position");
        hideAnchor.transform.position = new Vector3(furniturePosition.x - 1.1f, 0f, furniturePosition.z);
        HidingSpot hiding = cabinet.AddComponent<HidingSpot>();
        hiding.hidePosition = hideAnchor.transform;
        hiding.interactDistance = 2.5f;

        GameObject table = GameObject.CreatePrimitive(PrimitiveType.Cube);
        table.name = label + " Table";
        table.transform.position = center + new Vector3(-2.3f, 0.65f, -1.8f);
        table.transform.localScale = new Vector3(2.1f, 1.3f, 1.5f);
        table.GetComponent<Renderer>().sharedMaterial = furnitureMat;

        BuildRoomProps(label, center);
    }

    void BuildRoomProps(string label, Vector3 center)
    {
        if (label == "RUANG TAMU")
        {
            CreateProp(label + " Sofa Base", center + new Vector3(-2.8f, 0.38f, 2.7f), new Vector3(3.2f, 0.75f, 1.1f), fabricMat);
            CreateProp(label + " Sofa Back", center + new Vector3(-2.8f, 0.95f, 3.15f), new Vector3(3.2f, 0.75f, 0.25f), fabricMat);
            CreateProp(label + " Sofa Arm L", center + new Vector3(-4.25f, 0.75f, 2.7f), new Vector3(0.3f, 0.75f, 1.1f), furnitureMat);
            CreateProp(label + " Sofa Arm R", center + new Vector3(-1.35f, 0.75f, 2.7f), new Vector3(0.3f, 0.75f, 1.1f), furnitureMat);
        }
        else if (label == "DAPUR")
        {
            CreateProp(label + " Kitchen Counter", center + new Vector3(-2.8f, 0.62f, 2.8f), new Vector3(3.2f, 1.2f, 1.1f), applianceMat);
            CreateProp(label + " Refrigerator", center + new Vector3(-3.6f, 1.15f, -2.8f), new Vector3(1.25f, 2.3f, 1.15f), applianceMat);
            CreateProp(label + " Upper Cabinet", center + new Vector3(1.6f, 2.15f, 4.45f), new Vector3(2.2f, 0.8f, 0.55f), furnitureMat);
        }
        else if (label == "KAMAR")
        {
            CreateProp(label + " Bed Base", center + new Vector3(-2.7f, 0.32f, 2.7f), new Vector3(3.4f, 0.65f, 2.5f), furnitureMat);
            CreateProp(label + " Mattress", center + new Vector3(-2.7f, 0.72f, 2.7f), new Vector3(3.25f, 0.22f, 2.35f), mattressMat);
            CreateProp(label + " Pillow", center + new Vector3(-2.7f, 0.9f, 3.55f), new Vector3(1.05f, 0.2f, 0.55f), eyeMat);
        }
        else if (label == "GUDANG")
        {
            CreateProp(label + " Storage Box A", center + new Vector3(-3.2f, 0.5f, 2.8f), new Vector3(1.2f, 1f, 1.2f), boxMat);
            CreateProp(label + " Storage Box B", center + new Vector3(-2.5f, 1.45f, 2.8f), new Vector3(1.1f, 0.85f, 1.1f), boxMat);
            CreateProp(label + " Storage Box C", center + new Vector3(-0.9f, 0.4f, 3.1f), new Vector3(1.5f, 0.8f, 1.2f), boxMat);
            CreateProp(label + " Metal Shelf", center + new Vector3(3.6f, 1.2f, -2.8f), new Vector3(0.35f, 2.4f, 3.2f), applianceMat);
        }
    }

    void CreateProp(string name, Vector3 position, Vector3 scale, Material material)
    {
        GameObject prop = GameObject.CreatePrimitive(PrimitiveType.Cube);
        prop.name = name;
        prop.transform.position = position;
        prop.transform.localScale = scale;
        prop.GetComponent<Renderer>().sharedMaterial = material;
    }

    void CreateCorridor(Vector3 position, Vector3 scale)
    {
        GameObject corridor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        corridor.name = "Room Connector Floor";
        corridor.transform.position = position;
        corridor.transform.localScale = scale;
        corridor.GetComponent<Renderer>().sharedMaterial = floorMat;
    }

    void CreateEscapeExit(Vector3 position)
    {
        GameObject exit = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        exit.name = "Escape Exit";
        exit.transform.position = position;
        exit.transform.localScale = new Vector3(1.8f, 0.12f, 1.8f);
        exit.GetComponent<Renderer>().sharedMaterial = coinMat;
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
        wall.GetComponent<Renderer>().sharedMaterial = wallMat;
    }

    void CreateCoins(Vector3 start)
    {
        for (int i = 0; i < 4; i++)
        {
            GameObject coin = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            coin.name = "Coin";
            coin.transform.position = start + new Vector3(i * 0.65f, 0, 0);
            coin.transform.localScale = Vector3.one * 0.35f;
            coin.GetComponent<Renderer>().sharedMaterial = coinMat;
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
        canvasObject.AddComponent<PauseMenuController>();
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
        CreateActionButton(canvasObject.transform, "PAUSE", MobileActionButton.ActionType.Pause, new Vector2(0.96f, 0.78f), player);

        GameObject hud = new GameObject("HUD");
        hud.transform.SetParent(canvasObject.transform, false);
        RunHUD binder = hud.AddComponent<RunHUD>();
        binder.scoreText = AddHudText(hud.transform, "SCORE", new Vector2(0.02f, 0.95f));
        binder.coinText = AddHudText(hud.transform, "COINS", new Vector2(0.02f, 0.90f));
        binder.timerText = AddHudText(hud.transform, "TIME", new Vector2(0.02f, 0.85f));
        binder.bestText = AddHudText(hud.transform, "BEST", new Vector2(0.02f, 0.80f));
        binder.levelText = AddHudText(hud.transform, "LEVEL", new Vector2(0.88f, 0.95f));
        binder.xpText = AddHudText(hud.transform, "XP", new Vector2(0.88f, 0.90f));
        CreateGameplayHint(canvasObject.transform);
    }

    void CreateGameplayHint(Transform parent)
    {
        GameObject go = new GameObject("Gameplay Hint", typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.88f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(1100, 58);

        Text text = go.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = 24;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = new Color(1f, 1f, 1f, 0.95f);
        text.text = "KUMPULKAN KOIN   •   HINDARI MANUSIA   •   CARI JALAN KELUAR";
        text.raycastTarget = false;
        go.AddComponent<GameplayHint>();
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
        rect.pivot = (label == "LEVEL" || label == "XP") ? new Vector2(1, 0.5f) : new Vector2(0, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(label == "LEVEL" ? 220 : 360, 48);
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


    void ConfigureMobilePerformance()
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        if (Application.isMobilePlatform)
        {
            QualitySettings.antiAliasing = 2;
            QualitySettings.shadows = ShadowQuality.HardOnly;
            QualitySettings.shadowResolution = ShadowResolution.Low;
        }
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