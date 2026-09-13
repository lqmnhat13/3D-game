using UnityEngine;
using UnityEngine.AI;

public sealed class EnemySpawner : MonoBehaviour
{
    [SerializeField] private EnemyHealth enemyPrefab;
    [SerializeField] private HearthController hearth;
    [SerializeField] private Transform[] spawnPoints;
    private NavMeshAgent prefabAgent;
    private NavMeshPath path;

    private void Awake()
    {
        if (enemyPrefab != null) prefabAgent = enemyPrefab.GetComponent<NavMeshAgent>();
        path = new NavMeshPath();
    }

    public EnemyHealth Spawn()
    {
        if (prefabAgent == null || hearth == null || spawnPoints == null || spawnPoints.Length == 0)
            return null;
        path ??= new NavMeshPath();

        var filter = new NavMeshQueryFilter { agentTypeID = prefabAgent.agentTypeID, areaMask = prefabAgent.areaMask };
        if (!NavMesh.SamplePosition(hearth.transform.position, out var destination, 2f, filter)) return null;
        int first = Random.Range(0, spawnPoints.Length);
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            Transform point = spawnPoints[(first + i) % spawnPoints.Length];
            if (point == null || !NavMesh.SamplePosition(point.position, out var hit, 1f, filter)) continue;
            if (!NavMesh.CalculatePath(hit.position, destination.position, filter, path)
                || path.status == NavMeshPathStatus.PathInvalid) continue;
            // A Fence can make the route partial; the spawned enemy must be allowed to break it.

            EnemyHealth enemy = Instantiate(enemyPrefab, hit.position + Vector3.up * prefabAgent.baseOffset, point.rotation);
            enemy.GetComponent<EnemyController>().SetHearth(hearth);
            return enemy;
        }
        return null;
    }
}
