package com.classicremastered.bluetooth;

import android.app.Activity;
import android.bluetooth.BluetoothAdapter;
import android.bluetooth.BluetoothDevice;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.os.Build;

import com.unity3d.player.UnityPlayer;

public final class BluetoothDiscovery {
    private static BroadcastReceiver receiver;
    private static String unityObjectName = "BluetoothDiscoveryUI";

    private BluetoothDiscovery() { }

    public static synchronized String startDiscovery(Activity activity, String objectName) {
        if (activity == null) return "NO_ACTIVITY";
        BluetoothAdapter adapter = BluetoothAdapter.getDefaultAdapter();
        if (adapter == null) return "BLUETOOTH_UNAVAILABLE";
        unityObjectName = objectName == null ? "BluetoothDiscoveryUI" : objectName;

        try {
            if (Build.VERSION.SDK_INT >= 31) {
                if (activity.checkSelfPermission("android.permission.BLUETOOTH_SCAN") != android.content.pm.PackageManager.PERMISSION_GRANTED ||
                    activity.checkSelfPermission("android.permission.BLUETOOTH_CONNECT") != android.content.pm.PackageManager.PERMISSION_GRANTED)
                    return "PERMISSION_REQUIRED";
            } else if (Build.VERSION.SDK_INT >= 23 &&
                activity.checkSelfPermission("android.permission.ACCESS_FINE_LOCATION") != android.content.pm.PackageManager.PERMISSION_GRANTED) {
                return "PERMISSION_REQUIRED";
            }

            if (!adapter.isEnabled()) return "BLUETOOTH_OFF";
            if (receiver != null) {
                try { activity.unregisterReceiver(receiver); } catch (Exception ignored) { }
                receiver = null;
            }

            receiver = new BroadcastReceiver() {
                @Override public void onReceive(Context context, Intent intent) {
                    String action = intent.getAction();
                    if (BluetoothDevice.ACTION_FOUND.equals(action)) {
                        BluetoothDevice device = intent.getParcelableExtra(BluetoothDevice.EXTRA_DEVICE);
                        if (device != null) {
                            String name = "Unknown device";
                            try {
                                String foundName = device.getName();
                                if (foundName != null && !foundName.trim().isEmpty()) name = foundName;
                            } catch (SecurityException ignored) { }
                            String address = "";
                            try { address = device.getAddress(); } catch (SecurityException ignored) { }
                            UnityPlayer.UnitySendMessage(unityObjectName, "OnBluetoothDeviceFound", name + "|" + address);
                        }
                    } else if (BluetoothAdapter.ACTION_DISCOVERY_FINISHED.equals(action)) {
                        UnityPlayer.UnitySendMessage(unityObjectName, "OnBluetoothDiscoveryFinished", "");
                    }
                }
            };

            IntentFilter filter = new IntentFilter();
            filter.addAction(BluetoothDevice.ACTION_FOUND);
            filter.addAction(BluetoothAdapter.ACTION_DISCOVERY_FINISHED);
            if (Build.VERSION.SDK_INT >= 33)
                activity.registerReceiver(receiver, filter, Context.RECEIVER_NOT_EXPORTED);
            else
                activity.registerReceiver(receiver, filter);

            if (adapter.isDiscovering()) adapter.cancelDiscovery();
            return adapter.startDiscovery() ? "SCANNING" : "START_FAILED";
        } catch (SecurityException ex) {
            return "PERMISSION_REQUIRED";
        } catch (Exception ex) {
            return "ERROR:" + ex.getClass().getSimpleName();
        }
    }

    public static synchronized void stopDiscovery(Activity activity) {
        try {
            BluetoothAdapter adapter = BluetoothAdapter.getDefaultAdapter();
            if (adapter != null && adapter.isDiscovering()) adapter.cancelDiscovery();
            if (activity != null && receiver != null) activity.unregisterReceiver(receiver);
        } catch (Exception ignored) { }
        receiver = null;
    }
}
