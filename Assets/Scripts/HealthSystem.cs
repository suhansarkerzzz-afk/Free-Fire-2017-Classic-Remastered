using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HealthSystem : MonoBehaviour
{
    public static readonly List<HealthSystem> ActiveActors = new List<HealthSystem>();
    public float maxHealth = 100f;
    public float CurrentHealth { get; private set; }
    public bool IsDead { get; private set; }
    public GameObject LastDamageSource { get; private set; }
    public System.Action<float, float> OnHealthChanged;
    public System.Action OnDied;

    private void Awake()
    {
        CurrentHealth = maxHealth;
        if (!ActiveActors.Contains(this)) ActiveActors.Add(this);
    }

    private void OnDestroy() { ActiveActors.Remove(this); }

    public void TakeDamage(float amount, GameObject source = null)
    {
        if (IsDead || amount <= 0f) return;
        LastDamageSource = source;
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
