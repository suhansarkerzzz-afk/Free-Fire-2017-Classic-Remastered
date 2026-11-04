using System;
using System.Collections;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Serializable]
    private class WeaponData
    {
        public string weaponName;
        public float damage;
        public float range;
        public float fireCooldown;
        public int magazineSize;
        public int reserveAmmo;
        public float reloadDuration;
        [NonSerialized] public int currentAmmo;
    }

    public event Action<int, int> OnAmmoChanged;
    public event Action<bool> OnReloadStateChanged;
    public event Action<string> OnWeaponChanged;
    public event Action OnFired;

    public string CurrentWeaponName { get { return weapons != null && weapons.Length > 0 ? ActiveWeapon.weaponName : "RIFLE"; } }

    private WeaponData[] weapons;
    private int activeWeaponIndex;
    private float nextFireTime;
    private Camera aimCamera;
    private bool isReloading;

    private WeaponData ActiveWeapon { get { return weapons[activeWeaponIndex]; } }

    private void Start()
    {
        aimCamera = Camera.main;
        weapons = new WeaponData[]
        {
            new WeaponData { weaponName = "RIFLE", damage = 25f, range = 55f, fireCooldown = 0.16f, magazineSize = 30, reserveAmmo = 120, reloadDuration = 1.6f },
            new WeaponData { weaponName = "PISTOL", damage = 40f, range = 32f, fireCooldown = 0.38f, magazineSize = 12, reserveAmmo = 48, reloadDuration = 1.2f }
        };
        foreach (WeaponData weapon in weapons) weapon.currentAmmo = weapon.magazineSize;
        NotifyWeapon();
        NotifyAmmo();
    }

    private void Update()
    {
#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetButtonDown("Fire1")) Fire();
        if (Input.GetKeyDown(KeyCode.R)) Reload();
        if (Input.GetKeyDown(KeyCode.Q)) SwitchWeapon();
#endif
    }

    public void Fire()
    {
        if (weapons == null || isReloading || Time.time < nextFireTime) return;
        WeaponData weapon = ActiveWeapon;
        if (weapon.currentAmmo <= 0)
        {
            Reload();
            return;
        }

        nextFireTime = Time.time + weapon.fireCooldown;
        weapon.currentAmmo--;
        OnFired?.Invoke();
        NotifyAmmo();

        if (aimCamera == null) aimCamera = Camera.main;
        if (aimCamera == null) return;

        Ray ray = aimCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out RaycastHit hit, weapon.range))
        {
            HealthSystem health = hit.collider.GetComponentInParent<HealthSystem>();
            if (health != null && health.gameObject != gameObject)
                health.TakeDamage(weapon.damage);
            Debug.DrawLine(ray.origin, hit.point, Color.red, 0.5f);
        }
        else Debug.DrawRay(ray.origin, ray.direction * weapon.range, Color.red, 0.5f);
    }

    public void Reload()
    {
        if (weapons == null || isReloading || ActiveWeapon.currentAmmo >= ActiveWeapon.magazineSize || ActiveWeapon.reserveAmmo <= 0) return;
        StartCoroutine(ReloadRoutine());
    }

    private IEnumerator ReloadRoutine()
    {
        isReloading = true;
        OnReloadStateChanged?.Invoke(true);
        yield return new WaitForSeconds(ActiveWeapon.reloadDuration);

        WeaponData weapon = ActiveWeapon;
        int needed = weapon.magazineSize - weapon.currentAmmo;
        int loaded = Mathf.Min(needed, weapon.reserveAmmo);
        weapon.currentAmmo += loaded;
        weapon.reserveAmmo -= loaded;

        isReloading = false;
        NotifyAmmo();
        OnReloadStateChanged?.Invoke(false);
    }

    public void SwitchWeapon()
    {
        if (weapons == null || weapons.Length < 2) return;
        StopAllCoroutines();
        isReloading = false;
        OnReloadStateChanged?.Invoke(false);
        activeWeaponIndex = (activeWeaponIndex + 1) % weapons.Length;
        nextFireTime = 0f;
        NotifyWeapon();
        NotifyAmmo();
    }

    private void NotifyWeapon()
    {
        if (weapons != null && weapons.Length > 0)
            OnWeaponChanged?.Invoke(ActiveWeapon.weaponName);
    }

    private void NotifyAmmo()
    {
        if (weapons != null && weapons.Length > 0)
            OnAmmoChanged?.Invoke(ActiveWeapon.currentAmmo, ActiveWeapon.reserveAmmo);
    }
}
