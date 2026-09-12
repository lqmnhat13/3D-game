using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(BoxCollider), typeof(NavMeshObstacle))]
public sealed class Fence : MonoBehaviour
{
    [SerializeField, Min(1f)] private float maxHealth = 50f;
    [SerializeField] private float currentHealth = 50f;
    private BoxCollider body;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsDestroyed => currentHealth <= 0f;
    public Vector3 ClosestPoint(Vector3 position) => body.ClosestPoint(position);

    private void Awake()
    {
        body = GetComponent<BoxCollider>();
        maxHealth = float.IsFinite(maxHealth) ? Mathf.Max(1f, maxHealth) : 50f;
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (IsDestroyed || !float.IsFinite(amount) || amount <= 0f) return;
        currentHealth = Mathf.Max(0f, currentHealth - amount);
        if (!IsDestroyed) return;
        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}
