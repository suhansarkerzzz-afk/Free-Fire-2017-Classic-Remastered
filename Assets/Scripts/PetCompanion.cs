using UnityEngine;

// Cosmetic companion that follows the player without adding physics load.
public class PetCompanion : MonoBehaviour
{
    public string petName = "CLASSIC FOX";
    public float followDistance = 1.8f;
    public float followSpeed = 5f;
    private Transform owner;
    private Transform visual;
    private void Start()
    {
        GameObject player = GameObject.Find("Player");
        if (player != null) owner = player.transform;
        GameObject pet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        pet.name = petName;
        pet.transform.SetParent(transform, false);
        pet.transform.localScale = new Vector3(0.65f, 0.58f, 0.85f);
        pet.GetComponent<Renderer>().material.color = new Color(0.88f, 0.52f, 0.18f);
        Collider col = pet.GetComponent<Collider>();
        if (col != null) Destroy(col);
        visual = pet.transform;
        CreateEar(new Vector3(-0.22f, 0.28f, 0.1f));
        CreateEar(new Vector3(0.22f, 0.28f, 0.1f));
    }
    private void CreateEar(Vector3 localPosition)
    {
        GameObject ear = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        ear.name = "PetEar";
        ear.transform.SetParent(visual, false);
        ear.transform.localPosition = localPosition;
        ear.transform.localScale = new Vector3(0.28f, 0.42f, 0.25f);
        ear.GetComponent<Renderer>().material.color = new Color(0.55f, 0.25f, 0.08f);
        Collider col = ear.GetComponent<Collider>();
        if (col != null) Destroy(col);
    }
    private void Update()
    {
        if (owner == null) return;
        Vector3 desired = owner.position - owner.forward * followDistance + owner.right * 0.9f;
        desired.y = owner.position.y + 0.15f + Mathf.Sin(Time.time * 4f) * 0.12f;
        transform.position = Vector3.Lerp(transform.position, desired, Time.deltaTime * followSpeed);
        Vector3 look = owner.position - transform.position; look.y = 0f;
        if (look.sqrMagnitude > 0.01f) transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(look), Time.deltaTime * 5f);
    }
}
