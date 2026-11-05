using UnityEngine;

// A simple shrinking survival circle for the offline prototype.
public class SafeZoneController : MonoBehaviour
{
    public Transform target;
    public float startingRadius = 54f;
    public float finalRadius = 9f;
    public float shrinkDuration = 300f;
    public float outsideDamagePerSecond = 5f;
    private float elapsed;
    private float damageTimer;
    private LineRenderer ring;
    private const int Segments = 96;

    private void Start()
    {
        GameObject ringObject = new GameObject("SafeZoneBoundary");
        ring = ringObject.AddComponent<LineRenderer>();
        ring.useWorldSpace = true;
        ring.loop = true;
        ring.positionCount = Segments;
        ring.startWidth = 0.28f;
        ring.endWidth = 0.28f;
        ring.material = new Material(Shader.Find("Sprites/Default"));
        ring.startColor = new Color(0.1f, 0.55f, 1f, 0.9f);
        ring.endColor = ring.startColor;
        ring.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        ring.receiveShadows = false;
    }

    private void Update()
    {
        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / Mathf.Max(1f, shrinkDuration));
        float radius = Mathf.Lerp(startingRadius, finalRadius, t);
        DrawRing(radius);

        damageTimer += Time.deltaTime;
        if (damageTimer < 1f) return;
        damageTimer = 0f;
        for (int i = 0; i < HealthSystem.ActiveActors.Count; i++)
        {
            HealthSystem health = HealthSystem.ActiveActors[i];
            if (health == null || health.IsDead) continue;
            Vector3 flat = health.transform.position;
            flat.y = 0f;
            if (flat.magnitude > radius) health.TakeDamage(outsideDamagePerSecond);
        }
    }

    private void DrawRing(float radius)
    {
        if (ring == null) return;
        for (int i = 0; i < Segments; i++)
        {
            float angle = i * Mathf.PI * 2f / Segments;
            ring.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, 0.12f, Mathf.Sin(angle) * radius));
        }
    }
}
