using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public static class M6Validation
{
    private static GameManager game;
    private static WaveManager wave;
    private static HearthController hearth;
    private static PlayerHealth player;
    private static Transform[] points;
    private static readonly Dictionary<int, Vector3> seen = new();
    private static readonly int[] spawned = new int[4];
    private static float started, lastSpawn, nightStarted, oldDay, oldNight;
    private static float oldPlayerMax, oldPlayerHealth, oldHearthMax, oldHearthHealth;
    private static int night;
    private static bool deathChecked, moved, background;
    private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;

    [MenuItem("Tools/Validation/Run M6 (in Play Mode)")]
    public static void Run()
    {
        Require(Application.isPlaying, "Enter a fresh Play session.");
        game = Object.FindFirstObjectByType<GameManager>();
        wave = Object.FindFirstObjectByType<WaveManager>();
        hearth = Object.FindFirstObjectByType<HearthController>();
        player = Object.FindFirstObjectByType<PlayerHealth>();
        Require(game != null && wave != null && hearth != null && player != null && wave.enabled, "Required components ready.");
        Require(game.CurrentState == GameState.Day && game.CurrentDay == 1, "Start on Day 1.");
        points = GameObject.Find("EnemySpawnPoints").GetComponentsInChildren<Transform>();
        Require(points.Length == 5, "Four spawn points.");
        Require(NavMesh.SamplePosition(hearth.transform.position, out var end, 2f, NavMesh.AllAreas), "Hearth is navigable.");
        foreach (var point in points)
        {
            if (point.name == "EnemySpawnPoints") continue;
            Require(NavMesh.SamplePosition(point.position, out var hit, 1f, NavMesh.AllAreas), "Spawn resolves to NavMesh.");
            var path = new NavMeshPath();
            Require(NavMesh.CalculatePath(hit.position, end.position, NavMesh.AllAreas, path)
                && path.status == NavMeshPathStatus.PathComplete, "Complete spawn-to-Hearth path.");
        }
        oldDay = (float)typeof(GameManager).GetField("dayDuration", Private).GetValue(game);
        oldNight = (float)typeof(GameManager).GetField("nightDuration", Private).GetValue(game);
        oldPlayerMax = GetHealth(player, "maxHealth");
        oldPlayerHealth = GetHealth(player, "currentHealth");
        oldHearthMax = GetHealth(hearth, "maxHealth");
        oldHearthHealth = GetHealth(hearth, "currentHealth");
        SetHealth(player, 10000f, 10000f);
        SetHealth(hearth, 10000f, 10000f);
        SetDuration("dayDuration", 2f);
        SetDuration("nightDuration", 9f);
        typeof(GameManager).GetMethod("AdvanceTime", Private).Invoke(game, new object[] { Mathf.Max(0f, game.RemainingTime - 2f) });
        seen.Clear();
        System.Array.Clear(spawned, 0, spawned.Length);
        night = 0;
        deathChecked = moved = false;
        started = Time.time;
        background = Application.runInBackground;
        Application.runInBackground = true;
        EditorApplication.update -= Tick;
        EditorApplication.update += Tick;
    }

    private static void Tick()
    {
        if (!Application.isPlaying) { Finish(); return; }
        try
        {
            Require(Time.time - started < 40f, "Validation timeout.");
            Require(wave.EnemiesAlive >= 0, "Living count never negative.");
            var active = Object.FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
            if (game.CurrentState == GameState.Day)
            {
                Require(active.Length == 0 && wave.EnemiesAlive == 0 && wave.EnemiesSpawned == 0
                    && wave.TotalEnemiesScheduled == 0 && !wave.IsWaveActive, "Day is enemy-free and tracking reset.");
                if (game.CurrentDay == 2) Require(spawned[1] == 3 && deathChecked && moved, "Night 1 spawned three, death tracking and navigation passed.");
                if (game.CurrentDay == 3)
                {
                    Require(spawned[2] == 5, "Night 2 spawned five.");
                    SetDuration("nightDuration", 2.5f);
                }
                if (game.CurrentDay == 4 && game.RemainingTime < 0.3f)
                {
                    Require(spawned[3] == 1, "Short Night 3 cancels six pending spawns.");
                    Finish();
                    Debug.Log("M6 PASS A-G: Day empty; Night 1 3/3; Night 2 5/5; intervals ~1.5s; NavMesh origins/paths and navigation; death once before deactivation; Day cleanup; Night 3 1/7 then pending spawns cancelled.");
                }
                return;
            }
            Require(game.CurrentState == GameState.Night, "Wave validation must not enter a terminal state.");
            if (night != game.CurrentDay)
            {
                night = game.CurrentDay;
                seen.Clear();
                nightStarted = Time.time;
                lastSpawn = 0f;
            }
            Require(night <= 3 && wave.CurrentWave == night && wave.TotalEnemiesScheduled == 3 + (night - 1) * 2, "Scaling follows GameManager day.");
            foreach (var enemy in active)
            {
                int id = enemy.GetInstanceID();
                var agent = enemy.GetComponent<NavMeshAgent>();
                Require(agent.isOnNavMesh, "Every spawned enemy is on NavMesh.");
                if (!seen.TryGetValue(id, out var origin))
                {
                    bool atPoint = false;
                    foreach (var point in points)
                        if (point.name != "EnemySpawnPoints" && Vector3.Distance(enemy.transform.position, point.position + Vector3.up) < 1f) atPoint = true;
                    Require(atPoint, "Enemy originates at configured spawn point.");
                    float interval = lastSpawn == 0f ? Time.time - nightStarted : Time.time - lastSpawn;
                    Require(interval >= 1.4f && interval < 1.8f, "Gradual 1.5-second spawn interval.");
                    Debug.Log("M6 Night " + night + " spawn " + wave.EnemiesSpawned + ": interval " + interval.ToString("F3") + "s, position " + enemy.transform.position);
                    lastSpawn = Time.time;
                    seen.Add(id, enemy.transform.position);
                }
                else if (Vector3.Distance(enemy.transform.position, hearth.transform.position) < Vector3.Distance(origin, hearth.transform.position) - 0.5f)
                {
                    moved = true;
                }
            }
            spawned[night] = wave.EnemiesSpawned;
            Require(wave.EnemiesAlive == active.Length, "Living count matches active enemies.");
            if (night == 1 && wave.EnemiesSpawned == 2 && !deathChecked)
            {
                var victim = active[0];
                int before = wave.EnemiesAlive;
                int events = 0;
                bool beforeDeactivation = false;
                victim.Died += enemy => { events++; beforeDeactivation = enemy.gameObject.activeSelf && enemy.IsDead; };
                victim.TakeDamage(1000f);
                victim.TakeDamage(1000f);
                Require(events == 1 && beforeDeactivation, "Death event exactly once before deactivation.");
                Require(wave.EnemiesAlive == before - 1 && !victim.gameObject.activeSelf, "Living count decremented once.");
                Require(wave.IsWaveActive, "Pending spawns keep wave active.");
                deathChecked = true;
            }
        }
        catch (System.Exception exception)
        {
            Finish();
            Debug.LogException(exception);
        }
    }

    private static void SetDuration(string field, float value) => typeof(GameManager).GetField(field, Private).SetValue(game, value);
    private static float GetHealth(object target, string field) => (float)target.GetType().GetField(field, Private).GetValue(target);
    private static void SetHealth(object target, float max, float current)
    {
        target.GetType().GetField("maxHealth", Private).SetValue(target, max);
        target.GetType().GetField("currentHealth", Private).SetValue(target, current);
    }
    private static void Finish()
    {
        EditorApplication.update -= Tick;
        Application.runInBackground = background;
        if (game != null) { SetDuration("dayDuration", oldDay); SetDuration("nightDuration", oldNight); }
        if (player != null) SetHealth(player, oldPlayerMax, oldPlayerHealth);
        if (hearth != null) SetHealth(hearth, oldHearthMax, oldHearthHealth);
    }
    private static void Require(bool condition, string message)
    {
        if (!condition) throw new System.Exception("M6 FAIL: " + message);
    }
}
