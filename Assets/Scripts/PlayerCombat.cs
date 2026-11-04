using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public float damage = 35f;
    public float range = 45f;
    public float fireCooldown = 0.25f;
    private float nextFireTime;
    private Camera aimCamera;

    private void Start() { aimCamera = Camera.main; }

    private void Update()
    {
#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetButtonDown("Fire1")) Fire();
#endif
    }

    public void Fire()
    {
        if (Time.time < nextFireTime) return;
        nextFireTime = Time.time + fireCooldown;
        if (aimCamera == null) aimCamera = Camera.main;
        if (aimCamera == null) return;

        Ray ray = aimCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            HealthSystem health = hit.collider.GetComponentInParent<HealthSystem>();
            if (health != null && health.gameObject != gameObject)
                health.TakeDamage(damage);
            Debug.DrawLine(ray.origin, hit.point, Color.red, 0.5f);
        }
        else Debug.DrawRay(ray.origin, ray.direction * range, Color.red, 0.5f);
    }
}
