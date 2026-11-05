using UnityEngine;

public class LootPickup : MonoBehaviour
{
    public enum LootKind { Ammo, Medkit }
    public LootKind kind;
    public float pickupDistance = 2.1f;
    private Transform player;
    private float spinSpeed = 65f;

    private void Start()
    {
        GameObject playerObject = GameObject.Find("Player");
        if (playerObject != null) player = playerObject.transform;
    }

    private void Update()
    {
        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.World);
        if (player == null) return;
        if (Vector3.Distance(player.position, transform.position) > pickupDistance) return;
        PlayerCombat combat = player.GetComponent<PlayerCombat>();
        HealthSystem health = player.GetComponent<HealthSystem>();
        if (kind == LootKind.Ammo)
        {
            if (combat == null || !combat.AddReserveAmmo(30)) return;
        }
        else
        {
            if (health == null || health.IsDead || health.CurrentHealth >= health.maxHealth) return;
            health.Heal(35f);
        }
        Destroy(gameObject);
    }
}
