using UnityEngine;

// Real runtime graphics presets independent of Unity's optional QualitySettings asset.
public static class GraphicsProfile
{
    public static int CurrentLevel { get; private set; } = 1;

    public static void ApplySaved()
    {
        Apply(PlayerPrefs.GetInt("Classic_GraphicsLevel", 1), false);
    }

    public static void Apply(int level)
    {
        Apply(level, true);
    }

    private static void Apply(int level, bool save)
    {
        CurrentLevel = Mathf.Clamp(level, 0, 2);
        bool lowMemory = SystemInfo.systemMemorySize <= 3072;
        QualitySettings.vSyncCount = 0;
        QualitySettings.antiAliasing = 0;
        QualitySettings.softParticles = false;
        QualitySettings.realtimeReflectionProbes = false;

        if (CurrentLevel == 0)
        {
            Application.targetFrameRate = 30;
            QualitySettings.shadows = ShadowQuality.Disable;
            QualitySettings.shadowDistance = 0f;
            QualitySettings.pixelLightCount = 0;
            QualitySettings.lodBias = 0.65f;
            QualitySettings.maximumLODLevel = 1;
            QualitySettings.masterTextureLimit = 1;
        }
        else if (CurrentLevel == 1 || lowMemory)
        {
            Application.targetFrameRate = lowMemory ? 30 : 45;
            QualitySettings.shadows = lowMemory ? ShadowQuality.Disable : ShadowQuality.HardOnly;
            QualitySettings.shadowDistance = lowMemory ? 0f : 22f;
            QualitySettings.pixelLightCount = 1;
            QualitySettings.lodBias = 0.9f;
            QualitySettings.maximumLODLevel = 0;
            QualitySettings.masterTextureLimit = 0;
        }
        else
        {
            Application.targetFrameRate = 60;
            QualitySettings.shadows = ShadowQuality.All;
            QualitySettings.shadowDistance = 42f;
            QualitySettings.pixelLightCount = 3;
            QualitySettings.lodBias = 1.35f;
            QualitySettings.maximumLODLevel = 0;
            QualitySettings.masterTextureLimit = 0;
            QualitySettings.antiAliasing = 2;
        }

        if (save)
        {
            PlayerPrefs.SetInt("Classic_GraphicsLevel", CurrentLevel);
            PlayerPrefs.Save();
        }
        Debug.Log("Graphics profile applied: " + CurrentLabel + (lowMemory && CurrentLevel == 2 ? " (memory limited)" : ""));
    }

    public static string CurrentLabel
    {
        get { return CurrentLevel == 0 ? "LOW" : CurrentLevel == 1 ? "BALANCED" : "HIGH"; }
    }
}
