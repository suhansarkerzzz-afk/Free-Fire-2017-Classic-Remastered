using UnityEngine;

// Simple selectable cosmetic presets; no third-party character assets required.
public static class CharacterLoadout
{
    public static int SelectedCharacter { get; private set; } = Mathf.Clamp(PlayerPrefs.GetInt("Classic_Character", 0), 0, 3);
    public static void Select(int index)
    {
        SelectedCharacter = Mathf.Clamp(index, 0, 3);
        PlayerPrefs.SetInt("Classic_Character", SelectedCharacter);
        PlayerPrefs.Save();
    }
    public static Color GetPrimaryColor()
    {
        switch (SelectedCharacter)
        {
            case 1: return new Color(0.18f, 0.62f, 0.38f);
            case 2: return new Color(0.72f, 0.34f, 0.16f);
            case 3: return new Color(0.36f, 0.34f, 0.72f);
            default: return new Color(0.1f, 0.45f, 0.9f);
        }
    }
}
