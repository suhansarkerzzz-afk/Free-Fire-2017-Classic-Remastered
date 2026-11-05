package com.classicremastered.bluetooth;

import android.app.Activity;
import android.bluetooth.BluetoothAdapter;
import android.bluetooth.BluetoothServerSocket;
import android.bluetooth.BluetoothSocket;
import android.bluetooth.BluetoothDevice;
import android.os.Build;
import com.unity3d.player.UnityPlayer;
import java.io.BufferedReader;
import java.io.InputStreamReader;
import java.io.OutputStream;
import java.nio.charset.StandardCharsets;
import java.util.UUID;

public final class BluetoothGameTransport {
    private static final UUID SERVICE_UUID = UUID.fromString("7d3a0001-6b2f-4f1a-9c31-2017ff000001");
    private static volatile boolean running;
    private static volatile BluetoothSocket socket;
    private static BluetoothServerSocket serverSocket;
    private static OutputStream output;
    private static String unityObjectName = "BluetoothDiscoveryUI";

    private BluetoothGameTransport() { }

    private static boolean hasPermissions(Activity activity) {
        if (Build.VERSION.SDK_INT >= 31) {
            return activity.checkSelfPermission("android.permission.BLUETOOTH_CONNECT") == android.content.pm.PackageManager.PERMISSION_GRANTED &&
                   activity.checkSelfPermission("android.permission.BLUETOOTH_SCAN") == android.content.pm.PackageManager.PERMISSION_GRANTED;
        }
        return true;
    }

    public static synchronized String startHost(Activity activity, String objectName) {
        stop();
        if (activity == null) return "NO_ACTIVITY";
        if (!hasPermissions(activity)) return "PERMISSION_REQUIRED";
        BluetoothAdapter adapter = BluetoothAdapter.getDefaultAdapter();
        if (adapter == null) return "BLUETOOTH_UNAVAILABLE";
        try {
            if (!adapter.isEnabled()) return "BLUETOOTH_OFF";
            unityObjectName = objectName == null ? "BluetoothDiscoveryUI" : objectName;
            serverSocket = adapter.listenUsingInsecureRfcommWithServiceRecord("FF2017_LOCAL_MATCH", SERVICE_UUID);
            running = true;
            final BluetoothServerSocket listening = serverSocket;
            new Thread(() -> {
                try {
                    BluetoothSocket accepted = listening.accept();
                    if (!running) { accepted.close(); return; }
                    attach(accepted);
                } catch (Exception ex) {
                    notifyUnity("OnBluetoothTransportError", ex.getClass().getSimpleName());
                }
            }, "FF2017-BT-Host").start();
            return "WAITING_FOR_PLAYER";
        } catch (SecurityException ex) { return "PERMISSION_REQUIRED"; }
        catch (Exception ex) { return "HOST_ERROR:" + ex.getClass().getSimpleName(); }
    }

    public static synchronized String connect(Activity activity, String objectName, String address) {
        stop();
        if (activity == null) return "NO_ACTIVITY";
        if (!hasPermissions(activity)) return "PERMISSION_REQUIRED";
        if (address == null || address.trim().isEmpty()) return "INVALID_ADDRESS";
        BluetoothAdapter adapter = BluetoothAdapter.getDefaultAdapter();
        if (adapter == null) return "BLUETOOTH_UNAVAILABLE";
        try {
            if (!adapter.isEnabled()) return "BLUETOOTH_OFF";
            unityObjectName = objectName == null ? "BluetoothDiscoveryUI" : objectName;
            adapter.cancelDiscovery();
            final BluetoothDevice device = adapter.getRemoteDevice(address);
            running = true;
            new Thread(() -> {
                try {
                    BluetoothSocket candidate = device.createInsecureRfcommSocketToServiceRecord(SERVICE_UUID);
                    candidate.connect();
                    if (!running) { candidate.close(); return; }
                    attach(candidate);
                } catch (Exception ex) {
                    notifyUnity("OnBluetoothTransportError", ex.getClass().getSimpleName());
                    stop();
                }
            }, "FF2017-BT-Client").start();
            return "CONNECTING";
        } catch (SecurityException ex) { return "PERMISSION_REQUIRED"; }
        catch (Exception ex) { return "CONNECT_ERROR:" + ex.getClass().getSimpleName(); }
    }

    private static synchronized void attach(BluetoothSocket connected) throws Exception {
        socket = connected;
        output = connected.getOutputStream();
        notifyUnity("OnBluetoothTransportConnected", connected.getRemoteDevice().getAddress());
        Thread reader = new Thread(() -> {
            try {
                BufferedReader input = new BufferedReader(new InputStreamReader(connected.getInputStream(), StandardCharsets.UTF_8));
                String line;
                while (running && (line = input.readLine()) != null)
                    notifyUnity("OnBluetoothTransportMessage", line);
            } catch (Exception ex) {
                if (running) notifyUnity("OnBluetoothTransportError", "CONNECTION_LOST");
            } finally { stop(); }
        }, "FF2017-BT-Reader");
        reader.start();
    }

    public static synchronized String send(String message) {
        try {
            if (!running || output == null) return "NOT_CONNECTED";
            String safe = message == null ? "" : message.replace("\\n", " ").replace("\\r", " ");
            output.write((safe + "\n").getBytes(StandardCharsets.UTF_8));
            output.flush();
            return "SENT";
        } catch (Exception ex) { return "SEND_ERROR"; }
    }

    public static synchronized void stop() {
        running = false;
        try { if (serverSocket != null) serverSocket.close(); } catch (Exception ignored) { }
        try { if (socket != null) socket.close(); } catch (Exception ignored) { }
        serverSocket = null;
        socket = null;
        output = null;
    }

    private static void notifyUnity(String method, String value) {
        try { UnityPlayer.UnitySendMessage(unityObjectName, method, value == null ? "" : value); }
        catch (Exception ignored) { }
    }
}
