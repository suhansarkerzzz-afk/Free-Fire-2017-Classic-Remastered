using UnityEngine;
using UnityEngine.UI;

// Lightweight inventory state for the offline prototype.
public class InventorySystem : MonoBehaviour
{
    public int medkits;
    public int ammoCrates;
    public int backpackLevel = 1;
    public int maxMedkits = 3;
    private Text inventoryLabel;
    private HealthSystem health;
    private PlayerCombat combat;

    private void Start()
    {
        health = GetComponent<HealthSystem>();
        combat = GetComponent<PlayerCombat>();
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            GameObject labelObject = new GameObject("InventoryReadout", typeof(RectTransform), typeof(Text));
            labelObject.transform.SetParent(canvas.transform, false);
            RectTransform rect = labelObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.76f);
            rect.anchorMax = new Vector2(0.5f, 0.76f);
            rect.sizeDelta = new Vector2(420f, 34f);
            inventoryLabel = labelObject.GetComponent<Text>();
            inventoryLabel.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            inventoryLabel.fontSize = 17;
            inventoryLabel.alignment = TextAnchor.MiddleCenter;
            inventoryLabel.color = Color.white;
        }
        Refresh();
    }

    public bool AddMedkit(int amount = 1)
    {
        if (amount <= 0 || medkits >= maxMedkits + backpackLevel - 1) return false;
        medkits = Mathf.Min(maxMedkits + backpackLevel - 1, medkits + amount);
        Refresh();
        return true;
    }

    public bool UseMedkit()
    {
        if (medkits <= 0 || health == null || health.IsDead || health.CurrentHealth >= health.maxHealth) return false;
        medkits--;
        health.Heal(45f);
        Refresh();
        return true;
    }

    public bool AddAmmo(int amount)
    {
        if (combat == null || !combat.AddReserveAmmo(amount)) return false;
        ammoCrates++;
        Refresh();
        return true;
    }

    public void UpgradeBackpack()
    {
        backpackLevel = Mathf.Clamp(backpackLevel + 1, 1, 3);
        Refresh();
    }

    private void Refresh()
    {
        if (inventoryLabel != null)
            inventoryLabel.text = "BAG  |  MEDKIT: " + medkits + "  |  AMMO CRATES: " + ammoCrates + "  |  LV." + backpackLevel;
    }
}
