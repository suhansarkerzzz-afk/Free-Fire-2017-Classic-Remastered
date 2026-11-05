using System.Collections;
using UnityEngine;

// Procedural, original low-poly weapon presentation for the prototype.
public class WeaponView : MonoBehaviour
{
    private PlayerCombat combat;
    private Transform weaponRoot;
    private Vector3 basePosition;
    private Quaternion baseRotation;
    private GameObject muzzleFlash;
    private Light muzzleLight;
    private AudioSource audioSource;
    private AudioClip rifleClip;
    private AudioClip pistolClip;
    private float recoil;
    private Coroutine flashRoutine;

    private void Start()
    {
        combat = FindObjectOfType<PlayerCombat>();
        if (combat == null) return;

        audioSource = gameObject.GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        rifleClip = CreateShotClip("Rifle_Shot", 115f, 0.105f, 0.55f, 32f);
        pistolClip = CreateShotClip("Pistol_Shot", 185f, 0.14f, 0.65f, 24f);

        combat.OnFired += PlayShot;
        combat.OnWeaponChanged += BuildWeapon;
        BuildWeapon(combat.CurrentWeaponName);
    }

    private void OnDestroy()
    {
        if (combat != null)
        {
            combat.OnFired -= PlayShot;
            combat.OnWeaponChanged -= BuildWeapon;
        }
    }

    private void Update()
    {
        if (weaponRoot == null) return;
        recoil = Mathf.Lerp(recoil, 0f, Time.deltaTime * 12f);
        weaponRoot.localPosition = basePosition + new Vector3(0f, recoil * 0.22f, -recoil);
        weaponRoot.localRotation = baseRotation * Quaternion.Euler(-recoil * 24f, 0f, 0f);
    }

    private void BuildWeapon(string weaponName)
    {
        if (weaponRoot != null) Destroy(weaponRoot.gameObject);

        weaponRoot = new GameObject("Procedural_" + weaponName + "_View").transform;
        weaponRoot.SetParent(transform, false);
        basePosition = new Vector3(0.42f, -0.34f, 0.78f);
        baseRotation = Quaternion.identity;
        weaponRoot.localPosition = basePosition;
        weaponRoot.localRotation = baseRotation;

        bool rifle = weaponName == "RIFLE";
        Color body = new Color(0.12f, 0.15f, 0.17f);
        Color grip = new Color(0.22f, 0.19f, 0.14f);
        Color detail = new Color(0.38f, 0.4f, 0.38f);

        Part("Receiver", PrimitiveType.Cube, new Vector3(0f, 0f, 0f), new Vector3(rifle ? 0.16f : 0.13f, 0.13f, rifle ? 0.42f : 0.25f), body);
        Part("Barrel", PrimitiveType.Cylinder, new Vector3(0f, 0.015f, rifle ? 0.29f : 0.19f), new Vector3(0.035f, rifle ? 0.19f : 0.12f, 0.035f), detail, new Vector3(90f, 0f, 0f));
        Part("Grip", PrimitiveType.Cube, new Vector3(0f, -0.13f, -0.08f), new Vector3(0.095f, 0.22f, 0.11f), grip, new Vector3(-12f, 0f, 0f));

        if (rifle)
        {
            Part("Handguard", PrimitiveType.Cube, new Vector3(0f, 0.005f, 0.22f), new Vector3(0.12f, 0.09f, 0.24f), grip);
            Part("Magazine", PrimitiveType.Cube, new Vector3(0f, -0.16f, 0.02f), new Vector3(0.09f, 0.2f, 0.13f), body, new Vector3(8f, 0f, 0f));
            Part("Stock", PrimitiveType.Cube, new Vector3(0f, -0.015f, -0.29f), new Vector3(0.13f, 0.1f, 0.18f), grip);
            Part("Sight", PrimitiveType.Cube, new Vector3(0f, 0.095f, 0.02f), new Vector3(0.045f, 0.07f, 0.09f), detail);
        }
        else
        {
            Part("Slide", PrimitiveType.Cube, new Vector3(0f, 0.075f, 0.015f), new Vector3(0.12f, 0.055f, 0.25f), detail);
            Part("MagazineBase", PrimitiveType.Cube, new Vector3(0f, -0.18f, -0.035f), new Vector3(0.085f, 0.045f, 0.08f), body);
        }

        GameObject flash = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        flash.name = "MuzzleFlash";
        flash.transform.SetParent(weaponRoot, false);
        flash.transform.localPosition = new Vector3(0f, 0.02f, rifle ? 0.48f : 0.32f);
        flash.transform.localScale = new Vector3(0.11f, 0.11f, 0.2f);
        flash.GetComponent<Renderer>().material.color = new Color(1f, 0.72f, 0.18f);
        Collider flashCollider = flash.GetComponent<Collider>();
        if (flashCollider != null) Destroy(flashCollider);
        muzzleFlash = flash;
        muzzleFlash.SetActive(false);

        GameObject lightObject = new GameObject("MuzzleFlashLight");
        lightObject.transform.SetParent(weaponRoot, false);
        lightObject.transform.localPosition = flash.transform.localPosition;
        muzzleLight = lightObject.AddComponent<Light>();
        muzzleLight.type = LightType.Point;
        muzzleLight.range = 3f;
        muzzleLight.intensity = 2.5f;
        muzzleLight.color = new Color(1f, 0.58f, 0.2f);
        muzzleLight.enabled = false;
    }

    private void Part(string partName, PrimitiveType shape, Vector3 position, Vector3 scale, Color color, Vector3? rotation = null)
    {
        GameObject part = GameObject.CreatePrimitive(shape);
        part.name = partName;
        part.transform.SetParent(weaponRoot, false);
        part.transform.localPosition = position;
        part.transform.localScale = scale;
        if (rotation.HasValue) part.transform.localEulerAngles = rotation.Value;
        Renderer renderer = part.GetComponent<Renderer>();
        renderer.material.color = color;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        Collider collider = part.GetComponent<Collider>();
        if (collider != null) Destroy(collider);
    }

    private void PlayShot()
    {
        if (weaponRoot == null) return;
        bool rifle = combat != null && combat.CurrentWeaponName == "RIFLE";
        recoil = rifle ? 0.12f : 0.19f;
        if (audioSource != null) audioSource.PlayOneShot(rifle ? rifleClip : pistolClip, 0.65f);

        if (muzzleFlash != null)
        {
            muzzleFlash.SetActive(true);
            if (muzzleLight != null) muzzleLight.enabled = true;
            if (flashRoutine != null) StopCoroutine(flashRoutine);
            flashRoutine = StartCoroutine(HideFlash());
        }
    }

    private IEnumerator HideFlash()
    {
        yield return new WaitForSeconds(0.055f);
        if (muzzleFlash != null) muzzleFlash.SetActive(false);
        if (muzzleLight != null) muzzleLight.enabled = false;
        flashRoutine = null;
    }

    private AudioClip CreateShotClip(string clipName, float frequency, float duration, float volume, float decay)
    {
        const int sampleRate = 22050;
        int count = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[count];
        float phase = 0f;
        for (int i = 0; i < count; i++)
        {
            float t = i / (float)sampleRate;
            float envelope = Mathf.Exp(-t * decay);
            phase += 2f * Mathf.PI * frequency / sampleRate;
            float tone = Mathf.Sin(phase) * 0.32f;
            float noise = (Random.value * 2f - 1f) * 0.68f;
            samples[i] = (tone + noise) * envelope * volume;
        }
        AudioClip clip = AudioClip.Create(clipName, count, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
