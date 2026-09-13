using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class M9Validation
{
    private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
    private static IEnumerator checks;
    private static bool background;
    private static float timeScale;

    [MenuItem("Tools/Validation/M9 Player Day GameOver (in Play Mode)")]
    public static void RunPlayerDayGameOver() => Run(PlayerDayGameOver());

    [MenuItem("Tools/Validation/M9 Player GameOver (in Play Mode)")]
    public static void RunPlayerGameOver() => Run(PlayerGameOver());

    [MenuItem("Tools/Validation/M9 Hearth GameOver (in Play Mode)")]
    public static void RunHearthGameOver() => Run(HearthGameOver());

    [MenuItem("Tools/Validation/M9 Fuel Zero (in Play Mode)")]
    public static void RunFuelZero() => Run(FuelZero());

    [MenuItem("Tools/Validation/M9 Victory (in Play Mode)")]
    public static void RunVictory() => Run(Victory());

    [MenuItem("Tools/Validation/M9 Final Boundary Priority (in Play Mode)")]
    public static void RunFinalBoundaryPriority() => Run(FinalBoundaryPriority());

    [MenuItem("Tools/Validation/M9 Final Hearth Boundary Priority (in Play Mode)")]
    public static void RunFinalHearthBoundaryPriority() => Run(FinalHearthBoundaryPriority());

    private static void Run(IEnumerator routine)
    {
        Require(Application.isPlaying && checks == null, "Start in a fresh Play session.");
        background = Application.runInBackground;
        timeScale = Time.timeScale;
        Application.runInBackground = true;
        checks = routine;
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

    private static IEnumerator PlayerDayGameOver()
    {
        Get(out var game, out var player, out var hearth, out var wave);
        int deaths = 0, terminalChanges = 0;
        player.Died += _ => deaths++;
        game.StateChanged += state => { if (state == GameState.GameOver || state == GameState.Victory) terminalChanges++; };

        player.TakeDamage(1000f);
        Require(player.IsDead && player.CurrentHealth == 0f && deaths == 1, "Player death during Day clamps and fires once.");
        Require(game.CurrentState == GameState.GameOver && terminalChanges == 1, "Player death during Day enters GameOver once.");
        RequireTerminalSystemsStopped(game, wave);
        yield return null;
        Debug.Log("M9 PASS A1: Player death during Day -> one GameOver; gameplay input and systems stopped.");
    }

    private static IEnumerator PlayerGameOver()
    {
        Get(out var game, out var player, out var hearth, out var wave);
        int deaths = 0, terminalChanges = 0;
        player.Died += _ => deaths++;
        game.StateChanged += state => { if (state == GameState.GameOver || state == GameState.Victory) terminalChanges++; };

        Advance(game);
        Require(game.CurrentState == GameState.Night, "Night starts before Player death.");
        float until = Time.time + 1.7f;
        while (Time.time < until) yield return null;
        Require(wave.EnemiesSpawned == 1 && wave.EnemiesAlive == 1, "Wave has one active enemy before terminal state.");

        player.TakeDamage(1000f);
        Require(player.IsDead && player.CurrentHealth == 0f && deaths == 1, "Player death clamps and fires once.");
        Require(game.CurrentState == GameState.GameOver && terminalChanges == 1, "Player death enters GameOver once.");
        RequireTerminalSystemsStopped(game, wave);
        yield return null; yield return null;
        Require(Object.FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None).Length == 0, "Active wave enemies cleaned up.");

        int day = game.CurrentDay;
        float remaining = game.RemainingTime;
        Advance(game, 999f);
        Invoke(game, "Victory");
        player.TakeDamage(1000f);
        Require(game.CurrentState == GameState.GameOver && game.CurrentDay == day
            && game.RemainingTime == remaining && terminalChanges == 1 && deaths == 1,
            "GameOver cannot transition, advance, become Victory or fire twice.");
        until = Time.time + 1.7f; while (Time.time < until) yield return null;
        Require(wave.EnemiesSpawned == 0 && Object.FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None).Length == 0,
            "No spawning after GameOver.");
        Debug.Log("M9 PASS A2/E: Player death during Night -> one GameOver; gameplay/wave stopped, enemy cleaned, timer frozen, terminal immutable.");
    }

    private static IEnumerator HearthGameOver()
    {
        Get(out var game, out var player, out var hearth, out var wave);
        int destructions = 0, terminalChanges = 0;
        hearth.Destroyed += _ => destructions++;
        game.StateChanged += state => { if (state == GameState.GameOver) terminalChanges++; };
        hearth.TakeDamage(1000f);
        hearth.TakeDamage(1000f);
        Require(hearth.IsDestroyed && hearth.CurrentHealth == 0f && destructions == 1,
            "Hearth destruction clamps and fires once.");
        Require(!player.IsDead && player.CurrentHealth == 100f, "Player need not be dead.");
        Require(game.CurrentState == GameState.GameOver && terminalChanges == 1, "Hearth destruction enters GameOver once.");
        RequireTerminalSystemsStopped(game, wave);
        yield return null;
        Debug.Log("M9 PASS B: Hearth 100 -> 0 fired one destruction event and one GameOver while Player stayed 100.");
    }

    private static IEnumerator FuelZero()
    {
        Get(out var game, out var player, out var hearth, out var wave);
        hearth.ConsumeFuel(1000f);
        yield return null;
        Require(hearth.CurrentFuel == 0f && hearth.CurrentHealth == 100f && !hearth.IsDestroyed,
            "Fuel zero leaves Hearth Health intact.");
        Require(Mathf.Approximately(hearth.CurrentLightRadius, 4f), "Fuel zero uses minimum light radius.");
        Require(game.CurrentState == GameState.Day && game.enabled && wave.enabled,
            "Fuel zero is not terminal and gameplay continues.");
        Require(Mathf.Approximately(Time.timeScale, timeScale), "Fuel zero does not change Time.timeScale.");
        Debug.Log("M9 PASS C: Fuel 100 -> 0; Health 100; light radius 4; state remained Day.");
    }

    private static IEnumerator Victory()
    {
        Get(out var game, out var player, out var hearth, out var wave);
        int victories = 0;
        game.StateChanged += state => { if (state == GameState.Victory) victories++; };
        for (int night = 1; night <= 4; night++)
        {
            Advance(game);
            Require(game.CurrentState == GameState.Night && game.CurrentDay == night && victories == 0,
                "Night " + night + " starts without Victory.");
            Advance(game);
            Require(game.CurrentState == GameState.Day && game.CurrentDay == night + 1 && victories == 0,
                "Night " + night + " completes without early Victory.");
        }
        Advance(game);
        Require(game.CurrentState == GameState.Night && game.CurrentDay == 5 && victories == 0,
            "Night 5 begins without Victory.");
        Advance(game);
        Require(game.CurrentState == GameState.Victory && game.CurrentDay == 5
            && game.RemainingTime == 0f && victories == 1 && !player.IsDead && !hearth.IsDestroyed,
            "Completing living Night 5 enters Victory once without Day 6.");
        RequireTerminalSystemsStopped(game, wave);
        player.TakeDamage(1000f);
        Advance(game, 999f);
        Require(game.CurrentState == GameState.Victory && game.CurrentDay == 5 && victories == 1,
            "Victory cannot later become GameOver or resume time.");
        float until = Time.time + 1.7f; while (Time.time < until) yield return null;
        Require(Object.FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None).Length == 0,
            "Victory prevents future enemies.");
        Debug.Log("M9 PASS D/E: Nights 1-4 completed normally; Night 5 start was not Victory; completion -> one immutable Victory, no Day 6/spawns.");
    }

    private static IEnumerator FinalBoundaryPriority()
    {
        Get(out var game, out var player, out var hearth, out var wave);
        int gameOvers = 0, victories = 0;
        game.StateChanged += state => { if (state == GameState.GameOver) gameOvers++; if (state == GameState.Victory) victories++; };
        for (int night = 1; night <= 4; night++) { Advance(game); Advance(game); }
        Advance(game);
        Require(game.CurrentState == GameState.Night && game.CurrentDay == 5, "Prepared final Night boundary.");
        typeof(PlayerHealth).GetField("currentHealth", Private).SetValue(player, 0f);
        Advance(game);
        Require(game.CurrentState == GameState.GameOver && gameOvers == 1 && victories == 0,
            "Dead Player at final completion produces GameOver, never Victory.");
        RequireTerminalSystemsStopped(game, wave);
        yield return null;
        Debug.Log("M9 PASS F: Player dead at exact Night 5 completion boundary -> GameOver; Victory count stayed zero.");
    }

    private static IEnumerator FinalHearthBoundaryPriority()
    {
        Get(out var game, out var player, out var hearth, out var wave);
        int gameOvers = 0, victories = 0;
        game.StateChanged += state => { if (state == GameState.GameOver) gameOvers++; if (state == GameState.Victory) victories++; };
        for (int night = 1; night <= 4; night++) { Advance(game); Advance(game); }
        Advance(game);
        Require(game.CurrentState == GameState.Night && game.CurrentDay == 5, "Prepared final Night Hearth boundary.");
        typeof(HearthController).GetField("currentHealth", Private).SetValue(hearth, 0f);
        Advance(game);
        Require(game.CurrentState == GameState.GameOver && gameOvers == 1 && victories == 0,
            "Destroyed Hearth at final completion produces GameOver, never Victory.");
        RequireTerminalSystemsStopped(game, wave);
        yield return null;
        Debug.Log("M9 PASS G: Hearth destroyed at exact Night 5 completion boundary -> GameOver; Victory count stayed zero.");
    }

    private static void Get(out GameManager game, out PlayerHealth player,
        out HearthController hearth, out WaveManager wave)
    {
        game = Object.FindFirstObjectByType<GameManager>();
        player = Object.FindFirstObjectByType<PlayerHealth>();
        hearth = Object.FindFirstObjectByType<HearthController>();
        wave = Object.FindFirstObjectByType<WaveManager>();
        Require(game != null && game.enabled && player != null && !player.IsDead
            && hearth != null && !hearth.IsDestroyed && wave != null && wave.enabled,
            "Fresh Player, Hearth, GameManager and WaveManager.");
        Require(game.CurrentState == GameState.Day && game.CurrentDay == 1,
            "Fresh session begins Day 1.");
    }

    private static void RequireTerminalSystemsStopped(GameManager game, WaveManager wave)
    {
        var player = GameObject.Find("Player");
        Require(game.RemainingTime == 0f && !wave.enabled && !wave.IsWaveActive
            && !player.GetComponent<PlayerMovement>().enabled
            && !player.GetComponent<PlayerCombat>().enabled
            && !player.GetComponent<PlayerInteractor>().enabled
            && !player.GetComponent<BuildingSystem>().enabled
            && !player.GetComponent<UnityEngine.InputSystem.PlayerInput>().inputIsActive
            && Mathf.Approximately(Time.timeScale, timeScale),
            "Terminal state stops timer, wave, movement, combat, interaction, building and PlayerInput without changing Time.timeScale.");
    }

    private static void Advance(GameManager game, float? elapsed = null)
    {
        typeof(GameManager).GetMethod("AdvanceTime", Private).Invoke(game,
            new object[] { elapsed ?? game.RemainingTime });
    }

    private static void Invoke(GameManager game, string method)
    {
        typeof(GameManager).GetMethod(method, Private).Invoke(game, null);
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new System.Exception("M9 FAIL: " + message);
    }
}
