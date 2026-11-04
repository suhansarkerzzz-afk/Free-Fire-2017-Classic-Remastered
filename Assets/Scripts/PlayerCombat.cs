using System;
using System.Collections;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public float damage = 35f;
    public float range = 45f;
    public float fireCooldown = 0.25f;
    public int magazineSize = 30;
    public int reserveAmmo = 90;
    public float reloadDuration = 1.5f;

    public event Action<int, int> OnAmmoChanged;
    public event Action<bool> OnReloadStateChanged;

    private int currentAmmo;
    private float nextFireTime;
    private Camera aimCamera;
    private bool isReloading;

    private void Start()
    {
        aimCamera = Camera.main;
        currentAmmo = magazineSize;
        NotifyAmmo();
    }

    private void Update()
    {
#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetButtonDown("Fire1")) Fire();
        if (Input.GetKeyDown(KeyCode.R)) Reload();
#endif
    }

    public void Fire()
    {
        if (isReloading || Time.time < nextFireTime) return;
        if (currentAmmo <= 0)
        {
            Reload();
            return;
        }

        nextFireTime = Time.time + fireCooldown;
        currentAmmo--;
        NotifyAmmo();

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

    public void Reload()
    {
        if (isReloading || currentAmmo >= magazineSize || reserveAmmo <= 0) return;
        StartCoroutine(ReloadRoutine());
    }

    private IEnumerator ReloadRoutine()
    {
        isReloading = true;
        OnReloadStateChanged?.Invoke(true);
        yield return new WaitForSeconds(reloadDuration);

        int needed = magazineSize - currentAmmo;
        int loaded = Mathf.Min(needed, reserveAmmo);
        currentAmmo += loaded;
        reserveAmmo -= loaded;

        isReloading = false;
        NotifyAmmo();
        OnReloadStateChanged?.Invoke(false);
    }

    private void NotifyAmmo()
    {
        OnAmmoChanged?.Invoke(currentAmmo, reserveAmmo);
    }
}
