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
    }

    private bool cameraDistanceApplied;

    private void Update()
    {
        // The runtime camera is created by GameBootstrap; wait until it exists.
        if (cameraDistanceApplied || SystemInfo.systemMemorySize > 3072) return;
        Camera mainCamera = Camera.main;
        if (mainCamera == null) return;
        mainCamera.farClipPlane = 150f;
        cameraDistanceApplied = true;
    }

    private void OnLowMemory()
    {
        Resources.UnloadUnusedAssets();
        System.GC.Collect();
        Application.targetFrameRate = 25;
    }

    private void OnDestroy() { Application.lowMemory -= OnLowMemory; }
}
