using UnityEngine;

public class BotCombat : MonoBehaviour
{
    public Transform target;
    public float attackRange = 30f;
    public float damage = 20f;
    public float attackInterval = 0.75f;
    public float aimSpread = 0.05f;
    private float nextAttack;

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
        float distance = direction.magnitude;
        if (distance > attackRange) return;

        nextAttack = Time.time + attackInterval;
        if (Physics.Raycast(origin, direction.normalized, out RaycastHit hit, attackRange))
        {
            HealthSystem hitHealth = hit.collider.GetComponentInParent<HealthSystem>();
            if (hitHealth == targetHealth)
                targetHealth.TakeDamage(damage * Random.Range(1f - aimSpread, 1f + aimSpread), gameObject);
        }
    }
}
