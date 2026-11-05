using UnityEngine;

// Conservative runtime profile for low-memory Android phones, including 2 GB devices.
public class RuntimeOptimization : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ApplyEarly()
    {
        GraphicsProfile.ApplySaved();
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    private void Awake()
    {
        Application.lowMemory += OnLowMemory;
        if (SystemInfo.systemMemorySize <= 3072 && Camera.main != null)
            Camera.main.farClipPlane = 150f;
    }

    private void OnLowMemory()
    {
        Resources.UnloadUnusedAssets();
        System.GC.Collect();
        Application.targetFrameRate = 25;
    }

    private void OnDestroy() { Application.lowMemory -= OnLowMemory; }
}
