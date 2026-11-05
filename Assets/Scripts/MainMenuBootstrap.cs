using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System;

public class MainMenuBootstrap : MonoBehaviour
{
    private Font font;
    private Canvas canvas;
    private GameObject settingsPanel;
    private GameObject characterPanel;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void StartMenu()
    {
        // RuntimeInitializeOnLoadMethod runs for the initial scene only.
        // Subscribe to sceneLoaded so Play Again and Main Menu also rebuild the menu.
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;
        EnsureMenuExists();
    }

    private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsureMenuExists();
    }

    private static void EnsureMenuExists()
    {
        if (FindObjectOfType<MainMenuBootstrap>() == null)
            new GameObject("ClassicMainMenu").AddComponent<MainMenuBootstrap>();
    }

    private void Start()
    {
        font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        EventSystem eventSystem = FindObjectOfType<EventSystem>();
        if (eventSystem == null)
        {
            GameObject eventObject = new GameObject("EventSystem");
            eventSystem = eventObject.AddComponent<EventSystem>();
        }

        if (eventSystem.GetComponent<StandaloneInputModule>() == null)
            eventSystem.gameObject.AddComponent<StandaloneInputModule>();

        GameObject canvasObject = new GameObject("MainCanvas");
        canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObject.AddComponent<GraphicRaycaster>();

        Image background = CreateImage("Background", canvas.transform,
            new Color(0.035f, 0.09f, 0.15f));
        background.rectTransform.anchorMin = Vector2.zero;
        background.rectTransform.anchorMax = Vector2.one;
        background.rectTransform.offsetMin = Vector2.zero;
        background.rectTransform.offsetMax = Vector2.zero;

        CreateText("FREE FIRE", 65, Color.white,
            new Vector2(0.5f, 0.82f), new Vector2(0.5f, 0.82f), canvas.transform);
        CreateText("2017 CLASSIC REMASTERED", 27,
            new Color(1f, 0.72f, 0.12f),
            new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.72f), canvas.transform);

        CreateButton("OFFLINE SOLO (49 BOTS)", new Vector2(0.5f, 0.54f),
            GameBootstrap.StartMatch);
        CreateButton("LOCAL WI-FI / HOTSPOT (BETA)", new Vector2(0.5f, 0.42f),
            GameBootstrap.StartLocalMultiplayer);
        CreateButton("GRAPHICS SETTINGS", new Vector2(0.5f, 0.30f), ShowSettings);
        CreateButton("CHARACTER SELECT", new Vector2(0.5f, 0.18f), ShowCharacters);

        CreateText("CLASSIC BATTLE EXPERIENCE", 16, Color.white,
            new Vector2(0.5f, 0.96f), new Vector2(0.5f, 0.96f), canvas.transform);

        CreateSettings();
        CreateCharacterPanel();
        // Bluetooth is optional nearby-device discovery only; it never carries gameplay traffic.
        canvasObject.AddComponent<BluetoothDiscoveryUI>();
        if (GameBootstrap.ConsumeAutoStart()) GameBootstrap.StartMatch();
    }

    private Image CreateImage(string name, Transform parent, Color color)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        Image image = obj.AddComponent<Image>();
        image.color = color;
        return image;
    }

    private Text CreateText(string value, int size, Color color,
        Vector2 anchorMin, Vector2 anchorMax, Transform parent)
    {
        GameObject obj = new GameObject(value);
        obj.transform.SetParent(parent, false);
        Text text = obj.AddComponent<Text>();
        text.font = font;
        text.text = value;
        text.fontSize = size;
        text.color = color;
        text.alignment = TextAnchor.MiddleCenter;

        RectTransform rect = text.rectTransform;
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.sizeDelta = new Vector2(800, 100);
        return text;
    }

    private void CreateButton(string label, Vector2 position, Action action,
        Transform parent = null)
    {
        GameObject obj = new GameObject(label);
        obj.transform.SetParent(parent != null ? parent : canvas.transform, false);

        Image image = obj.AddComponent<Image>();
        image.color = new Color(0.85f, 0.48f, 0.08f);

        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = position;
        rect.anchorMax = position;
        rect.sizeDelta = new Vector2(350, 65);

        Button button = obj.AddComponent<Button>();
        button.onClick.AddListener(() => action?.Invoke());

        Text text = CreateText(label, 23, Color.white,
            Vector2.zero, Vector2.one, obj.transform);
        text.rectTransform.sizeDelta = Vector2.zero;
    }

    private void CreateSettings()
    {
        settingsPanel = new GameObject("SettingsPanel");
        settingsPanel.transform.SetParent(canvas.transform, false);

        Image panel = settingsPanel.AddComponent<Image>();
        panel.color = new Color(0.04f, 0.08f, 0.12f, 0.97f);

        RectTransform rect = panel.rectTransform;
        rect.anchorMin = new Vector2(0.2f, 0.2f);
        rect.anchorMax = new Vector2(0.8f, 0.8f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        CreateText("GRAPHICS SETTINGS", 30, Color.white,
            new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.75f), settingsPanel.transform);
        CreateButton("LOW", new Vector2(0.5f, 0.55f),
            () => SetGraphics(0), settingsPanel.transform);
        CreateButton("BALANCED", new Vector2(0.5f, 0.38f),
            () => SetGraphics(1), settingsPanel.transform);
        CreateButton("HIGH", new Vector2(0.5f, 0.21f),
            () => SetGraphics(2), settingsPanel.transform);

        settingsPanel.SetActive(false);
    }

    private void CreateCharacterPanel()
    {
        characterPanel = new GameObject("CharacterSelectPanel");
        characterPanel.transform.SetParent(canvas.transform, false);
        Image panel = characterPanel.AddComponent<Image>();
        panel.color = new Color(0.035f, 0.07f, 0.11f, 0.98f);
        RectTransform rect = panel.rectTransform;
        rect.anchorMin = new Vector2(0.18f, 0.15f);
        rect.anchorMax = new Vector2(0.82f, 0.85f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        CreateText("CHOOSE YOUR CLASSIC SURVIVOR", 28, Color.white,
            new Vector2(0.5f, 0.90f), new Vector2(0.5f, 0.90f), characterPanel.transform);
        CreateButton("SCOUT", new Vector2(0.5f, 0.73f), () => SelectCharacter(0), characterPanel.transform);
        CreateButton("RANGER", new Vector2(0.5f, 0.57f), () => SelectCharacter(1), characterPanel.transform);
        CreateButton("DESERT", new Vector2(0.5f, 0.41f), () => SelectCharacter(2), characterPanel.transform);
        CreateButton("SHADOW", new Vector2(0.5f, 0.25f), () => SelectCharacter(3), characterPanel.transform);
        characterPanel.SetActive(false);
    }

    private void CreateSmallButton(string label, Vector2 position, Action action, Transform parent)
    {
        GameObject obj = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = position; rect.anchorMax = position; rect.sizeDelta = new Vector2(135f, 44f);
        obj.GetComponent<Image>().color = new Color(0.18f, 0.35f, 0.42f, 1f);
        Text text = CreateText(label, 16, Color.white, Vector2.zero, Vector2.one, obj.transform);
        text.rectTransform.sizeDelta = Vector2.zero;
        obj.GetComponent<Button>().onClick.AddListener(() => action?.Invoke());
    }

    private void ShowCharacters()
    {
        characterPanel.SetActive(true);
    }

    private void SelectCharacter(int index)
    {
        CharacterLoadout.Select(index);
        characterPanel.SetActive(false);
    }

    private void ShowSettings()
    {
        settingsPanel.SetActive(true);
    }

    private void SetGraphics(int level)
    {
        GraphicsProfile.Apply(level);
        settingsPanel.SetActive(false);
    }
}
