using UnityEngine;
using UnityEngine.UI;

public class HealthSystem : MonoBehaviour
{
    public float maxHealth = 100f;
    public float CurrentHealth { get; private set; }
    public bool IsDead { get; private set; }
    public System.Action<float, float> OnHealthChanged;
    public System.Action OnDied;

    private void Awake() { CurrentHealth = maxHealth; }

    public void TakeDamage(float amount)
    {
        if (IsDead || amount <= 0f) return;
        CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        if (CurrentHealth <= 0f)
        {
            IsDead = true;
            OnDied?.Invoke();
        }
    }

    public void Heal(float amount)
    {
        if (IsDead || amount <= 0f) return;
        CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }
}
