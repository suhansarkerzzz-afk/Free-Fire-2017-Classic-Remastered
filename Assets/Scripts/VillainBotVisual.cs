using UnityEngine;

// Asset-free villain silhouette: dark armour, red warning marks and glowing red visor.
public class VillainBotVisual : MonoBehaviour
{
    private void Start()
    {
        Renderer placeholder = GetComponent<Renderer>();
        if (placeholder != null) placeholder.enabled = false;
        Transform root = new GameObject("VillainBotModel").transform;
        root.SetParent(transform, false);
        Material armour = MakeMaterial(new Color(0.055f, 0.065f, 0.085f));
        Material darkMetal = MakeMaterial(new Color(0.12f, 0.13f, 0.16f));
        Material red = MakeMaterial(new Color(0.72f, 0.025f, 0.035f));
        Material visor = MakeMaterial(new Color(1f, 0.015f, 0.02f));
        visor.EnableKeyword("_EMISSION");
        visor.SetColor("_EmissionColor", new Color(1f, 0f, 0f) * 2.2f);
        Part(PrimitiveType.Capsule, "ArmouredTorso", root, new Vector3(0f, 0.05f, 0f), new Vector3(0.68f, 0.82f, 0.42f), armour);
        Part(PrimitiveType.Cube, "ChestPlate", root, new Vector3(0f, 0.18f, 0.20f), new Vector3(0.52f, 0.42f, 0.12f), darkMetal);
        Part(PrimitiveType.Sphere, "Helmet", root, new Vector3(0f, 0.94f, 0f), new Vector3(0.43f, 0.42f, 0.40f), darkMetal);
        Part(PrimitiveType.Cube, "RedVisor", root, new Vector3(0f, 0.98f, 0.205f), new Vector3(0.32f, 0.075f, 0.035f), visor);
        Part(PrimitiveType.Cube, "LeftShoulderMark", root, new Vector3(-0.39f, 0.40f, 0.02f), new Vector3(0.22f, 0.20f, 0.25f), red);
        Part(PrimitiveType.Cube, "RightShoulderMark", root, new Vector3(0.39f, 0.40f, 0.02f), new Vector3(0.22f, 0.20f, 0.25f), red);
        Part(PrimitiveType.Capsule, "LeftLeg", root, new Vector3(-0.17f, -0.49f, 0f), new Vector3(0.24f, 0.49f, 0.25f), armour);
        Part(PrimitiveType.Capsule, "RightLeg", root, new Vector3(0.17f, -0.49f, 0f), new Vector3(0.24f, 0.49f, 0.25f), armour);
        Part(PrimitiveType.Cube, "BackWarningStripe", root, new Vector3(0f, 0.20f, -0.23f), new Vector3(0.36f, 0.10f, 0.035f), red);
    }
    private static Material MakeMaterial(Color color)
    {
        Shader shader = Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
        Material material = new Material(shader); material.color = color; return material;
    }
    private static void Part(PrimitiveType type, string partName, Transform parent, Vector3 localPosition, Vector3 localScale, Material material)
    {
        GameObject part = GameObject.CreatePrimitive(type); part.name = partName;
        part.transform.SetParent(parent, false); part.transform.localPosition = localPosition; part.transform.localScale = localScale;
        Collider collider = part.GetComponent<Collider>(); if (collider != null) Destroy(collider);
        Renderer renderer = part.GetComponent<Renderer>(); if (renderer != null) renderer.material = material;
    }
}