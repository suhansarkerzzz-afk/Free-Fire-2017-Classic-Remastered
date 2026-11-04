using UnityEngine;

public class BotCombat : MonoBehaviour
{
    public Transform target;
    public float attackRange = 12f;
    public float damage = 8f;
    public float attackInterval = 1.5f;
    private float nextAttack;

    private void Update()
    {
        if (target == null || Time.time < nextAttack) return;
        float distance = Vector3.Distance(transform.position, target.position);
        if (distance <= attackRange)
        {
            HealthSystem health = target.GetComponent<HealthSystem>();
            if (health != null) health.TakeDamage(damage);
            nextAttack = Time.time + attackInterval;
        }
    }
}
