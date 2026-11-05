using UnityEngine;
using UnityEngine.UI;

// Small in-match LAN room panel for host/create and join-by-code.
public class LocalMultiplayerUI : MonoBehaviour
{
    public static bool OpenOnStart;
    public LocalMultiplayerManager manager;
    private GameObject panel;
    private InputField codeInput;
    private Text status;
    private void Start()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;
        panel = new GameObject("LANRoomPanel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(canvas.transform, false);
        Image bg = panel.GetComponent<Image>();
        bg.color = new Color(0.025f, 0.06f, 0.09f, 0.96f);
        RectTransform rect = bg.rectTransform;
        rect.anchorMin = new Vector2(0.32f, 0.25f);
        rect.anchorMax = new Vector2(0.68f, 0.78f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        AddText("LOCAL WI-FI ROOM", 26, new Vector2(0.5f, 0.85f), panel.transform);
        codeInput = CreateInput(new Vector2(0.5f, 0.63f), panel.transform);
        AddButton("CREATE ROOM", new Vector2(0.5f, 0.43f), CreateRoom, panel.transform);
        AddButton("JOIN ROOM", new Vector2(0.5f, 0.25f), JoinRoom, panel.transform);
        status = AddText("Same Wi-Fi / hotspot required", 16, new Vector2(0.5f, 0.08f), panel.transform);
        panel.SetActive(OpenOnStart);
        OpenOnStart = false;

        AddButton("LAN ROOM", new Vector2(0.5f, 0.08f), TogglePanel, canvas.transform);
    }

    private Text AddText(string value, int size, Vector2 anchor, Transform parent)
    {
        GameObject obj = new GameObject(value, typeof(RectTransform), typeof(Text));
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = anchor; rect.anchorMax = anchor; rect.sizeDelta = new Vector2(420f, 42f);
        Text text = obj.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.text = value; text.fontSize = size; text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
        return text;
    }

    private InputField CreateInput(Vector2 anchor, Transform parent)
    {
        GameObject obj = new GameObject("RoomCodeInput", typeof(RectTransform), typeof(Image), typeof(InputField));
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = anchor; rect.anchorMax = anchor; rect.sizeDelta = new Vector2(320f, 54f);
        obj.GetComponent<Image>().color = Color.white;
        Text value = AddText("", 22, new Vector2(0.5f, 0.5f), obj.transform);
        value.color = Color.black;
        InputField input = obj.GetComponent<InputField>();
        input.targetGraphic = obj.GetComponent<Image>();
        input.textComponent = value;
        input.characterLimit = 6;
        input.contentType = InputField.ContentType.Alphanumeric;
        return input;
    }

    private void AddButton(string title, Vector2 anchor, UnityEngine.Events.UnityAction action, Transform parent)
    {
        GameObject obj = new GameObject(title, typeof(RectTransform), typeof(Image), typeof(Button));
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = anchor; rect.anchorMax = anchor; rect.sizeDelta = new Vector2(300f, 52f);
        obj.GetComponent<Image>().color = new Color(0.78f, 0.39f, 0.08f);
        Text label = AddText(title, 19, new Vector2(0.5f, 0.5f), obj.transform);
        label.rectTransform.sizeDelta = Vector2.zero;
        obj.GetComponent<Button>().onClick.AddListener(action);
    }

    private void Update()
    {
        if (manager == null || status == null || !panel.activeSelf) return;
        if (manager.IsHost) status.text = "HOSTING ROOM: " + manager.RoomCode + "  |  PLAYERS: " + (manager.RemotePlayerCount + 1);
        else if (!string.IsNullOrEmpty(manager.StatusMessage)) status.text = manager.StatusMessage;
    }

    private void TogglePanel() { panel.SetActive(!panel.activeSelf); }

    private void CreateRoom()
    {
        if (manager == null) return;
        string code = manager.CreateRoom();
        if (string.IsNullOrEmpty(code))
        {
            status.text = manager.StatusMessage;
            return;
        }
        codeInput.text = code;
        status.text = "HOSTING ROOM: " + code + "  |  SHARE THIS CODE";
    }

    private void JoinRoom()
    {
        if (manager == null) return;
        manager.JoinRoom(codeInput.text);
        status.text = manager.StatusMessage;
    }
}
