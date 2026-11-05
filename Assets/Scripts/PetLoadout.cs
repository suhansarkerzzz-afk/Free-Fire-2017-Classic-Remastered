using UnityEngine;

public static class PetLoadout
{
    private static readonly string[] PetNames = { "CLASSIC FOX", "FIELD HAWK", "DESERT CUB" };
    public static int SelectedPet { get; private set; }
    public static void Select(int index) { SelectedPet = Mathf.Clamp(index, 0, PetNames.Length - 1); }
    public static string CurrentName { get { return PetNames[SelectedPet]; } }
}
