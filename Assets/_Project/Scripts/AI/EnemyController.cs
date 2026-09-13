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
    private Fence blockingFence;
    private PlayerHealth player;
    private NavMeshPath hearthPath;
    private NavMeshPath approachPath;
    private readonly Vector3[] corners = new Vector3[64];
    private readonly Collider[] nearby = new Collider[32];

    public EnemyState State => state;

    public void SetHearth(HearthController target)
    {
        hearth = target;
    }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<EnemyHealth>();
        hearthPath = new NavMeshPath();
        approachPath = new NavMeshPath();
    }

    private void Start()
    {
        if (hearth == null) hearth = FindFirstObjectByType<HearthController>();
        player = FindFirstObjectByType<PlayerHealth>();
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

        bool targetsPlayer = player != null && player.isActiveAndEnabled && !player.IsDead
            && HorizontalDistanceSquared(player.transform.position) <= attackRange * attackRange;
        if (!targetsPlayer && Time.time >= nextPathTime)
        {
            nextPathTime = Time.time + 0.25f;
            UpdateRoute();
        }

        bool targetsFence = !targetsPlayer && blockingFence != null && blockingFence.isActiveAndEnabled && !blockingFence.IsDestroyed;
        Vector3 target = targetsPlayer ? player.transform.position
            : targetsFence ? blockingFence.ClosestPoint(transform.position) : hearth.transform.position;
        Vector3 offset = target - transform.position;
        offset.y = 0f;
        bool fenceInWay = !targetsFence && Physics.Linecast(transform.position, hearth.transform.position,
            out var obstruction, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)
            && obstruction.collider.TryGetComponent<Fence>(out _);
        if (offset.sqrMagnitude <= attackRange * attackRange && !fenceInWay)
        {
            state = EnemyState.Attack;
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            if (offset.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(offset);
            if (Time.time >= nextAttackTime)
            {
                if (targetsPlayer) player.TakeDamage(attackDamage);
                else if (targetsFence) blockingFence.TakeDamage(attackDamage);
                else hearth.TakeDamage(attackDamage);
                nextAttackTime = Time.time + attackCooldown;
            }
            return;
        }

        state = EnemyState.Chase;
        agent.isStopped = false;
    }

    private float HorizontalDistanceSquared(Vector3 target)
    {
        Vector3 offset = target - transform.position;
        offset.y = 0f;
        return offset.sqrMagnitude;
    }

    private void UpdateRoute()
    {
        blockingFence = null;
        if (!NavMesh.SamplePosition(hearth.transform.position, out var hit, 2f, agent.areaMask)) return;
        agent.CalculatePath(hit.position, hearthPath);
        if (hearthPath.status == NavMeshPathStatus.PathComplete)
        {
            agent.SetPath(hearthPath);
            return;
        }

        int cornerCount = hearthPath.GetCornersNonAlloc(corners);
        Vector3 end = cornerCount > 0 ? corners[cornerCount - 1] : transform.position;
        int count = Physics.OverlapSphereNonAlloc(end, 3f, nearby, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
        float nearest = float.PositiveInfinity;
        Vector3 destination = end;
        // ponytail: local partial-path endpoint search; larger mazes would need obstacle planning.
        for (int i = 0; i < count; i++)
        {
            if (!nearby[i].TryGetComponent<Fence>(out var fence) || fence.IsDestroyed) continue;
            Vector3 point = fence.ClosestPoint(end + Vector3.up * agent.baseOffset);
            float distance = (point - end).sqrMagnitude;
            if (distance >= nearest || !NavMesh.SamplePosition(point, out var approach, 2f, agent.areaMask)
                || !agent.CalculatePath(approach.position, approachPath)
                || approachPath.status != NavMeshPathStatus.PathComplete) continue;
            nearest = distance;
            blockingFence = fence;
            destination = approach.position;
        }
        agent.SetDestination(destination);
    }

    public void Die()
    {
        if (state == EnemyState.Dead) return;
        state = EnemyState.Dead;
        if (agent.enabled && agent.isOnNavMesh) agent.ResetPath();
        agent.enabled = false;
    }
}
