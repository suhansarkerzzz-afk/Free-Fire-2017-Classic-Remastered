using UnityEngine;

// Lightweight, original low-poly POI pass for the classic Bermuda-inspired island.
// These are named districts inspired by the 2017 map; no proprietary game assets are used.
public class ClassicBermudaLocations : MonoBehaviour
{
    private static readonly string[] LocationNames =
    {
        "SHIPYARD", "BULLSEYE", "GRAVEYARD", "OBSERVATORY", "KATULISTIWA",
        "PLANTATION", "RIVERSIDE", "MILL", "KERATON", "CAPE TOWN",
        "HANGAR", "BIMASAKTI STRIP", "CLOCK TOWER", "FACTORY", "PEAK",
        "KOTA TUA", "POCHINOK", "RIM NAM VILLAGE", "MARS ELECTRIC", "SENTOSA"
    };

    private Material wallMat;
    private Material roofMat;
    private Material landmarkMat;
    private Material signMat;

    private void Start()
    {
        wallMat = MakeMaterial(new Color(0.58f, 0.53f, 0.43f));
        roofMat = MakeMaterial(new Color(0.25f, 0.20f, 0.17f));
        landmarkMat = MakeMaterial(new Color(0.32f, 0.39f, 0.42f));
        signMat = MakeMaterial(new Color(0.10f, 0.16f, 0.19f));
        BuildAllLocations();
    }

    private Material MakeMaterial(Color color)
    {
        Material material = new Material(Shader.Find("Standard"));
        material.color = color;
        return material;
    }

    private void BuildAllLocations()
    {
        // 5 columns x 4 rows spread over the existing 120 x 120 prototype island.
        for (int i = 0; i < LocationNames.Length; i++)
        {
            int column = i % 5;
            int row = i / 5;
            Vector3 center = new Vector3(-44f + column * 22f, 0f, 36f - row * 24f);
            BuildLocation(LocationNames[i], center, i);
        }
    }

    private void BuildLocation(string locationName, Vector3 center, int index)
    {
        int buildingCount = 2 + (index % 3);
        for (int b = 0; b < buildingCount; b++)
        {
            float x = (b % 2 == 0 ? -1f : 1f) * (3.2f + (b / 2) * 2.4f);
            float z = (b / 2) * 5.5f - 1.5f;
            Vector3 p = center + new Vector3(x, 0f, z);
            float width = 4.2f + (b % 2) * 1.2f;
            float depth = 4f + (b % 3) * 0.6f;
            GameObject building = GameObject.CreatePrimitive(PrimitiveType.Cube);
            building.name = locationName + "_Building_" + (b + 1);
            building.transform.position = p + Vector3.up * 1.65f;
            building.transform.localScale = new Vector3(width, 3.3f, depth);
            building.GetComponent<Renderer>().material = wallMat;

            GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roof.name = locationName + "_Roof_" + (b + 1);
            roof.transform.position = p + Vector3.up * 3.42f;
            roof.transform.localScale = new Vector3(width + 0.45f, 0.35f, depth + 0.45f);
            roof.GetComponent<Renderer>().material = roofMat;
        }

        // A distinct landmark silhouette makes each district recognizable from a distance.
        GameObject landmark = GameObject.CreatePrimitive(index % 4 == 0 ? PrimitiveType.Cylinder : PrimitiveType.Cube);
        landmark.name = locationName + "_Landmark";
        landmark.transform.position = center + new Vector3(0f, 2.2f, -7f);
        landmark.transform.localScale = index % 4 == 0
            ? new Vector3(2.4f, 2.2f, 2.4f)
            : new Vector3(3.2f, 4.4f, 2.2f);
        landmark.GetComponent<Renderer>().material = landmarkMat;

        GameObject sign = GameObject.CreatePrimitive(PrimitiveType.Cube);
        sign.name = locationName + "_Location_Sign";
        sign.transform.position = center + new Vector3(0f, 1.35f, 8f);
        sign.transform.localScale = new Vector3(8.5f, 1.5f, 0.25f);
        sign.GetComponent<Renderer>().material = signMat;

        GameObject labelObject = new GameObject(locationName + "_World_Label");
        labelObject.transform.position = sign.transform.position + Vector3.up * 0.02f + Vector3.back * 0.16f;
        TextMesh label = labelObject.AddComponent<TextMesh>();
        label.text = locationName;
        label.anchor = TextAnchor.MiddleCenter;
        label.alignment = TextAlignment.Center;
        label.fontSize = 48;
        label.characterSize = 0.12f;
        label.color = Color.white;
        label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        labelObject.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        Collider signCollider = sign.GetComponent<Collider>();
        if (signCollider != null) Destroy(signCollider);
    }
}