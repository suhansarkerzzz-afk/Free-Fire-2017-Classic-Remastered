using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;

// Android Bluetooth Classic RFCOMM connection bridge.
// This establishes a direct phone-to-phone data channel; match-state replication is a separate layer.
public class BluetoothGameTransport : MonoBehaviour
{
    private const string NativeClass = "com.classicremastered.bluetooth.BluetoothGameTransport";
    private string lastDeviceAddress;
    private Text status;
    private Button hostButton;
    private Button connectButton;

    private void Start()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;
        CreateButton(canvas.transform, "BT HOST", new Vector2(0.35f, 0.10f), StartHost);
        connectButton = CreateButton(canvas.transform, "BT CONNECT", new Vector2(0.65f, 0.10f), ConnectLastDevice);
        GameObject label = new GameObject("BluetoothTransportStatus", typeof(RectTransform), typeof(Text));
        label.transform.SetParent(canvas.transform, false);
        RectTransform rect = label.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.22f); rect.anchorMax = new Vector2(0.5f, 0.22f);
        rect.sizeDelta = new Vector2(520f, 32f);
        status = label.GetComponent<Text>();
        status.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        status.fontSize = 15; status.alignment = TextAnchor.MiddleCenter; status.color = Color.white;
        status.text = "BLUETOOTH DIRECT LINK: SCAN A NEARBY PHONE";
    }

    private Button CreateButton(Transform parent, string title, Vector2 anchor, UnityEngine.Events.UnityAction action)
    {
        GameObject obj = new GameObject(title, typeof(RectTransform), typeof(Image), typeof(Button));
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = anchor; rect.anchorMax = anchor; rect.sizeDelta = new Vector2(190f, 48f);
        obj.GetComponent<Image>().color = new Color(0.12f, 0.30f, 0.42f, 0.95f);
        GameObject label = new GameObject("Label", typeof(RectTransform), typeof(Text));
        label.transform.SetParent(obj.transform, false);
        Text text = label.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.text = title; text.fontSize = 17; text.alignment = TextAnchor.MiddleCenter; text.color = Color.white;
        text.rectTransform.anchorMin = Vector2.zero; text.rectTransform.anchorMax = Vector2.one;
        text.rectTransform.offsetMin = Vector2.zero; text.rectTransform.offsetMax = Vector2.zero;
        Button button = obj.GetComponent<Button>();
        button.onClick.AddListener(action);
        return button;
    }

    private bool RequestBluetoothPermissions()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        int sdk = 0;
        using (AndroidJavaClass version = new AndroidJavaClass("android.os.Build$VERSION"))
            sdk = version.GetStatic<int>("SDK_INT");
        if (sdk >= 31)
        {
            string[] permissions = { "android.permission.BLUETOOTH_SCAN", "android.permission.BLUETOOTH_CONNECT" };
            foreach (string permission in permissions)
                if (!Permission.HasUserAuthorizedPermission(permission))
                {
                    Permission.RequestUserPermissions(permissions);
                    SetStatus("ALLOW NEARBY DEVICES, THEN TRY AGAIN");
                    return false;
                }
        }
#endif
        return true;
    }

    private string CallNative(string method, params object[] args)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (AndroidJavaClass plugin = new AndroidJavaClass(NativeClass))
            {
                object[] callArgs = new object[args.Length + 2];
                callArgs[0] = activity;
                callArgs[1] = gameObject.name;
                for (int i = 0; i < args.Length; i++) callArgs[i + 2] = args[i];
                return plugin.CallStatic<string>(method, callArgs);
            }
        }
        catch (System.Exception ex) { return "ERROR:" + ex.GetType().Name; }
#else
        return "ANDROID_BUILD_REQUIRED";
#endif
    }

    public void StartHost()
    {
        if (!RequestBluetoothPermissions()) return;
        SetStatus(CallNative("startHost"));
    }

    public void ConnectLastDevice()
    {
        if (!RequestBluetoothPermissions()) return;
        if (string.IsNullOrEmpty(lastDeviceAddress)) { SetStatus("SCAN AND SELECT A PHONE FIRST"); return; }
        SetStatus(CallNative("connect", lastDeviceAddress));
    }

    public void SetDiscoveredDevice(string address)
    {
        lastDeviceAddress = address;
        if (connectButton != null) connectButton.interactable = !string.IsNullOrEmpty(address);
        SetStatus("DEVICE FOUND: " + address + " — TAP BT CONNECT");
    }

    public void OnBluetoothTransportConnected(string address) { SetStatus("BLUETOOTH LINK CONNECTED: " + address); }
    public void OnBluetoothTransportMessage(string message) { SetStatus("BT DATA RECEIVED: " + message); }
    public void OnBluetoothTransportError(string message) { SetStatus("BLUETOOTH: " + message); }

    public void SendText(string message)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (AndroidJavaClass plugin = new AndroidJavaClass(NativeClass))
                SetStatus(plugin.CallStatic<string>("send", message));
        }
        catch (System.Exception ex) { SetStatus("SEND ERROR: " + ex.GetType().Name); }
#endif
    }

    private void SetStatus(string value)
    {
        if (status != null) status.text = value;
        Debug.Log(value);
    }

    private void OnDestroy()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try { using (AndroidJavaClass plugin = new AndroidJavaClass(NativeClass)) plugin.CallStatic("stop"); }
        catch { }
#endif
    }
}
