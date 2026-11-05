using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

// Lightweight same-Wi-Fi UDP room and transform synchronisation prototype.
// Host and clients must be on the same LAN; this is not an internet matchmaking service.
public class LocalMultiplayerManager : MonoBehaviour
{
    public Transform localPlayer;
    public int port = 47820;
    public int maxRemotePlayers = 11;
    public string RoomCode { get; private set; }
    public bool IsHost { get; private set; }
    public bool IsConnected { get; private set; }
    private UdpClient socket;
    private IPEndPoint anyEndpoint = new IPEndPoint(IPAddress.Any, 0);
    private IPEndPoint hostEndpoint;
    private readonly Dictionary<string, IPEndPoint> peers = new Dictionary<string, IPEndPoint>();
    private readonly Dictionary<string, Transform> remotePlayers = new Dictionary<string, Transform>();
    private readonly Dictionary<string, Vector3> targetPositions = new Dictionary<string, Vector3>();
    private string localId;
    private float sendTimer;
    private float discoveryTimer;

    public string CreateRoom()
    {
        StopRoom();
        RoomCode = LocalRoomCode.CreateCode();
        localId = Guid.NewGuid().ToString("N").Substring(0, 8);
        socket = new UdpClient(port);
        socket.EnableBroadcast = true;
        socket.Client.Blocking = false;
        IsHost = true;
        IsConnected = true;
        return RoomCode;
    }

    public void JoinRoom(string code)
    {
        StopRoom();
        if (!LocalRoomCode.IsValid(code)) { Debug.LogWarning("Invalid room code."); return; }
        RoomCode = code.ToUpperInvariant();
        localId = Guid.NewGuid().ToString("N").Substring(0, 8);
        socket = new UdpClient(0);
        socket.EnableBroadcast = true;
        socket.Client.Blocking = false;
        IsHost = false;
        IsConnected = false;
        discoveryTimer = 0f;
    }

    private void Update()
    {
        if (socket == null) return;
        ReceivePackets();
        if (!IsHost && !IsConnected)
        {
            discoveryTimer -= Time.deltaTime;
            if (discoveryTimer <= 0f)
            {
                SendBroadcast("DISCOVER|" + RoomCode);
                discoveryTimer = 1f;
            }
        }
        if (IsConnected && localPlayer != null)
        {
            sendTimer -= Time.deltaTime;
            if (sendTimer <= 0f)
            {
                SendState();
                sendTimer = 0.08f;
            }
        }
        foreach (var item in targetPositions)
        {
            if (remotePlayers.TryGetValue(item.Key, out Transform remote) && remote != null)
                remote.position = Vector3.Lerp(remote.position, item.Value, Time.deltaTime * 12f);
        }
    }

    private void ReceivePackets()
    {
        for (int i = 0; i < 40; i++)
        {
            try
            {
                byte[] bytes = socket.Receive(ref anyEndpoint);
                string message = Encoding.UTF8.GetString(bytes);
                HandlePacket(message, anyEndpoint);
            }
            catch (SocketException ex)
            {
                if (ex.SocketErrorCode == SocketError.WouldBlock || ex.SocketErrorCode == SocketError.IOPending) break;
                Debug.LogWarning("LAN receive: " + ex.Message);
                break;
            }
            catch (ObjectDisposedException) { break; }
        }
    }

