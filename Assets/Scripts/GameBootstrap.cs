using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GameBootstrap : MonoBehaviour
{
    private static bool started;
    private static bool startAfterReload;

    public static bool ConsumeAutoStart()
    {
        bool value = startAfterReload;
        startAfterReload = false;
        return value;
    }

    public static void RestartMatch()
    {
        Time.timeScale = 1f;
        started = false;
        startAfterReload = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public static void ReturnToMenu()
    {
        Time.timeScale = 1f;
        started = false;
        startAfterReload = false;
        localWifiMode = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private static bool localWifiMode;

    public static void StartMatch()
    {
        // Preserve the selected local-Wi-Fi mode when restarting the current match.
        StartMatch(localWifiMode);
    }

    public static void StartLocalMultiplayer()
    {
        StartMatch(true);
    }

    private static void StartMatch(bool useLocalWifi)
    {
        if (started) return;
        started = true;
        localWifiMode = useLocalWifi;
        foreach (Canvas canvas in FindObjectsOfType<Canvas>()) canvas.gameObject.SetActive(false);
        new GameObject("ClassicMatch").AddComponent<GameBootstrap>();
    }

    private void Start()
    {
        gameObject.AddComponent<RuntimeOptimization>();
        if (FindObjectOfType<Light>() == null)
        {
            GameObject sun = new GameObject("Sun");
            Light light = sun.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            sun.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        RenderSettings.ambientLight = new Color(0.55f, 0.58f, 0.62f);
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "TrainingIsland";
        ground.transform.localScale = new Vector3(12f, 1f, 12f);
        ground.GetComponent<Renderer>().material.color = new Color(0.25f, 0.38f, 0.22f);

        CreateTrainingEnvironment();
        gameObject.AddComponent<TerrainMapDetails>();
        gameObject.AddComponent<ClassicBermudaLocations>();

        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.transform.position = new Vector3(0f, 1.1f, 0f);
        player.GetComponent<Collider>().enabled = false;
        CharacterController cc = player.AddComponent<CharacterController>();
        cc.height = 2f; cc.radius = 0.45f; cc.center = new Vector3(0f, 0f, 0f);
        player.AddComponent<PlayerMovement>();
        player.AddComponent<ClassicCharacterVisual>();
        player.AddComponent<HealthSystem>();
        TeamAffiliation.Assign(player, 0);
        player.AddComponent<PlayerCombat>();
        player.AddComponent<InventorySystem>();
        player.AddComponent<EmoteController>();
        player.AddComponent<ProgressionSystem>();
        player.AddComponent<ParachuteDeployment>();
        player.GetComponent<Renderer>().material.color = CharacterLoadout.GetPrimaryColor();

        GameObject cameraObject = Camera.main ? Camera.main.gameObject : new GameObject("Main Camera");
        Camera camera = cameraObject.GetComponent<Camera>();
        if (camera == null) camera = cameraObject.AddComponent<Camera>();
        camera.tag = "MainCamera";
        if (cameraObject.GetComponent<AudioListener>() == null) cameraObject.AddComponent<AudioListener>();
        FollowCamera follow = cameraObject.GetComponent<FollowCamera>();
        if (follow == null) follow = cameraObject.AddComponent<FollowCamera>();
        follow.target = player.transform;
        if (cameraObject.GetComponent<TouchCameraLook>() == null) cameraObject.AddComponent<TouchCameraLook>();
        if (cameraObject.GetComponent<WeaponView>() == null) cameraObject.AddComponent<WeaponView>();
        cameraObject.transform.position = player.transform.position + follow.offset;

        CreateControls();

        // LAN networking is opt-in. Solo Offline never opens a socket or searches the network.
        if (localWifiMode)
        {
            LocalMultiplayerManager lanManager = gameObject.AddComponent<LocalMultiplayerManager>();
            lanManager.localPlayer = player.transform;
            LocalMultiplayerUI.OpenOnStart = true;
            LocalMultiplayerUI lanUI = gameObject.AddComponent<LocalMultiplayerUI>();
            lanUI.manager = lanManager;
        }

        CreateVehicle(new Vector3(10f, 0.7f, -12f));
        CreateWeaponPickups();
        BattleRoyaleMatchManager match = gameObject.AddComponent<BattleRoyaleMatchManager>();
        match.botCount = 49;
        match.Initialize(player.transform);
    }

    private void CreateVehicle(Vector3 position)
    {
        GameObject vehicle = GameObject.CreatePrimitive(PrimitiveType.Cube);
        vehicle.name = "ClassicJeep";
        vehicle.transform.position = position;
        vehicle.transform.localScale = new Vector3(2.2f, 1.1f, 3.8f);
        vehicle.GetComponent<Renderer>().material.color = new Color(0.22f, 0.31f, 0.18f);
        vehicle.AddComponent<SimpleVehicleController>();
        GameObject cabin = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cabin.name = "JeepCabin";
        cabin.transform.SetParent(vehicle.transform, false);
        cabin.transform.localPosition = new Vector3(0f, 0.85f, -0.15f);
        cabin.transform.localScale = new Vector3(0.82f, 0.8f, 0.55f);
        cabin.GetComponent<Renderer>().material.color = new Color(0.12f, 0.18f, 0.12f);
        cabin.GetComponent<Collider>().enabled = false;
    }

    private void CreateWeaponPickups()
    {
        for (int i = 0; i < 8; i++)
        {
            Vector2 p = Random.insideUnitCircle * 30f;
            GameObject pickup = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pickup.name = "WeaponAmmoPickup_" + (i + 1);
            pickup.transform.position = new Vector3(p.x, 0.55f, p.y);
            pickup.transform.localScale = new Vector3(0.8f, 0.5f, 0.8f);
            WeaponPickup weapon = pickup.AddComponent<WeaponPickup>();
            string[] types = { "SMG", "SHOTGUN", "SNIPER", "RIFLE" };
            weapon.weaponName = types[i % types.Length];
            weapon.ammoAmount = 30;
            pickup.GetComponent<Renderer>().material.color = i % 4 == 2 ? new Color(0.22f, 0.48f, 0.9f) : new Color(0.95f, 0.75f, 0.12f);
        }
    }

    // Lightweight, procedural cover and landmarks for the first training island.
    // Uses built-in primitives only so the prototype needs no external assets.
    private void CreateTrainingEnvironment()
    {
        Material buildingMat = new Material(Shader.Find("Standard"));
        buildingMat.color = new Color(0.62f, 0.55f, 0.43f);
        Material roofMat = new Material(Shader.Find("Standard"));
        roofMat.color = new Color(0.28f, 0.20f, 0.16f);
        Material trunkMat = new Material(Shader.Find("Standard"));
        trunkMat.color = new Color(0.30f, 0.19f, 0.10f);
        Material leafMat = new Material(Shader.Find("Standard"));
        leafMat.color = new Color(0.16f, 0.36f, 0.15f);
        Material rockMat = new Material(Shader.Find("Standard"));
        rockMat.color = new Color(0.38f, 0.40f, 0.39f);

        // Small village buildings around the playable center; leave the center open.
        Vector3[] housePositions = {
            new Vector3(-24f, 0f, 15f), new Vector3(23f, 0f, 18f),
            new Vector3(-25f, 0f, -20f), new Vector3(25f, 0f, -22f)
        };
        for (int i = 0; i < housePositions.Length; i++)
        {
            GameObject house = GameObject.CreatePrimitive(PrimitiveType.Cube);
            house.name = "Village_House_" + (i + 1);
            house.transform.position = housePositions[i] + Vector3.up * 2f;
            house.transform.localScale = new Vector3(8f, 4f, 7f);
            house.GetComponent<Renderer>().material = buildingMat;

            GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roof.name = "House_Roof_" + (i + 1);
            roof.transform.position = housePositions[i] + Vector3.up * 4.35f;
            roof.transform.localScale = new Vector3(8.8f, 0.8f, 7.8f);
            roof.GetComponent<Renderer>().material = roofMat;
        }

        // Low cover pieces give the player and bots simple line-of-sight obstacles.
        for (int i = 0; i < 8; i++)
        {
            float angle = i * Mathf.PI * 0.25f;
            Vector3 position = new Vector3(Mathf.Cos(angle) * 13f, 0.8f, Mathf.Sin(angle) * 13f);
            GameObject cover = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cover.name = "Battle_Cover_" + (i + 1);
            cover.transform.position = position;
            cover.transform.rotation = Quaternion.Euler(0f, -i * 22.5f, 0f);
            cover.transform.localScale = new Vector3(3.5f, 1.6f, 0.8f);
            cover.GetComponent<Renderer>().material = rockMat;
        }

        // Trees and rocks are placed outside the central combat ring.
        for (int i = 0; i < 18; i++)
        {
            float angle = i * Mathf.PI * 2f / 18f;
            float radius = 34f + (i % 3) * 3f;
            Vector3 basePosition = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
            GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = "Island_Tree_Trunk_" + (i + 1);
            trunk.transform.position = basePosition + Vector3.up * 1.5f;
            trunk.transform.localScale = new Vector3(0.55f, 1.5f, 0.55f);
            trunk.GetComponent<Renderer>().material = trunkMat;

            GameObject leaves = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            leaves.name = "Island_Tree_Canopy_" + (i + 1);
            leaves.transform.position = basePosition + Vector3.up * 3.7f;
            leaves.transform.localScale = new Vector3(3.2f, 3.8f, 3.2f);
            leaves.GetComponent<Renderer>().material = leafMat;
        }

        for (int i = 0; i < 10; i++)
        {
            float angle = i * Mathf.PI * 2f / 10f + 0.3f;
            Vector3 position = new Vector3(Mathf.Cos(angle) * 29f, 0.65f, Mathf.Sin(angle) * 29f);
            GameObject rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rock.name = "Island_Rock_" + (i + 1);
            rock.transform.position = position;
            rock.transform.localScale = new Vector3(2.4f, 1.3f, 1.8f);
            rock.GetComponent<Renderer>().material = rockMat;
        }
    }

    private void CreateControls()
    {
        EventSystem es = FindObjectOfType<EventSystem>();
        if (es == null) new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        GameObject canvasObject = new GameObject("MobileControls", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);

        CreateJoystick(canvas.transform);
        CreateFireButton(canvas.transform);
        CreateReloadButton(canvas.transform);
        CreateWeaponSwitchButton(canvas.transform);
        CreateWeaponDisplay(canvas.transform);
        CreateAmmoDisplay(canvas.transform);
        CreateHealthDisplay(canvas.transform);
        CreateMedkitButton(canvas.transform);
        CreateVehicleButton(canvas.transform);
        CreateDropWeaponButton(canvas.transform);
        CreateEmoteButton(canvas.transform);
        CreateSprintButton(canvas.transform);
        CreateJumpButton(canvas.transform);

        GameObject hint = new GameObject("ControlHint", typeof(RectTransform), typeof(Text));
        hint.transform.SetParent(canvas.transform, false);
        RectTransform rect = hint.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.90f); rect.anchorMax = new Vector2(0.5f, 0.90f);
        rect.sizeDelta = new Vector2(600f, 34f);
        Text text = hint.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.text = "TRAINING MATCH  |  LEFT: MOVE JOYSTICK  |  RIGHT: DRAG TO LOOK";
        text.fontSize = 16; text.alignment = TextAnchor.MiddleCenter; text.color = Color.white;
    }


    private void CreateJoystick(Transform parent)
    {
        GameObject baseObject = new GameObject("MOVE_JOYSTICK", typeof(RectTransform), typeof(Image), typeof(MobileJoystick));
        baseObject.transform.SetParent(parent, false);
        RectTransform baseRect = baseObject.GetComponent<RectTransform>();
        baseRect.anchorMin = new Vector2(0.16f, 0.19f);
        baseRect.anchorMax = new Vector2(0.16f, 0.19f);
        baseRect.sizeDelta = new Vector2(180f, 180f);
        Image baseImage = baseObject.GetComponent<Image>();
        baseImage.color = new Color(0.05f, 0.08f, 0.12f, 0.38f);

        GameObject handleObject = new GameObject("HANDLE", typeof(RectTransform), typeof(Image));
        handleObject.transform.SetParent(baseObject.transform, false);
        RectTransform handleRect = handleObject.GetComponent<RectTransform>();
        handleRect.anchorMin = new Vector2(0.5f, 0.5f);
        handleRect.anchorMax = new Vector2(0.5f, 0.5f);
        handleRect.sizeDelta = new Vector2(76f, 76f);
        Image handleImage = handleObject.GetComponent<Image>();
        handleImage.color = new Color(0.8f, 0.85f, 0.9f, 0.78f);
        handleImage.raycastTarget = false;
        baseObject.GetComponent<MobileJoystick>().handle = handleRect;
    }

    private void CreateHealthDisplay(Transform parent)
    {
        GameObject healthObject = new GameObject("HealthDisplay", typeof(RectTransform), typeof(Text));
        healthObject.transform.SetParent(parent, false);
        RectTransform rect = healthObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.83f);
        rect.anchorMax = new Vector2(0.5f, 0.83f);
        rect.sizeDelta = new Vector2(360f, 48f);
        Text label = healthObject.GetComponent<Text>();
        label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        label.fontSize = 26;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = new Color(0.35f, 1f, 0.4f);
        HealthSystem health = FindObjectOfType<PlayerCombat>().GetComponent<HealthSystem>();
        label.text = "HP: 100 / 100";
        health.OnHealthChanged += (current, maximum) =>
        {
            label.text = "HP: " + Mathf.CeilToInt(current) + " / " + Mathf.CeilToInt(maximum);
            label.color = current <= maximum * 0.25f ? Color.red : new Color(0.35f, 1f, 0.4f);
        };
        health.OnDied += () => { label.text = "YOU ARE ELIMINATED"; label.color = Color.red; };
    }

    private void CreateFireButton(Transform parent)
    {
        GameObject buttonObject = new GameObject("FIRE", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.87f, 0.18f);
        rect.anchorMax = new Vector2(0.87f, 0.18f);
        rect.sizeDelta = new Vector2(125f, 100f);
        buttonObject.GetComponent<Image>().color = new Color(0.8f, 0.12f, 0.08f, 0.85f);
        Text label = new GameObject("Label", typeof(RectTransform), typeof(Text)).GetComponent<Text>();
        label.transform.SetParent(buttonObject.transform, false);
        label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        label.text = "FIRE";
        label.fontSize = 24;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = Color.white;
        RectTransform labelRect = label.rectTransform;
        labelRect.anchorMin = Vector2.zero; labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero; labelRect.offsetMax = Vector2.zero;
        buttonObject.GetComponent<Button>().onClick.AddListener(() =>
        {
            PlayerCombat combat = FindObjectOfType<PlayerCombat>();
            if (combat != null) combat.Fire();
        });
    }


    private void CreateAmmoDisplay(Transform parent)
    {
        GameObject ammoObject = new GameObject("AmmoDisplay", typeof(RectTransform), typeof(Text));
        ammoObject.transform.SetParent(parent, false);
        RectTransform rect = ammoObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.87f, 0.42f);
        rect.anchorMax = new Vector2(0.87f, 0.42f);
        rect.sizeDelta = new Vector2(220f, 48f);

        Text label = ammoObject.GetComponent<Text>();
        label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        label.fontSize = 24;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = Color.white;

        PlayerCombat combat = FindObjectOfType<PlayerCombat>();
        if (combat == null) return;
        label.text = "AMMO: 30 / 90";
        combat.OnAmmoChanged += (current, reserve) =>
        {
            label.text = "AMMO: " + current + " / " + reserve;
            label.color = current <= 5 ? new Color(1f, 0.35f, 0.25f) : Color.white;
        };
        combat.OnReloadStateChanged += (reloading) =>
        {
            if (reloading) label.text = "RELOADING...";
        };
    }

    private void CreateReloadButton(Transform parent)
    {
        GameObject buttonObject = new GameObject("RELOAD", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.87f, 0.32f);
        rect.anchorMax = new Vector2(0.87f, 0.32f);
        rect.sizeDelta = new Vector2(125f, 70f);
        buttonObject.GetComponent<Image>().color = new Color(0.12f, 0.28f, 0.55f, 0.88f);

        Text label = new GameObject("Label", typeof(RectTransform), typeof(Text)).GetComponent<Text>();
        label.transform.SetParent(buttonObject.transform, false);
        label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        label.text = "RELOAD";
        label.fontSize = 20;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = Color.white;
        RectTransform labelRect = label.rectTransform;
        labelRect.anchorMin = Vector2.zero; labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero; labelRect.offsetMax = Vector2.zero;

        buttonObject.GetComponent<Button>().onClick.AddListener(() =>
        {
            PlayerCombat combat = FindObjectOfType<PlayerCombat>();
            if (combat != null) combat.Reload();
        });
    }


    private void CreateWeaponDisplay(Transform parent)
    {
        GameObject weaponObject = new GameObject("WeaponDisplay", typeof(RectTransform), typeof(Text));
        weaponObject.transform.SetParent(parent, false);
        RectTransform rect = weaponObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.87f, 0.50f);
        rect.anchorMax = new Vector2(0.87f, 0.50f);
        rect.sizeDelta = new Vector2(220f, 44f);

        Text label = weaponObject.GetComponent<Text>();
        label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        label.fontSize = 23;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = new Color(1f, 0.85f, 0.3f);

        PlayerCombat combat = FindObjectOfType<PlayerCombat>();
        if (combat == null) return;
        label.text = "RIFLE";
        combat.OnWeaponChanged += (weaponName) => { label.text = weaponName; };
    }

    private void CreateWeaponSwitchButton(Transform parent)
    {
        GameObject buttonObject = new GameObject("SWAP", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.87f, 0.61f);
        rect.anchorMax = new Vector2(0.87f, 0.61f);
        rect.sizeDelta = new Vector2(125f, 64f);
        buttonObject.GetComponent<Image>().color = new Color(0.28f, 0.32f, 0.18f, 0.9f);

        Text label = new GameObject("Label", typeof(RectTransform), typeof(Text)).GetComponent<Text>();
        label.transform.SetParent(buttonObject.transform, false);
        label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        label.text = "SWAP";
        label.fontSize = 20;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = Color.white;
        RectTransform labelRect = label.rectTransform;
        labelRect.anchorMin = Vector2.zero; labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero; labelRect.offsetMax = Vector2.zero;

        buttonObject.GetComponent<Button>().onClick.AddListener(() =>
        {
            PlayerCombat combat = FindObjectOfType<PlayerCombat>();
            if (combat != null) combat.SwitchWeapon();
        });
    }

    private void CreateMedkitButton(Transform parent)
    {
        CreateActionButton("MEDKIT", parent, new Vector2(0.72f, 0.18f), () =>
        {
            InventorySystem inventory = FindObjectOfType<InventorySystem>();
            if (inventory != null) inventory.UseMedkit();
        });
    }

    private void CreateVehicleButton(Transform parent)
    {
        CreateActionButton("DRIVE", parent, new Vector2(0.72f, 0.31f), SimpleVehicleController.ToggleNearest);
    }

    private void CreateActionButton(string title, Transform parent, Vector2 anchor, UnityEngine.Events.UnityAction action)
    {
        GameObject buttonObject = new GameObject(title, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = anchor; rect.anchorMax = anchor; rect.sizeDelta = new Vector2(112f, 62f);
        buttonObject.GetComponent<Image>().color = new Color(0.12f, 0.24f, 0.32f, 0.88f);
        Text label = new GameObject("Label", typeof(RectTransform), typeof(Text)).GetComponent<Text>();
        label.transform.SetParent(buttonObject.transform, false);
        label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        label.text = title; label.fontSize = 18; label.alignment = TextAnchor.MiddleCenter; label.color = Color.white;
        label.rectTransform.anchorMin = Vector2.zero; label.rectTransform.anchorMax = Vector2.one;
        label.rectTransform.offsetMin = Vector2.zero; label.rectTransform.offsetMax = Vector2.zero;
        buttonObject.GetComponent<Button>().onClick.AddListener(action);
    }

    private void CreateSprintButton(Transform parent)
    {
        CreateActionButton("SPRINT", parent, new Vector2(0.16f, 0.43f), () =>
            PlayerMovement.SetMobileSprint(!PlayerMovement.MobileSprint));
    }

    private void CreateJumpButton(Transform parent)
    {
        CreateActionButton("JUMP", parent, new Vector2(0.16f, 0.53f), PlayerMovement.RequestJump);
    }

    private void CreateDropWeaponButton(Transform parent)
    {
        CreateActionButton("DROP", parent, new Vector2(0.72f, 0.44f), () =>
        {
            PlayerCombat combat = FindObjectOfType<PlayerCombat>();
            if (combat != null) combat.DropCurrentWeapon();
        });
    }

    private void CreateEmoteButton(Transform parent)
    {
        CreateActionButton("EMOTE", parent, new Vector2(0.72f, 0.57f), () =>
        {
            EmoteController emote = FindObjectOfType<EmoteController>();
            if (emote != null) emote.PlayEmote(Random.Range(0, 3));
        });
    }

    private void CreateMoveButton(Transform parent, string label, Vector2 anchor, Vector2 direction)
    {
        GameObject buttonObject = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button), typeof(EventTrigger));
        buttonObject.transform.SetParent(parent, false);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = anchor; rect.anchorMax = anchor; rect.sizeDelta = new Vector2(105f, 85f);
        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.05f, 0.08f, 0.12f, 0.65f);
        Text labelText = new GameObject("Label", typeof(RectTransform), typeof(Text)).GetComponent<Text>();
        labelText.transform.SetParent(buttonObject.transform, false);
        labelText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        labelText.text = label; labelText.fontSize = 18; labelText.alignment = TextAnchor.MiddleCenter; labelText.color = Color.white;
        RectTransform labelRect = labelText.rectTransform; labelRect.anchorMin = Vector2.zero; labelRect.anchorMax = Vector2.one; labelRect.offsetMin = Vector2.zero; labelRect.offsetMax = Vector2.zero;

        EventTrigger trigger = buttonObject.GetComponent<EventTrigger>();
        EventTrigger.Entry down = new EventTrigger.Entry(); down.eventID = EventTriggerType.PointerDown;
        down.callback.AddListener((data) => PlayerMovement.SetMobileMove(direction));
        trigger.triggers.Add(down);
        EventTrigger.Entry up = new EventTrigger.Entry(); up.eventID = EventTriggerType.PointerUp;
        up.callback.AddListener((data) => PlayerMovement.SetMobileMove(Vector2.zero));
        trigger.triggers.Add(up);
        EventTrigger.Entry exit = new EventTrigger.Entry(); exit.eventID = EventTriggerType.PointerExit;
        exit.callback.AddListener((data) => PlayerMovement.SetMobileMove(Vector2.zero));
        trigger.triggers.Add(exit);
    }
}
