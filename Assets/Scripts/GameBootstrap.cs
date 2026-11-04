using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GameBootstrap : MonoBehaviour
{
    private static bool started;

    public static void StartMatch()
    {
        if (started) return;
        started = true;
        foreach (Canvas canvas in FindObjectsOfType<Canvas>()) canvas.gameObject.SetActive(false);
        new GameObject("ClassicMatch").AddComponent<GameBootstrap>();
    }

    private void Start()
    {
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

        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.transform.position = new Vector3(0f, 1.1f, 0f);
        player.GetComponent<Collider>().enabled = false;
        CharacterController cc = player.AddComponent<CharacterController>();
        cc.height = 2f; cc.radius = 0.45f; cc.center = new Vector3(0f, 0f, 0f);
        player.AddComponent<PlayerMovement>();
        player.AddComponent<HealthSystem>();
        player.AddComponent<PlayerCombat>();
        player.GetComponent<Renderer>().material.color = new Color(0.1f, 0.45f, 0.9f);

        GameObject cameraObject = Camera.main ? Camera.main.gameObject : new GameObject("Main Camera");
        Camera camera = cameraObject.GetComponent<Camera>();
        if (camera == null) camera = cameraObject.AddComponent<Camera>();
        camera.tag = "MainCamera";
        if (cameraObject.GetComponent<AudioListener>() == null) cameraObject.AddComponent<AudioListener>();
        FollowCamera follow = cameraObject.GetComponent<FollowCamera>();
        if (follow == null) follow = cameraObject.AddComponent<FollowCamera>();
        follow.target = player.transform;
        if (cameraObject.GetComponent<TouchCameraLook>() == null) cameraObject.AddComponent<TouchCameraLook>();
        cameraObject.transform.position = player.transform.position + follow.offset;

        for (int i = 0; i < 5; i++)
        {
            Vector2 p = Random.insideUnitCircle * 22f;
            GameObject bot = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            bot.name = "TrainingBot_" + (i + 1);
            bot.transform.position = new Vector3(p.x, 1f, p.y);
            bot.GetComponent<Renderer>().material.color = new Color(0.75f, 0.2f, 0.16f);
            BasicBotAI ai = bot.AddComponent<BasicBotAI>();
            ai.target = player.transform;
            bot.AddComponent<HealthSystem>();
            BotCombat combat = bot.AddComponent<BotCombat>();
            combat.target = player.transform;
        }

        CreateControls();
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

        GameObject hint = new GameObject("ControlHint", typeof(RectTransform), typeof(Text));
        hint.transform.SetParent(canvas.transform, false);
        RectTransform rect = hint.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.94f); rect.anchorMax = new Vector2(0.5f, 0.94f);
        rect.sizeDelta = new Vector2(600f, 50f);
        Text text = hint.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.text = "TRAINING MATCH  |  LEFT: MOVE JOYSTICK  |  RIGHT: DRAG TO LOOK";
        text.fontSize = 20; text.alignment = TextAnchor.MiddleCenter; text.color = Color.white;
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
        rect.anchorMin = new Vector2(0.5f, 0.87f);
        rect.anchorMax = new Vector2(0.5f, 0.87f);
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
