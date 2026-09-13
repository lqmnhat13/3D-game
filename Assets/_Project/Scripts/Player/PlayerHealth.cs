using UnityEngine;

public sealed class PlayerHealth : MonoBehaviour
{
    [SerializeField, Min(1f)] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsDead => currentHealth <= 0f;
    public event System.Action<PlayerHealth> Died;

    private void Awake()
    {
        maxHealth = float.IsFinite(maxHealth) ? Mathf.Max(1f, maxHealth) : 100f;
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (IsDead || !float.IsFinite(amount) || amount <= 0f) return;
        currentHealth = Mathf.Max(0f, currentHealth - amount);
        if (IsDead) Died?.Invoke(this);
    }

    public void Heal(float amount)
    {
        if (IsDead || !float.IsFinite(amount) || amount <= 0f) return;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
    }
}
