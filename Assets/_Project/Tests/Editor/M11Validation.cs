using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public static class M11Validation
{
    private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
    private static IEnumerator checks;
    private static bool background;

    [MenuItem("Tools/Validation/M11A Hearth Refueling (in Play Mode)")]
    public static void RunRefueling()
    {
        Require(Application.isPlaying, "Enter a fresh Play session.");
        var player = Object.FindFirstObjectByType<PlayerInteractor>();
        var inventory = Object.FindFirstObjectByType<PlayerInventory>();
        var hearth = Object.FindFirstObjectByType<HearthController>();
        Require(player != null && inventory != null && hearth != null, "Player, Inventory and Hearth are ready.");

        hearth.ConsumeFuel(30f);
        Require(inventory.AddResource(ResourceType.Wood, 3), "Test Wood added.");
        player.transform.position = hearth.transform.position;
        Physics.SyncTransforms();

        Require(player.Interact() && inventory.Wood == 2 && hearth.CurrentFuel == 90f,
            "One Wood restores exactly 20 Fuel.");
        Require(player.Interact() && inventory.Wood == 1 && hearth.CurrentFuel == 100f,
            "Refueling clamps at maximum.");
        Require(!player.Interact() && inventory.Wood == 1 && hearth.CurrentFuel == 100f,
            "Full Hearth consumes no Wood.");

        hearth.ConsumeFuel(20f);
        player.transform.position = hearth.transform.position + Vector3.right * 3f;
        Physics.SyncTransforms();
        Require(!player.Interact() && inventory.Wood == 1 && hearth.CurrentFuel == 80f,
            "Refueling respects interaction range.");
        Debug.Log("M11A PASS: E interaction spends one Wood for 20 Fuel, clamps at full, preserves Wood when full and respects range.");
    }

    [MenuItem("Tools/Validation/M11B Daily Resources (in Play Mode)")]
    public static void RunDailyResources()
    {
        Require(Application.isPlaying, "Enter a fresh Play session.");
        var game = Object.FindFirstObjectByType<GameManager>();
        var inventory = Object.FindFirstObjectByType<PlayerInventory>();
        var tree = GameObject.Find("Tree_Test").GetComponent<ResourceNode>();
        var rock = GameObject.Find("Rock_Test").GetComponent<ResourceNode>();
        Require(game != null && inventory != null && tree != null && rock != null, "Game, Inventory and resources are ready.");

        Require(tree.Gather(inventory) && rock.Gather(inventory)
            && inventory.Wood == 10 && inventory.Stone == 3, "Day 1 provides 10 Wood and 3 Stone.");
        Require(!tree.Gather(inventory) && !rock.Gather(inventory), "Nodes cannot reward twice in one Day.");

        Advance(game);
        Advance(game);
        Require(game.CurrentState == GameState.Day && game.CurrentDay == 2
            && tree.CanGather && rock.CanGather, "Entering Day 2 reactivates depleted nodes.");
        Require(tree.Gather(inventory) && rock.Gather(inventory)
            && inventory.Wood == 20 && inventory.Stone == 6, "Day 2 provides the same resources again.");
        Debug.Log("M11B PASS: each Day resets one 10-Wood tree and one 3-Stone rock without duplicate same-Day rewards.");
    }

    private static void Advance(GameManager game)
    {
        typeof(GameManager).GetMethod("AdvanceTime", Private).Invoke(game, new object[] { game.RemainingTime });
    }

    [MenuItem("Tools/Validation/M11C Hearth Light Slow (in Play Mode)")]
    public static void RunHearthLightSlow()
    {
        Require(Application.isPlaying, "Enter a fresh Play session.");
        var hearth = Object.FindFirstObjectByType<HearthController>();
        var spawner = Object.FindFirstObjectByType<EnemySpawner>();
        var enemy = spawner != null ? spawner.Spawn() : null;
        Require(hearth != null && enemy != null, "Hearth and spawned enemy are ready.");
        var controller = enemy.GetComponent<EnemyController>();
        var agent = enemy.GetComponent<NavMeshAgent>();

        InvokeUpdate(controller);
        Require(Mathf.Approximately(agent.speed, 6f), "Enemy keeps normal speed outside Hearth light.");
        Require(NavMesh.SamplePosition(hearth.transform.position + Vector3.right * 6f,
            out var inside, 2f, agent.areaMask) && agent.Warp(inside.position), "Inside-light test position is navigable.");
        InvokeUpdate(controller);
        Require(Mathf.Approximately(agent.speed, 4.2f), "Enemy speed is multiplied by 0.7 inside Hearth light.");

        hearth.ConsumeFuel(1000f);
        InvokeUpdate(controller);
        Require(Mathf.Approximately(agent.speed, 6f), "Enemy returns to normal speed when Fuel contracts the light.");
        enemy.TakeDamage(1000f);
        Debug.Log("M11C PASS: Shadow Crawler speed is 6 outside Hearth light, 4.2 inside, and restores when the light contracts.");
    }

    private static void InvokeUpdate(EnemyController controller)
    {
        typeof(EnemyController).GetMethod("Update", Private).Invoke(controller, null);
    }

    [MenuItem("Tools/Validation/M11D Early Night Completion (in Play Mode)")]
    public static void RunEarlyNightCompletion()
    {
        Require(Application.isPlaying && checks == null, "Enter a fresh Play session.");
        background = Application.runInBackground;
        Application.runInBackground = true;
        checks = EarlyNightCompletion();
        EditorApplication.update += Tick;
    }

    private static void Tick()
    {
        try
        {
            if (Application.isPlaying && checks.MoveNext()) return;
            Finish();
        }
        catch (System.Exception error) { Finish(); Debug.LogException(error); }
    }

    private static void Finish()
    {
        EditorApplication.update -= Tick;
        (checks as System.IDisposable)?.Dispose();
        checks = null;
        Application.runInBackground = background;
    }

    private static IEnumerator EarlyNightCompletion()
    {
        var game = Object.FindFirstObjectByType<GameManager>();
        var wave = Object.FindFirstObjectByType<WaveManager>();
        var hearth = Object.FindFirstObjectByType<HearthController>();
        Require(game != null && wave != null && hearth != null && game.CurrentState == GameState.Day,
            "Fresh GameManager, WaveManager and Hearth are ready.");
        int completions = 0;
        wave.WaveCompleted += () => completions++;

        Advance(game);
        float until = Time.time + 8f;
        while (wave.EnemiesSpawned < wave.TotalEnemiesScheduled && Time.time < until) yield return null;
        Require(game.CurrentState == GameState.Night && wave.EnemiesSpawned == 3, "Night 1 schedules and spawns all three enemies.");
        float fuelBeforeKill = hearth.CurrentFuel;
        foreach (var enemy in Object.FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None)) enemy.TakeDamage(1000f);
        yield return null;
        Require(game.CurrentState == GameState.Day && game.CurrentDay == 2 && completions == 1,
            "Clearing Night 1 enters Day 2 exactly once.");
        Require(Mathf.Abs(hearth.CurrentFuel - fuelBeforeKill) < 0.1f, "Skipped Night time consumes no extra Fuel.");
        Require(wave.EnemiesAlive == 0 && wave.EnemiesSpawned == 0 && wave.TotalEnemiesScheduled == 0,
            "Day cleanup resets wave tracking.");

        for (int night = 2; night <= 4; night++)
        {
            Advance(game);
            Require(game.CurrentState == GameState.Night && game.CurrentDay == night, "Night " + night + " begins.");
            Advance(game);
            Require(game.CurrentState == GameState.Day && game.CurrentDay == night + 1, "Night " + night + " timer still reaches the next Day.");
        }

        Advance(game);
        Require(game.CurrentState == GameState.Night && game.CurrentDay == 5, "Night 5 begins without early Victory.");
        until = Time.time + 20f;
        while (game.CurrentState == GameState.Night && Time.time < until)
        {
            foreach (var enemy in Object.FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None)) enemy.TakeDamage(1000f);
            yield return null;
        }
        Require(game.CurrentState == GameState.Victory && game.CurrentDay == 5
            && game.RemainingTime == 0f && completions == 2, "Clearing Night 5 produces one Victory and no Day 6.");
        Debug.Log("M11D PASS: complete waves end Nights early, skip remaining Fuel drain, preserve timed cleanup and produce Victory after Night 5 without Day 6.");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new System.Exception("M11 FAIL: " + message);
    }
}
