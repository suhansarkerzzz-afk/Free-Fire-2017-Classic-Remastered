using UnityEngine;

// Conservative runtime profile for low-memory Android phones, including 2 GB devices.
public class RuntimeOptimization : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ApplyEarly()
    {
        Application.targetFrameRate = SystemInfo.systemMemorySize <= 3072 ? 30 : 45;
        QualitySettings.vSyncCount = 0;
        QualitySettings.antiAliasing = 0;
        QualitySettings.shadowDistance = SystemInfo.systemMemorySize <= 3072 ? 18f : 32f;
        QualitySettings.pixelLightCount = SystemInfo.systemMemorySize <= 3072 ? 1 : 2;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    private void Awake()
    {
        Application.lowMemory += OnLowMemory;
        if (SystemInfo.systemMemorySize <= 3072)
        {
            QualitySettings.SetQualityLevel(0, true);
            if (Camera.main != null) Camera.main.farClipPlane = 150f;
        }
    }

    private void OnLowMemory()
    {
        Resources.UnloadUnusedAssets();
        System.GC.Collect();
        Application.targetFrameRate = 25;
    }

    private void OnDestroy() { Application.lowMemory -= OnLowMemory; }
}
