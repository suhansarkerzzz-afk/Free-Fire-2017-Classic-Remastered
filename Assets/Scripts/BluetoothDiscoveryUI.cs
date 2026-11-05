using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;

// Optional Android Bluetooth discovery. Bluetooth is discovery-only; gameplay remains on Wi-Fi/LAN.
public class BluetoothDiscoveryUI : MonoBehaviour
{
    private Text status;
    private const string NativeClass = "com.classicremastered.bluetooth.BluetoothDiscovery";

    private void Start()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;
        GameObject labelObject = new GameObject("BluetoothStatus", typeof(RectTransform), typeof(Text));
        labelObject.transform.SetParent(canvas.transform, false);
        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0.5f, 0.20f);
        labelRect.anchorMax = new Vector2(0.5f, 0.20f);
        labelRect.sizeDelta = new Vector2(500f, 34f);
        status = labelObject.GetComponent<Text>();
        status.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        status.fontSize = 16;
        status.alignment = TextAnchor.MiddleCenter;
        status.color = Color.white;
        status.text = "BLUETOOTH: OPTIONAL DEVICE DISCOVERY";

        GameObject buttonObject = new GameObject("BLUETOOTH_SCAN", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(canvas.transform, false);
        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.27f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.27f);
        buttonRect.sizeDelta = new Vector2(190f, 52f);
        buttonObject.GetComponent<Image>().color = new Color(0.12f, 0.32f, 0.42f, 0.9f);
        Text buttonText = new GameObject("Label", typeof(RectTransform), typeof(Text)).GetComponent<Text>();
        buttonText.transform.SetParent(buttonObject.transform, false);
        buttonText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        buttonText.text = "BLUETOOTH SCAN";
        buttonText.fontSize = 17;
        buttonText.alignment = TextAnchor.MiddleCenter;
        buttonText.color = Color.white;
        buttonText.rectTransform.anchorMin = Vector2.zero;
        buttonText.rectTransform.anchorMax = Vector2.one;
        buttonText.rectTransform.offsetMin = Vector2.zero;
        buttonText.rectTransform.offsetMax = Vector2.zero;
        buttonObject.GetComponent<Button>().onClick.AddListener(StartDiscovery);
    }

    public void StartDiscovery()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        int sdk = 0;
        using (AndroidJavaClass version = new AndroidJavaClass("android.os.Build$VERSION"))
            sdk = version.GetStatic<int>("SDK_INT");

        if (sdk >= 31)
        {
            string[] permissions = { "android.permission.BLUETOOTH_SCAN", "android.permission.BLUETOOTH_CONNECT" };
            bool missing = false;
            foreach (string permission in permissions)
                if (!Permission.HasUserAuthorizedPermission(permission)) missing = true;
            if (missing)
            {
                Permission.RequestUserPermissions(permissions);
                SetStatus("ALLOW NEARBY DEVICES, THEN TAP SCAN AGAIN");
                return;
            }
        }
        else if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Permission.RequestUserPermission(Permission.FineLocation);
            SetStatus("ALLOW LOCATION, THEN TAP SCAN AGAIN");
            return;
        }

        try
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (AndroidJavaClass plugin = new AndroidJavaClass(NativeClass))
            {
                string result = plugin.CallStatic<string>("startDiscovery", activity, gameObject.name);
                SetStatus(result == "SCANNING" ? "SCANNING FOR NEARBY BLUETOOTH DEVICES..." : "BLUETOOTH: " + result);
            }
        }
        catch (System.Exception ex) { SetStatus("BLUETOOTH ERROR: " + ex.GetType().Name); }
#else
        SetStatus("BLUETOOTH DISCOVERY IS AVAILABLE IN THE ANDROID BUILD");
#endif
    }

    public void OnBluetoothDeviceFound(string payload)
    {
        if (string.IsNullOrEmpty(payload)) return;
        string[] parts = payload.Split('|');
        string deviceName = parts.Length > 0 ? parts[0] : "Unknown";
        string address = parts.Length > 1 ? parts[1] : "";
        SetStatus("FOUND: " + deviceName + (string.IsNullOrEmpty(address) ? "" : "  " + address));
    }

    public void OnBluetoothDiscoveryFinished(string unused)
    {
        SetStatus("BLUETOOTH SCAN FINISHED");
    }

    private void SetStatus(string value)
    {
        if (status != null) status.text = value;
        Debug.Log(value);
    }

    private void OnDestroy()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (AndroidJavaClass plugin = new AndroidJavaClass(NativeClass))
                plugin.CallStatic("stopDiscovery", activity);
        }
        catch { }
#endif
    }
}
