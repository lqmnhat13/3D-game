using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

// Run once in a fresh Play session. Stop Play Mode to discard test damage.
public static class M5Validation
{
    private static EnemyController enemy;
    private static EnemyHealth health;
    private static HearthController hearth;
    private static NavMeshAgent agent;
    private static float started, lastHit, previousHealth, sampledTime;
    private static int hits;
    private static Vector3 origin, sampledPosition, attackPosition;
    private static bool sawPath, sawReturnToChase, background;

    [MenuItem("Tools/Validation/Run M5 (in Play Mode)")]
    public static void Run()
    {
        Require(Application.isPlaying, "Enter a fresh Play session first.");
        enemy = Object.FindFirstObjectByType<EnemyController>();
        hearth = Object.FindFirstObjectByType<HearthController>();
        Require(enemy != null && hearth != null, "Enemy and Hearth exist.");
        health = enemy.GetComponent<EnemyHealth>();
        agent = enemy.GetComponent<NavMeshAgent>();
        Require(hearth.CurrentHealth == 100f && hearth.CurrentFuel == 100f, "Hearth starts at 100 Health/Fuel.");
        Require(agent.isOnNavMesh && NavMesh.CalculateTriangulation().vertices.Length > 0, "Spawn is on baked NavMesh.");
        Require(NavMesh.SamplePosition(hearth.transform.position, out var end, 2f, agent.areaMask), "Hearth area is navigable.");
        var path = new NavMeshPath();
        Require(agent.CalculatePath(end.position, path) && path.status == NavMeshPathStatus.PathComplete, "Complete spawn-to-Hearth path.");
        Require(health.CurrentHealth == 30f && health.MaxHealth == 30f && !health.IsDead, "Initial enemy health.");
        health.TakeDamage(-5f);
        health.TakeDamage(float.NaN);
        health.Heal(-5f);
        health.Heal(float.PositiveInfinity);
        Require(health.CurrentHealth == 30f, "Invalid health amounts ignored.");
        health.TakeDamage(12f);
        Require(health.CurrentHealth == 18f, "Exact damage.");
        health.Heal(5f);
        Require(health.CurrentHealth == 23f, "Exact healing.");
        health.Heal(100f);
        Require(health.CurrentHealth == 30f, "Healing clamps at maximum.");
        origin = sampledPosition = enemy.transform.position;
        Require(Vector3.Distance(origin, hearth.transform.position) > 10f, "Distant initial spawn; no test teleport.");
        previousHealth = hearth.CurrentHealth;
        started = sampledTime = Time.time;
        lastHit = 0f;
        hits = 0;
        sawPath = sawReturnToChase = false;
        background = Application.runInBackground;
        Application.runInBackground = true;
        EditorApplication.update -= Tick;
        EditorApplication.update += Tick;
    }

    private static void Tick()
    {
        if (!Application.isPlaying) { Finish(); return; }
        if (Time.time <= sampledTime) return;
        try
        {
            float elapsed = Time.time - sampledTime;
            Require(Time.time - started < 25f, "Validation completes within 25 seconds.");
            Require(Vector3.Distance(sampledPosition, enemy.transform.position) <= agent.speed * elapsed + 0.2f, "Navigation does not teleport.");
            sampledTime = Time.time;
            sampledPosition = enemy.transform.position;
            float distance = Vector3.ProjectOnPlane(hearth.transform.position - enemy.transform.position, Vector3.up).magnitude;
            if (hits < 4 && distance > 1.8f)
            {
                Require(hearth.CurrentHealth == previousHealth, "No damage outside attack range.");
                Require(enemy.State == EnemyState.Chase, "Outside range stays in Chase.");
                if (hits == 2) sawReturnToChase = true;
            }
            if (agent.enabled && agent.hasPath && agent.pathStatus == NavMeshPathStatus.PathComplete) sawPath = true;
            if (hits == 1)
                Require(Vector3.Distance(attackPosition, enemy.transform.position) < 0.15f, "Enemy stays stopped instead of pushing through Hearth.");
            if (hits < 4 && hearth.CurrentHealth < previousHealth)
            {
                Require(distance <= 1.8f && enemy.State == EnemyState.Attack && agent.isStopped, "In range and stopped in Attack.");
                Require(sawPath && Vector3.Distance(origin, enemy.transform.position) > 5f, "Followed complete path to Hearth.");
                float expectedDamage = hits == 3 ? 5f : 10f;
                Require(Mathf.Approximately(previousHealth - hearth.CurrentHealth, expectedDamage), "Exact attack damage, clamped at zero.");
                if (hits > 0)
                {
                    float interval = Time.time - lastHit;
                    Require(interval >= 0.95f, "Cooldown prevents per-frame damage.");
                    if (hits == 1 || hits == 3) Require(interval < 1.3f, "Approximately one-second cooldown.");
                    Debug.Log("M5 attack interval: " + interval.ToString("F3") + " seconds");
                }
                Debug.Log("M5 Hearth health: " + previousHealth + " -> " + hearth.CurrentHealth);
                previousHealth = hearth.CurrentHealth;
                lastHit = Time.time;
                hits++;
                if (hits == 1) attackPosition = enemy.transform.position;
                if (hits == 2)
                {
                    Require(agent.Warp(origin), "Explicit transition test reposition succeeds.");
                    sampledPosition = enemy.transform.position;
                }
                if (hits == 3)
                {
                    Require(sawReturnToChase, "Attack -> Chase -> Attack after test reposition.");
                    hearth.TakeDamage(hearth.CurrentHealth - 5f);
                    previousHealth = 5f;
                }
                if (hits == 4)
                {
                    Require(hearth.CurrentHealth == 0f, "Overkill attack clamps Hearth at zero.");
                    hearth.Heal(100f);
                    previousHealth = 100f;
                    health.TakeDamage(1000f);
                    health.TakeDamage(1000f);
                    health.Heal(100f);
                    Require(health.CurrentHealth == 0f && health.IsDead && enemy.State == EnemyState.Dead, "Death processed once; no healing resurrection.");
                    Require(!enemy.gameObject.activeSelf && !agent.enabled, "Dead enemy and movement disabled.");
                    attackPosition = enemy.transform.position;
                }
            }
            if (hits == 4 && Time.time - lastHit >= 2f)
            {
                Require(hearth.CurrentHealth == previousHealth, "No post-death damage for two cooldowns.");
                Require(enemy.transform.position == attackPosition && enemy.State == EnemyState.Dead, "Dead enemy never moves or chases.");
                Finish();
                Debug.Log("M5 PASS A-G: complete path, natural navigation, range/stopping, exact/clamped damage, cooldown, Chase -> Attack -> Chase -> Attack, health/healing, death and no post-death activity.");
            }
        }
        catch (System.Exception exception)
        {
            Finish();
            Debug.LogException(exception);
        }
    }

    private static void Finish()
    {
        EditorApplication.update -= Tick;
        Application.runInBackground = background;
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new System.Exception("M5 FAIL: " + message);
    }
}
