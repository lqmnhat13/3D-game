using UnityEngine;

[RequireComponent(typeof(Light))]
public sealed class HearthController : MonoBehaviour
{
    [Header("Health")]
    [SerializeField, Min(0f)] private float maxHealth = 100f;
    [SerializeField, Min(0f)] private float currentHealth = 100f;

    [Header("Fuel")]
    [SerializeField, Min(0f)] private float maxFuel = 100f;
    [SerializeField, Min(0f)] private float currentFuel = 100f;

    [Header("Light")]
    [SerializeField, Min(0f)] private float minLightRadius = 4f;
    [SerializeField, Min(0f)] private float maxLightRadius = 15f;

    private Light hearthLight;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public float CurrentFuel => currentFuel;
    public float MaxFuel => maxFuel;
    public float CurrentLightRadius => hearthLight != null ? hearthLight.range : minLightRadius;
    public bool IsDestroyed => currentHealth <= 0f;

    private void Awake()
    {
        hearthLight = GetComponent<Light>();
        currentHealth = maxHealth;
        currentFuel = maxFuel;
        UpdateLightRadius();
    }

    private void OnValidate()
    {
        maxHealth = Mathf.Max(0f, maxHealth);
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        maxFuel = Mathf.Max(0f, maxFuel);
        currentFuel = Mathf.Clamp(currentFuel, 0f, maxFuel);
        minLightRadius = Mathf.Max(0f, minLightRadius);
        maxLightRadius = Mathf.Max(minLightRadius, maxLightRadius);

        hearthLight = GetComponent<Light>();
        UpdateLightRadius();
    }

    public void TakeDamage(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth - Mathf.Max(0f, amount), 0f, maxHealth);
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + Mathf.Max(0f, amount), 0f, maxHealth);
    }

    public void AddFuel(float amount)
    {
        currentFuel = Mathf.Clamp(currentFuel + Mathf.Max(0f, amount), 0f, maxFuel);
        UpdateLightRadius();
    }

    public void ConsumeFuel(float amount)
    {
        currentFuel = Mathf.Clamp(currentFuel - Mathf.Max(0f, amount), 0f, maxFuel);
        UpdateLightRadius();
    }

    public bool IsInsideLight(Vector3 worldPosition)
    {
        return Vector3.Distance(transform.position, worldPosition) <= CurrentLightRadius;
    }

    private void UpdateLightRadius()
    {
        if (hearthLight == null)
        {
            return;
        }

        float normalizedFuel = maxFuel > 0f ? currentFuel / maxFuel : 0f;
        hearthLight.range = Mathf.Lerp(minLightRadius, maxLightRadius, normalizedFuel);
    }
}
