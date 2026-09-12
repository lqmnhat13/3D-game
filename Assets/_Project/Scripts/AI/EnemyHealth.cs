using UnityEngine;

public sealed class EnemyHealth : MonoBehaviour
{
    [SerializeField, Min(1f)] private float maxHealth = 30f;
    [SerializeField] private float currentHealth;
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsDead => currentHealth <= 0f;

    private void Awake()
    {
        maxHealth = float.IsFinite(maxHealth) ? Mathf.Max(1f, maxHealth) : 30f;
        currentHealth = maxHealth;
    }

    public void Heal(float amount)
    {
        if (IsDead || !float.IsFinite(amount) || amount <= 0f) return;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
    }

    public void TakeDamage(float amount)
    {
        if (IsDead || !float.IsFinite(amount) || amount <= 0f) return;
        currentHealth = Mathf.Max(0f, currentHealth - amount);
        if (!IsDead) return;

        if (TryGetComponent<EnemyController>(out var controller)) controller.Die();
        gameObject.SetActive(false);
    }
}
