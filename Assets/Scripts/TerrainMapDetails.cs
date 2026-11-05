using UnityEngine;

// Adds recognizable classic-island roads, river crossing and village props with primitives only.
public class TerrainMapDetails : MonoBehaviour
{
    private Material roadMat;
    private Material roadLineMat;
    private Material waterMat;
    private Material woodMat;
    private Material lampMat;

    private void Start()
    {
        roadMat = MakeMaterial(new Color(0.24f, 0.25f, 0.23f));
        roadLineMat = MakeMaterial(new Color(0.86f, 0.78f, 0.48f));
        waterMat = MakeMaterial(new Color(0.08f, 0.34f, 0.55f, 0.82f));
        woodMat = MakeMaterial(new Color(0.35f, 0.22f, 0.12f));
        lampMat = MakeMaterial(new Color(0.82f, 0.72f, 0.42f));
        BuildRoad();
        BuildRiverAndBridge();
        BuildVillageDetails();
    }

    private Material MakeMaterial(Color color)
    {
        Shader shader = Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
        Material material = new Material(shader);
        material.color = color;
        return material;
    }

    private GameObject Cube(string name, Vector3 position, Vector3 scale, Material material, bool collider = true)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.name = name;
        obj.transform.position = position;
        obj.transform.localScale = scale;
        obj.GetComponent<Renderer>().material = material;
        if (!collider)
        {
            Collider c = obj.GetComponent<Collider>();
            if (c != null) Destroy(c);
        }
        return obj;
    }

    private void BuildRoad()
    {
        Cube("Island_Main_Road", new Vector3(0f, 0.045f, -5f), new Vector3(108f, 0.09f, 5.2f), roadMat, false);
        Cube("Road_Center_Line", new Vector3(0f, 0.096f, -5f), new Vector3(104f, 0.012f, 0.12f), roadLineMat, false);
        for (int i = -4; i <= 4; i++)
        {
            float x = i * 11f;
            Cube("Road_Edge_Marker_L", new Vector3(x, 0.105f, -7.45f), new Vector3(0.22f, 0.08f, 0.22f), lampMat, false);
            Cube("Road_Edge_Marker_R", new Vector3(x, 0.105f, -2.55f), new Vector3(0.22f, 0.08f, 0.22f), lampMat, false);
        }
    }

    private void BuildRiverAndBridge()
    {
        Cube("Blue_River", new Vector3(-15f, 0.015f, 0f), new Vector3(7.2f, 0.12f, 108f), waterMat, false);
        Cube("River_Bank_West", new Vector3(-19f, 0.02f, 0f), new Vector3(1.0f, 0.14f, 108f), woodMat, false);
        Cube("River_Bank_East", new Vector3(-11f, 0.02f, 0f), new Vector3(1.0f, 0.14f, 108f), woodMat, false);
        for (int i = 0; i < 11; i++)
        {
            float z = -10f + i * 0.95f;
            Cube("Bridge_Deck_" + i, new Vector3(-15f, 0.28f, z), new Vector3(10.5f, 0.28f, 0.82f), woodMat, true);
        }
        Cube("Bridge_Rail_Left", new Vector3(-15f, 0.82f, -0.1f), new Vector3(10.5f, 0.16f, 0.16f), woodMat, true);
        Cube("Bridge_Rail_Right", new Vector3(-15f, 0.82f, -9.2f), new Vector3(10.5f, 0.16f, 0.16f), woodMat, true);
    }

    private void BuildVillageDetails()
    {
        for (int i = 0; i < 6; i++)
        {
            float x = -30f + i * 12f;
            GameObject lamp = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            lamp.name = "Village_Lamp_Post_" + i;
            lamp.transform.position = new Vector3(x, 2.1f, -1.5f);
            lamp.transform.localScale = new Vector3(0.16f, 2.1f, 0.16f);
            lamp.GetComponent<Renderer>().material = woodMat;
            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Village_Lamp_Head_" + i;
            head.transform.position = new Vector3(x, 4.2f, -1.5f);
            head.transform.localScale = Vector3.one * 0.42f;
            head.GetComponent<Renderer>().material = lampMat;
            Collider a = lamp.GetComponent<Collider>(); if (a != null) Destroy(a);
            Collider b = head.GetComponent<Collider>(); if (b != null) Destroy(b);
        }

        for (int i = 0; i < 12; i++)
        {
            float x = -28f + (i % 6) * 10f;
            float z = (i / 6 == 0) ? 24f : -28f;
            GameObject crate = Cube("Village_Supply_Crate_" + i, new Vector3(x, 0.65f, z), new Vector3(1.3f, 1.3f, 1.3f), woodMat);
            crate.transform.rotation = Quaternion.Euler(0f, (i * 17f) % 90f, 0f);
        }
    }
}
