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
        public bool unlocked;
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
            new WeaponData { weaponName = "RIFLE", damage = 25f, range = 55f, fireCooldown = 0.16f, magazineSize = 30, reserveAmmo = 120, reloadDuration = 1.6f, unlocked = true },
            new WeaponData { weaponName = "PISTOL", damage = 40f, range = 32f, fireCooldown = 0.38f, magazineSize = 12, reserveAmmo = 48, reloadDuration = 1.2f, unlocked = true },
            new WeaponData { weaponName = "SMG", damage = 17f, range = 38f, fireCooldown = 0.09f, magazineSize = 35, reserveAmmo = 105, reloadDuration = 1.5f },
            new WeaponData { weaponName = "SHOTGUN", damage = 13f, range = 18f, fireCooldown = 0.8f, magazineSize = 6, reserveAmmo = 30, reloadDuration = 2.0f },
            new WeaponData { weaponName = "SNIPER", damage = 75f, range = 95f, fireCooldown = 1.1f, magazineSize = 5, reserveAmmo = 25, reloadDuration = 2.2f }
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

    public bool AddWeapon(string weaponName)
    {
        if (weapons == null || string.IsNullOrEmpty(weaponName)) return false;
        for (int i = 0; i < weapons.Length; i++)
        {
            if (!string.Equals(weapons[i].weaponName, weaponName, StringComparison.OrdinalIgnoreCase)) continue;
            if (weapons[i].unlocked)
            {
                weapons[i].reserveAmmo = Mathf.Min(240, weapons[i].reserveAmmo + 30);
                if (i == activeWeaponIndex) NotifyAmmo();
                return true;
            }
            weapons[i].unlocked = true;
            weapons[i].currentAmmo = weapons[i].magazineSize;
            activeWeaponIndex = i;
            NotifyWeapon();
            NotifyAmmo();
            return true;
        }
        return false;
    }

    public bool AddReserveAmmo(int amount)
    {
        if (weapons == null || amount <= 0) return false;
        WeaponData weapon = ActiveWeapon;
        if (weapon.reserveAmmo >= 240) return false;
        weapon.reserveAmmo = Mathf.Min(240, weapon.reserveAmmo + amount);
        NotifyAmmo();
        return true;
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
        for (int step = 1; step <= weapons.Length; step++)
        {
            int candidate = (activeWeaponIndex + step) % weapons.Length;
            if (weapons[candidate].unlocked) { activeWeaponIndex = candidate; break; }
        }
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
