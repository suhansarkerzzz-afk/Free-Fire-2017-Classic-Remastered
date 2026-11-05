using UnityEngine;

// Lightweight, asset-free third-person character silhouette for the prototype.
// All mesh parts are children of the gameplay root; the root CharacterController
// remains the only gameplay collider.
public class ClassicCharacterVisual : MonoBehaviour
{
    private Transform model;
    private Transform leftArm;
    private Transform rightArm;
    private Transform leftLeg;
    private Transform rightLeg;
    private Vector3 baseScale;

    private void Start()
    {
        Renderer placeholder = GetComponent<Renderer>();
        if (placeholder != null) placeholder.enabled = false;

        model = new GameObject("ClassicCharacterModel").transform;
        model.SetParent(transform, false);
        model.localPosition = Vector3.zero;

        Material shirt = MakeMaterial(new Color(0.12f, 0.30f, 0.62f));
        Material pants = MakeMaterial(new Color(0.12f, 0.15f, 0.22f));
        Material skin = MakeMaterial(new Color(0.72f, 0.48f, 0.32f));
        Material vest = MakeMaterial(new Color(0.20f, 0.24f, 0.22f));
        Material boots = MakeMaterial(new Color(0.08f, 0.07f, 0.06f));

        CreatePart(PrimitiveType.Capsule, "Torso", model, new Vector3(0f, 0.05f, 0f),
            new Vector3(0.62f, 0.78f, 0.36f), shirt);
        CreatePart(PrimitiveType.Cube, "FieldVest", model, new Vector3(0f, 0.12f, 0.19f),
            new Vector3(0.48f, 0.48f, 0.12f), vest);
        CreatePart(PrimitiveType.Sphere, "Head", model, new Vector3(0f, 0.92f, 0f),
            new Vector3(0.34f, 0.38f, 0.34f), skin);
        CreatePart(PrimitiveType.Cube, "Hair", model, new Vector3(0f, 1.10f, -0.015f),
            new Vector3(0.34f, 0.10f, 0.34f), boots);

        leftArm = CreateLimb("LeftArm", new Vector3(-0.39f, 0.38f, 0f), shirt, skin, true);
        rightArm = CreateLimb("RightArm", new Vector3(0.39f, 0.38f, 0f), shirt, skin, true);
        leftLeg = CreateLimb("LeftLeg", new Vector3(-0.17f, -0.48f, 0f), pants, boots, false);
        rightLeg = CreateLimb("RightLeg", new Vector3(0.17f, -0.48f, 0f), pants, boots, false);
        baseScale = model.localScale;
    }

    private Transform CreateLimb(string partName, Vector3 localPosition, Material upper,
        Material lower, bool arm)
    {
        Transform pivot = new GameObject(partName + "Pivot").transform;
        pivot.SetParent(model, false);
        pivot.localPosition = localPosition;
        float width = arm ? 0.19f : 0.23f;
        float length = arm ? 0.43f : 0.48f;
        CreatePart(PrimitiveType.Capsule, partName + "Upper", pivot,
            new Vector3(0f, -length * 0.48f, 0f), new Vector3(width, length * 0.62f, width), upper);
        CreatePart(PrimitiveType.Capsule, partName + "Lower", pivot,
            new Vector3(0f, -length * 1.20f, 0.015f), new Vector3(width * 0.82f, length * 0.58f, width * 0.82f), lower);
        if (!arm)
            CreatePart(PrimitiveType.Cube, partName + "Boot", pivot,
                new Vector3(0f, -length * 1.72f, 0.10f), new Vector3(0.25f, 0.16f, 0.38f), lower);
        return pivot;
    }

    private void Update()
    {
        if (model == null) return;
        float t = Time.time;
        float moving = 0f;
#if ENABLE_LEGACY_INPUT_MANAGER
        moving = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).magnitude;
#endif
        float swing = Mathf.Sin(t * (moving > 0.1f ? 10f : 2f)) * (moving > 0.1f ? 24f : 2f);
        if (leftArm != null) leftArm.localRotation = Quaternion.Euler(swing, 0f, -7f);
        if (rightArm != null) rightArm.localRotation = Quaternion.Euler(-swing, 0f, 7f);
        if (leftLeg != null) leftLeg.localRotation = Quaternion.Euler(-swing * 0.8f, 0f, 0f);
        if (rightLeg != null) rightLeg.localRotation = Quaternion.Euler(swing * 0.8f, 0f, 0f);
        float breathe = 1f + Mathf.Sin(t * 2.2f) * 0.008f;
        model.localScale = new Vector3(baseScale.x * breathe, baseScale.y, baseScale.z * breathe);
    }

    private static Material MakeMaterial(Color color)
    {
        Shader shader = Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
        Material material = new Material(shader);
        material.color = color;
        return material;
    }

    private static GameObject CreatePart(PrimitiveType type, string partName, Transform parent,
        Vector3 localPosition, Vector3 localScale, Material material)
    {
        GameObject part = GameObject.CreatePrimitive(type);
        part.name = partName;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = localPosition;
        part.transform.localScale = localScale;
        Collider collider = part.GetComponent<Collider>();
        if (collider != null) Destroy(collider);
        Renderer renderer = part.GetComponent<Renderer>();
        if (renderer != null) renderer.material = material;
        return part;
    }
}