    private void HandlePacket(string message, IPEndPoint sender)
    {
        string[] p = message.Split('|');
        if (p.Length < 2) return;
        if (p[0] == "DISCOVER" && IsHost && p[1] == RoomCode)
        {
            SendTo(sender, "ROOM|" + RoomCode + "|" + localId);
            return;
        }
        if (p[0] == "ROOM" && !IsHost && p.Length >= 3 && p[1] == RoomCode && !IsConnected)
        {
            hostEndpoint = sender;
            SendTo(hostEndpoint, "JOIN|" + RoomCode + "|" + localId);
            return;
        }
        if (p[0] == "JOIN" && IsHost && p.Length >= 3 && p[1] == RoomCode)
        {
            if (!peers.ContainsKey(p[2]) && peers.Count >= maxRemotePlayers)
            {
                SendTo(sender, "FULL|" + RoomCode);
                return;
            }
            peers[p[2]] = sender;
            SendTo(sender, "WELCOME|" + RoomCode + "|" + localId);
            return;
        }
        if (p[0] == "FULL" && !IsHost && p.Length >= 2 && p[1] == RoomCode)
        {
            IsConnected = false;
            hostEndpoint = null;
            Debug.LogWarning("LAN room is full.");
            return;
        }
        if (p[0] == "WELCOME" && !IsHost && p.Length >= 3 && p[1] == RoomCode)
        {
            hostEndpoint = sender;
            IsConnected = true;
            return;
        }
        if (p[0] == "STATE" && p.Length >= 9 && p[1] == RoomCode)
        {
            string id = p[2];
            if (id == localId) return;
            if (IsHost)
            {
                peers[id] = sender;
                Relay(message, sender);
            }
            UpdateRemote(id, p);
        }
    }

    private void UpdateRemote(string id, string[] p)
    {
        if (!float.TryParse(p[3], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float x) ||
            !float.TryParse(p[4], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float y) ||
            !float.TryParse(p[5], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float z) ||
            !float.TryParse(p[6], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float yaw)) return;
        if (!remotePlayers.TryGetValue(id, out Transform remote) || remote == null)
        {
            GameObject avatar = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            avatar.name = "LAN_Player_" + id;
            avatar.GetComponent<Renderer>().material.color = new Color(0.2f, 0.8f, 0.95f);
            avatar.AddComponent<TeamAffiliation>();
            remote = avatar.transform;
            remotePlayers[id] = remote;
        }
        int teamId = 0;
        int.TryParse(p[8], out teamId);
        TeamAffiliation.Assign(remote.gameObject, teamId);
        remote.rotation = Quaternion.Euler(0f, yaw, 0f);
        targetPositions[id] = new Vector3(x, y, z);
    }

    private void SendState()
    {
        if (localPlayer == null) return;
        Vector3 p = localPlayer.position;
        string packet = string.Join("|", "STATE", RoomCode, localId,
            p.x.ToString("F2", System.Globalization.CultureInfo.InvariantCulture),
            p.y.ToString("F2", System.Globalization.CultureInfo.InvariantCulture),
            p.z.ToString("F2", System.Globalization.CultureInfo.InvariantCulture),
            localPlayer.eulerAngles.y.ToString("F1", System.Globalization.CultureInfo.InvariantCulture), "100",
            (localPlayer.GetComponent<TeamAffiliation>() != null ? localPlayer.GetComponent<TeamAffiliation>().teamId : 0).ToString());
        if (IsHost) Relay(packet, null);
        else if (hostEndpoint != null) SendTo(hostEndpoint, packet);
    }

    private void Relay(string packet, IPEndPoint except)
    {
        foreach (var peer in peers)
            if (except == null || !peer.Value.Equals(except)) SendTo(peer.Value, packet);
    }

    private void SendBroadcast(string message)
    {
        if (socket == null) return;
        byte[] bytes = Encoding.UTF8.GetBytes(message);
        socket.Send(bytes, bytes.Length, new IPEndPoint(IPAddress.Broadcast, port));
    }

    private void SendTo(IPEndPoint endpoint, string message)
    {
        if (socket == null || endpoint == null) return;
        byte[] bytes = Encoding.UTF8.GetBytes(message);
        socket.Send(bytes, bytes.Length, endpoint);
    }

    public void StopRoom()
    {
        IsConnected = false;
        IsHost = false;
        RoomCode = null;
        hostEndpoint = null;
        peers.Clear();
        foreach (Transform remote in remotePlayers.Values) if (remote != null) Destroy(remote.gameObject);
        remotePlayers.Clear();
        targetPositions.Clear();
        if (socket != null) { socket.Close(); socket = null; }
    }

    private void OnDestroy() { StopRoom(); }
}
