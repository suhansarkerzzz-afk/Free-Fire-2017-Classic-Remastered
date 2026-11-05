using System.Globalization;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;

// Android Bluetooth Classic RFCOMM transport with lightweight 1-to-1 player transform/team sync.
// Bluetooth is an optional local link; this does not provide authoritative combat or multi-client rooms.
public class BluetoothGameTransport : MonoBehaviour
{
    private const string NativeClass = "com.classicremastered.bluetooth.BluetoothGameTransport";
    public Transform localPlayer;
    private string lastDeviceAddress;
    private Text status;
    private Button connectButton;
    private Transform remotePlayer;
    private Vector3 remoteTargetPosition;
    private bool connected;
    private float sendTimer;
    private const float SendInterval = 0.12f;

    private void Start()
    {
        if (localPlayer == null)
        {
            PlayerMovement movement = FindObjectOfType<PlayerMovement>();
            if (movement != null) localPlayer = movement.transform;
        }
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;
        CreateButton(canvas.transform, "BT HOST", new Vector2(0.50f, 0.12f), StartHost);
        connectButton = CreateButton(canvas.transform, "BT CONNECT", new Vector2(0.76f, 0.12f), ConnectLastDevice);
        GameObject label = new GameObject("BluetoothTransportStatus", typeof(RectTransform), typeof(Text));
        label.transform.SetParent(canvas.transform, false);
        RectTransform rect = label.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.63f, 0.19f); rect.anchorMax = new Vector2(0.63f, 0.19f);
        rect.sizeDelta = new Vector2(430f, 30f);
        status = label.GetComponent<Text>();
        status.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        status.fontSize = 14; status.alignment = TextAnchor.MiddleCenter; status.color = Color.white;
        status.text = "BLUETOOTH: SCAN, THEN HOST OR CONNECT (1-TO-1)";
    }

    private Button CreateButton(Transform parent, string title, Vector2 anchor, UnityEngine.Events.UnityAction action)
    {
        GameObject obj = new GameObject(title, typeof(RectTransform), typeof(Image), typeof(Button));
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = anchor; rect.anchorMax = anchor; rect.sizeDelta = new Vector2(190f, 46f);
        obj.GetComponent<Image>().color = new Color(0.12f, 0.30f, 0.42f, 0.95f);
        GameObject label = new GameObject("Label", typeof(RectTransform), typeof(Text));
        label.transform.SetParent(obj.transform, false);
        Text text = label.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.text = title; text.fontSize = 16; text.alignment = TextAnchor.MiddleCenter; text.color = Color.white;
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
        else if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Permission.RequestUserPermission(Permission.FineLocation);
            SetStatus("ALLOW LOCATION FOR BLUETOOTH DISCOVERY");
            return false;
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
        SetStatus("FOUND " + address + " — TAP BT CONNECT");
    }

    public void OnBluetoothTransportConnected(string address)
    {
        connected = true;
        SetStatus("BLUETOOTH LINK CONNECTED — PLAYER SYNC ACTIVE");
    }

    public void OnBluetoothTransportMessage(string message)
    {
        if (string.IsNullOrEmpty(message)) return;
        string[] p = message.Split('|');
        if (p.Length < 7 || p[0] != "STATE") return;
        if (!TryFloat(p[1], out float x) || !TryFloat(p[2], out float y) ||
            !TryFloat(p[3], out float z) || !TryFloat(p[4], out float yaw)) return;
        int.TryParse(p[6], out int teamId);
        if (remotePlayer == null)
        {
            GameObject avatar = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            avatar.name = "Bluetooth_Remote_Player";
            avatar.GetComponent<Renderer>().material.color = new Color(0.95f, 0.25f, 0.18f);
            TeamAffiliation.Assign(avatar, teamId);
            remotePlayer = avatar.transform;
        }
        TeamAffiliation.Assign(remotePlayer.gameObject, teamId);
        remotePlayer.rotation = Quaternion.Euler(0f, yaw, 0f);
        remoteTargetPosition = new Vector3(x, y, z);
        SetStatus("BLUETOOTH MATCH LINK — PLAYER CONNECTED");
    }

    private bool TryFloat(string value, out float result)
    {
        return float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result);
    }

    private void Update()
    {
        if (connected && localPlayer != null)
        {
            sendTimer -= Time.deltaTime;
            if (sendTimer <= 0f)
            {
                SendPlayerState();
                sendTimer = SendInterval;
            }
        }
        if (remotePlayer != null)
            remotePlayer.position = Vector3.Lerp(remotePlayer.position, remoteTargetPosition, Time.deltaTime * 12f);
    }

    private void SendPlayerState()
    {
        if (!connected || localPlayer == null) return;
        Vector3 p = localPlayer.position;
        HealthSystem health = localPlayer.GetComponent<HealthSystem>();
        float hp = health != null ? health.CurrentHealth : 100f;
        TeamAffiliation team = localPlayer.GetComponent<TeamAffiliation>();
        int teamId = team != null ? team.teamId : 0;
        string packet = string.Join("|", "STATE",
            p.x.ToString("F2", CultureInfo.InvariantCulture),
            p.y.ToString("F2", CultureInfo.InvariantCulture),
            p.z.ToString("F2", CultureInfo.InvariantCulture),
            localPlayer.eulerAngles.y.ToString("F1", CultureInfo.InvariantCulture),
            hp.ToString("F0", CultureInfo.InvariantCulture), teamId.ToString());
        SendText(packet);
    }

    public void OnBluetoothTransportError(string message)
    {
        connected = false;
        SetStatus("BLUETOOTH: " + message);
    }

    public void SendText(string message)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (AndroidJavaClass plugin = new AndroidJavaClass(NativeClass))
            {
                string result = plugin.CallStatic<string>("send", message);
                if (result != "SENT") SetStatus("BLUETOOTH: " + result);
            }
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
        connected = false;
        if (remotePlayer != null) Destroy(remotePlayer.gameObject);
    }
}