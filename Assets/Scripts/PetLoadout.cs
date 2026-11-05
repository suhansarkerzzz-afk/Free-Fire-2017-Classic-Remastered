using UnityEngine;

public static class PetLoadout
{
    private static readonly string[] PetNames = { "CLASSIC FOX", "FIELD HAWK", "DESERT CUB" };
    public static int SelectedPet { get; private set; } = Mathf.Clamp(PlayerPrefs.GetInt("Classic_Pet", 0), 0, PetNames.Length - 1);
    public static void Select(int index)
    {
        SelectedPet = Mathf.Clamp(index, 0, PetNames.Length - 1);
        PlayerPrefs.SetInt("Classic_Pet", SelectedPet);
        PlayerPrefs.Save();
    }
    public static string CurrentName { get { return PetNames[SelectedPet]; } }
    public static Color CurrentColor
    {
        get
        {
            if (SelectedPet == 1) return new Color(0.24f, 0.48f, 0.78f);
            if (SelectedPet == 2) return new Color(0.78f, 0.62f, 0.28f);
            return new Color(0.88f, 0.52f, 0.18f);
        }
    }
}
