
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class MainMenuBootstrap : MonoBehaviour
{
    private Font font;
    private Canvas canvas;
    private GameObject settingsPanel;

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void StartMenu()
    {
        if (FindObjectOfType<MainMenuBootstrap>() == null)
        {
            new GameObject("ClassicMainMenu")
                .AddComponent<MainMenuBootstrap>();
        }
    }

    private void Start()
    {
        font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        if (FindObjectOfType<EventSystem>() == null)
            new GameObject("EventSystem")
                .AddComponent<EventSystem>();

        var input = new GameObject("UIInput");
        input.AddComponent<StandaloneInputModule>();

        var canvasObject = new GameObject("MainCanvas");
        canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();

        Image background = CreateImage(
            "Background", canvas.transform,
            new Color(0.035f, 0.09f, 0.15f));

        background.rectTransform.anchorMin = Vector2.zero;
        background.rectTransform.anchorMax = Vector2.one;
        background.rectTransform.offsetMin = Vector2.zero;
        background.rectTransform.offsetMax = Vector2.zero;

        CreateText("FREE FIRE", 65, Color.white,
            new Vector2(0.5f, 0.82f),
            new Vector2(0.5f, 0.82f), canvas.transform);

        CreateText("2017 CLASSIC REMASTERED", 27,
            new Color(1f, 0.72f, 0.12f),
            new Vector2(0.5f, 0.72f),
            new Vector2(0.5f, 0.72f), canvas.transform);

        CreateButton("TAP TO START", new Vector2(0.5f, 0.43f),
            () => Debug.Log("Start Game"));

        CreateButton("GRAPHICS SETTINGS", new Vector2(0.5f, 0.31f),
            ShowSettings);

        CreateButton("MULTIPLAYER", new Vector2(0.5f, 0.19f),
            () => Debug.Log("Multiplayer Coming Soon"));

        CreateText("CLASSIC BATTLE EXPERIENCE",
            18, Color.white,
            new Vector2(0.5f, 0.06f),
            new Vector2(0.5f, 0.06f), canvas.transform);

        CreateSettings();
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

    private void CreateButton(string label, Vector2 position,
        Action action)
    {
        GameObject obj = new GameObject(label);
        obj.transform.SetParent(canvas.transform, false);

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
            new Vector2(0.5f, 0.75f),
            new Vector2(0.5f, 0.75f), settingsPanel.transform);

        CreateButton("LOW", new Vector2(0.5f, 0.55f),
            () => SetGraphics(0));

        CreateButton("BALANCED", new Vector2(0.5f, 0.38f),
            () => SetGraphics(1));

        CreateButton("HIGH", new Vector2(0.5f, 0.21f),
            () => SetGraphics(2));

        settingsPanel.SetActive(false);
    }

    private void ShowSettings()
    {
        settingsPanel.SetActive(true);
    }

    private void SetGraphics(int level)
    {
        QualitySettings.SetQualityLevel(level, true);
        Debug.Log("Graphics Level: " + level);
        settingsPanel.SetActive(false);
    }
}
