using UnityEngine;

// World pickup that grants a weapon slot or ammunition when approached.
public class WeaponPickup : MonoBehaviour
{
    public string weaponName = "RIFLE";
    public int ammoAmount = 45;
    public float pickupRadius = 2.3f;
    private Transform player;
    private void Start()
    {
        GameObject found = GameObject.Find("Player");
        if (found != null) player = found.transform;
    }
    private void Update()
    {
        transform.Rotate(Vector3.up, 75f * Time.deltaTime, Space.World);
        if (player == null || Vector3.Distance(player.position, transform.position) > pickupRadius) return;
        PlayerCombat combat = player.GetComponent<PlayerCombat>();
        if (combat == null) return;
        if (!combat.AddWeapon(weaponName, ammoAmount) && !combat.AddReserveAmmo(ammoAmount)) return;
        Destroy(gameObject);
    }
}
