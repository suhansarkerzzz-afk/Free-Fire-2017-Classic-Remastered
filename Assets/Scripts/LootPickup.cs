using UnityEngine;

public class LootPickup : MonoBehaviour
{
    public enum LootKind { Ammo, Medkit, Backpack }
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
        InventorySystem inventory = player.GetComponent<InventorySystem>();
        if (kind == LootKind.Ammo)
        {
            if (inventory != null)
            {
                if (!inventory.AddAmmo(30)) return;
            }
            else if (combat == null || !combat.AddReserveAmmo(30)) return;
        }
        else if (kind == LootKind.Medkit)
        {
            if (inventory != null)
            {
                if (!inventory.AddMedkit()) return;
            }
            else
            {
                if (health == null || health.IsDead || health.CurrentHealth >= health.maxHealth) return;
                health.Heal(35f);
            }
        }
        else
        {
            if (inventory == null || inventory.backpackLevel >= 3) return;
            inventory.UpgradeBackpack();
        }
        Destroy(gameObject);
    }
}
