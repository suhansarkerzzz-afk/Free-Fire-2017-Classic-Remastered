using UnityEngine;
using UnityEngine.UI;

// Result overlay remains interactive when gameplay time is paused.
public class MatchResultUI : MonoBehaviour
{
    private GameObject panel;

    public void Show(bool won, int kills)
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;
        panel = new GameObject("MatchResultPanel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(canvas.transform, false);
        Image background = panel.GetComponent<Image>();
        background.color = new Color(0.015f, 0.035f, 0.055f, 0.97f);
        RectTransform rect = background.rectTransform;
        rect.anchorMin = new Vector2(0.25f, 0.18f);
        rect.anchorMax = new Vector2(0.75f, 0.82f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        AddText(won ? "BOOYAH!" : "MATCH OVER", 44, won ? new Color(1f, 0.78f, 0.12f) : Color.white,
            new Vector2(0.5f, 0.76f));
        AddText("PLAYER KILLS: " + kills, 26, Color.white, new Vector2(0.5f, 0.57f));
        AddButton("PLAY AGAIN", new Vector2(0.5f, 0.34f), GameBootstrap.RestartMatch);
        AddButton("MAIN MENU", new Vector2(0.5f, 0.14f), GameBootstrap.ReturnToMenu);
    }

    private void AddText(string value, int size, Color color, Vector2 anchor)
    {
        GameObject obj = new GameObject(value, typeof(RectTransform), typeof(Text));
        obj.transform.SetParent(panel.transform, false);
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = anchor; rect.anchorMax = anchor; rect.sizeDelta = new Vector2(560f, 64f);
        Text text = obj.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.text = value; text.fontSize = size; text.color = color;
        text.alignment = TextAnchor.MiddleCenter;
    }

    private void AddButton(string value, Vector2 anchor, UnityEngine.Events.UnityAction action)
    {
        GameObject obj = new GameObject(value, typeof(RectTransform), typeof(Image), typeof(Button));
        obj.transform.SetParent(panel.transform, false);
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = anchor; rect.anchorMax = anchor; rect.sizeDelta = new Vector2(330f, 58f);
        obj.GetComponent<Image>().color = new Color(0.78f, 0.38f, 0.08f, 1f);
        Text label = new GameObject("Label", typeof(RectTransform), typeof(Text)).GetComponent<Text>();
        label.transform.SetParent(obj.transform, false);
        label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        label.text = value; label.fontSize = 22; label.color = Color.white;
        label.alignment = TextAnchor.MiddleCenter;
        label.rectTransform.anchorMin = Vector2.zero; label.rectTransform.anchorMax = Vector2.one;
        label.rectTransform.offsetMin = Vector2.zero; label.rectTransform.offsetMax = Vector2.zero;
        obj.GetComponent<Button>().onClick.AddListener(action);
    }
}
