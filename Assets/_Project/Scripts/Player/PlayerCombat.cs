using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerHealth), typeof(PlayerInput))]
public sealed class PlayerCombat : MonoBehaviour
{
    [SerializeField, Min(0f)] private float attackDamage = 15f;
    [SerializeField, Min(0.1f)] private float attackRange = 2f;
    [SerializeField, Min(0.1f)] private float attackCooldown = 0.6f;
    private readonly Collider[] nearby = new Collider[32];
    private PlayerHealth health;
    private BuildingSystem building;
    private float nextAttackTime;

    private void Awake()
    {
        health = GetComponent<PlayerHealth>();
        building = GetComponent<BuildingSystem>();
    }

    private void OnAttack(InputValue value)
    {
        if (value.isPressed) TryAttack();
    }

    public bool TryAttack()
    {
        if (health.IsDead || (building != null && building.IsBuildMode) || Time.time < nextAttackTime) return false;

        EnemyHealth nearest = null;
        float nearestDistance = attackRange * attackRange;
        int count = Physics.OverlapSphereNonAlloc(transform.position, attackRange, nearby,
            Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < count; i++)
        {
            EnemyHealth enemy = nearby[i].GetComponentInParent<EnemyHealth>();
            if (enemy == null || enemy.IsDead || !enemy.isActiveAndEnabled) continue;
            float distance = (enemy.transform.position - transform.position).sqrMagnitude;
            if (distance > nearestDistance) continue;
            nearestDistance = distance;
            nearest = enemy;
        }
        if (nearest == null) return false;

        nearest.TakeDamage(attackDamage);
        nextAttackTime = Time.time + attackCooldown;
        return true;
    }
}
