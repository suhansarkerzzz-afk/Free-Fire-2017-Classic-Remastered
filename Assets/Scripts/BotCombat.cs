using UnityEngine;

public class BotCombat : MonoBehaviour
{
    public Transform target;
    public float attackRange = 30f;
    public float damage = 30f;
    public float attackInterval = 0.60f;
    public float aimSpread = 0.05f;
    public string weaponName = "RIFLE";
    private float nextAttack;

    // Same prototype weapon damage as the player, plus five damage per hit.
    public void ConfigureWeapon(string selectedWeapon)
    {
        weaponName = selectedWeapon;
        attackInterval = 0.60f;
        switch (selectedWeapon)
        {
            case "PISTOL": damage = 45f; attackRange = 30f; break;
            case "SMG": damage = 22f; attackRange = 28f; break;
            case "SHOTGUN": damage = 18f; attackRange = 20f; break;
            case "SNIPER": damage = 80f; attackRange = 40f; break;
            default: damage = 30f; attackRange = 40f; weaponName = "RIFLE"; break;
        }
    }

    private void Update()
    {
        HealthSystem ownHealth = GetComponent<HealthSystem>();
        if (ownHealth != null && ownHealth.IsDead) return;
        BasicBotAI botAI = GetComponent<BasicBotAI>();
        if (botAI != null) target = botAI.target;
        if (target == null || Time.time < nextAttack) return;
        HealthSystem targetHealth = target.GetComponent<HealthSystem>();
        if (targetHealth == null || targetHealth.IsDead) return;
        Vector3 origin = transform.position + Vector3.up * 0.8f;
        Vector3 destination = target.position + Vector3.up * 0.8f;
        Vector3 direction = destination - origin;
        if (direction.magnitude > attackRange) return;
        nextAttack = Time.time + attackInterval;
        if (Physics.Raycast(origin, direction.normalized, out RaycastHit hit, attackRange))
        {
            HealthSystem hitHealth = hit.collider.GetComponentInParent<HealthSystem>();
            if (hitHealth == targetHealth)
                targetHealth.TakeDamage(damage * Random.Range(1f - aimSpread, 1f + aimSpread), gameObject);
        }
    }
}