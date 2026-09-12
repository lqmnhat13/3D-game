using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(EnemyHealth))]
public sealed class EnemyController : MonoBehaviour
{
    [SerializeField] private HearthController hearth;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private EnemyHealth health;
    [SerializeField, Min(0.1f)] private float attackRange = 1.8f;
    [SerializeField, Min(0f)] private float attackDamage = 10f;
    [SerializeField, Min(0.1f)] private float attackCooldown = 1f;
    [SerializeField] private EnemyState state = EnemyState.Chase;
    private float nextAttackTime;
    private float nextPathTime;

    public EnemyState State => state;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<EnemyHealth>();
    }

    private void Start()
    {
        if (hearth == null) hearth = FindFirstObjectByType<HearthController>();
        agent.stoppingDistance = Mathf.Min(agent.stoppingDistance, attackRange * 0.9f);
    }

    private void Update()
    {
        if (health.IsDead) { Die(); return; }
        if (state == EnemyState.Dead || !agent.enabled || !agent.isOnNavMesh) return;
        if (hearth == null || !hearth.isActiveAndEnabled || hearth.IsDestroyed)
        {
            agent.isStopped = true;
            state = EnemyState.Chase;
            return;
        }

        Vector3 offset = hearth.transform.position - transform.position;
        offset.y = 0f;
        if (offset.sqrMagnitude <= attackRange * attackRange)
        {
            state = EnemyState.Attack;
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            if (offset.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(offset);
            if (Time.time >= nextAttackTime)
            {
                hearth.TakeDamage(attackDamage);
                nextAttackTime = Time.time + attackCooldown;
            }
            return;
        }

        state = EnemyState.Chase;
        agent.isStopped = false;
        if (Time.time < nextPathTime) return;
        nextPathTime = Time.time + 0.25f;
        if (NavMesh.SamplePosition(hearth.transform.position, out var hit, 2f, agent.areaMask))
            agent.SetDestination(hit.position);
    }

    public void Die()
    {
        if (state == EnemyState.Dead) return;
        state = EnemyState.Dead;
        if (agent.enabled && agent.isOnNavMesh) agent.ResetPath();
        agent.enabled = false;
    }
}
