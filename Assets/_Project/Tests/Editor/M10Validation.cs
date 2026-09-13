using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class M10Validation
{
    private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
    private static IEnumerator checks;
    private static bool background;

    [MenuItem("Tools/Validation/M10 HUD (in Play Mode)")]
    public static void RunHud() => Run(Hud());

    [MenuItem("Tools/Validation/M10 GameOver UI (in Play Mode)")]
    public static void RunGameOver() => Run(GameOver());

    [MenuItem("Tools/Validation/M10 Victory UI (in Play Mode)")]
    public static void RunVictory() => Run(Victory());

    private static void Run(IEnumerator routine)
    {
        Require(Application.isPlaying && checks == null, "Start in a fresh Play session.");
        background = Application.runInBackground;
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

    private static IEnumerator Hud()
    {
        Get(out var game, out var player, out var inventory, out var hearth, out var wave,
            out var text, out var gameOver, out var victory);
        float until = Time.realtimeSinceStartup + 1f;
        while (!text.text.Contains("Player HP: 100 / 100") && Time.realtimeSinceStartup < until)
            yield return null;
        Require(text.text.Contains("Player HP: 100 / 100") && text.text.Contains("Hearth HP: 100 / 100")
            && text.text.Contains("Fuel: 100 / 100") && text.text.Contains("Wood: 0")
            && text.text.Contains("Stone: 0") && text.text.Contains("Day 1")
            && text.text.Contains("Time:") && text.text.Contains("Enemies: 0"), "Initial HUD values are valid.");
        Require(!gameOver.activeSelf && !victory.activeSelf, "End panels begin hidden during Day.");

        int wood = inventory.Wood, stone = inventory.Stone;
        Require(GameObject.Find("Tree_Test").GetComponent<ResourceNode>().Gather(inventory), "Tree gathers Wood.");
        Require(GameObject.Find("Rock_Test").GetComponent<ResourceNode>().Gather(inventory), "Rock gathers Stone.");
        until = Time.realtimeSinceStartup + 1f;
        while ((!text.text.Contains("Wood: " + inventory.Wood)
            || !text.text.Contains("Stone: " + inventory.Stone)) && Time.realtimeSinceStartup < until)
            yield return null;
        Require(inventory.Wood > wood && inventory.Stone > stone
            && text.text.Contains("Wood: " + inventory.Wood) && text.text.Contains("Stone: " + inventory.Stone),
            "Gathered Wood and Stone update the HUD.");

        player.TakeDamage(25f);
        hearth.TakeDamage(20f);
        hearth.ConsumeFuel(30f);
        until = Time.realtimeSinceStartup + 1f;
        while ((!text.text.Contains("Player HP: 75 / 100")
            || !text.text.Contains("Hearth HP: 80 / 100")
            || !text.text.Contains("Fuel: 70 / 100")) && Time.realtimeSinceStartup < until)
            yield return null;
        Require(text.text.Contains("Player HP: 75 / 100"), "Player damage updates the HUD.");
        Require(text.text.Contains("Hearth HP: 80 / 100") && text.text.Contains("Fuel: 70 / 100"),
            "Hearth damage and fuel consumption update the HUD.");

        int before = Mathf.CeilToInt(game.RemainingTime);
        Advance(game, 1.1f);
        until = Time.realtimeSinceStartup + 1f;
        while (!text.text.Contains("Time: " + Mathf.CeilToInt(game.RemainingTime))
            && Time.realtimeSinceStartup < until) yield return null;
        Require(text.text.Contains("Time: " + Mathf.CeilToInt(game.RemainingTime))
            && Mathf.CeilToInt(game.RemainingTime) < before, "Timer counts down with upward whole-second rounding.");

        Advance(game);
        until = Time.realtimeSinceStartup + 1f;
        while (!text.text.Contains("Night 1") && Time.realtimeSinceStartup < until) yield return null;
        Require(game.CurrentState == GameState.Night && text.text.Contains("Night 1"), "HUD changes to Night 1.");
        until = Time.time + 1.7f;
        while (Time.time < until) yield return null;
        until = Time.realtimeSinceStartup + 1f;
        while (!text.text.Contains("Enemies: 1") && Time.realtimeSinceStartup < until) yield return null;
        Require(wave.EnemiesAlive == 1 && text.text.Contains("Enemies: 1"), "Night wave count appears in the HUD.");

        Advance(game);
        until = Time.realtimeSinceStartup + 1f;
        while ((!text.text.Contains("Day 2") || !text.text.Contains("Enemies: 0"))
            && Time.realtimeSinceStartup < until) yield return null;
        Require(game.CurrentState == GameState.Day && game.CurrentDay == 2 && text.text.Contains("Day 2"),
            "Night completion advances the HUD to Day 2.");
        Require(wave.EnemiesAlive == 0 && text.text.Contains("Enemies: 0")
            && Object.FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None).Length == 0,
            "Wave cleanup returns the HUD enemy count to zero.");
        Require(!gameOver.activeSelf && !victory.activeSelf, "End panels stay hidden during normal Day/Night play.");
        Debug.Log("M10 PASS A-F: initial values, gathered resources, Player/Hearth/Fuel changes, countdown, Day/Night/day number and wave cleanup all update the HUD.");
    }

    private static IEnumerator GameOver()
    {
        Get(out var game, out var player, out _, out _, out _, out var text, out var gameOver, out var victory);
        player.TakeDamage(1000f);
        float until = Time.realtimeSinceStartup + 1f;
        while ((!gameOver.activeSelf || !text.text.Contains("Game Over 1"))
            && Time.realtimeSinceStartup < until) yield return null;
        Require(game.CurrentState == GameState.GameOver && gameOver.activeSelf && !victory.activeSelf
            && text.text.Contains("Game Over 1"), "Player death shows only GameOverPanel.");
        Debug.Log("M10 PASS G: Player death shows GameOverPanel and keeps VictoryPanel hidden.");
    }

    private static IEnumerator Victory()
    {
        Get(out var game, out _, out _, out _, out _, out var text, out var gameOver, out var victory);
        for (int night = 1; night <= 5; night++)
        {
            Advance(game);
            Require(game.CurrentState == GameState.Night, "Night " + night + " begins.");
            Advance(game);
        }
        float until = Time.realtimeSinceStartup + 1f;
        while ((!victory.activeSelf || !text.text.Contains("Victory 5"))
            && Time.realtimeSinceStartup < until) yield return null;
        Require(game.CurrentState == GameState.Victory && victory.activeSelf && !gameOver.activeSelf
            && text.text.Contains("Victory 5"), "Completing Night 5 shows only VictoryPanel.");
        Debug.Log("M10 PASS H: controlled Night 5 completion shows VictoryPanel and keeps GameOverPanel hidden.");
    }

    private static void Get(out GameManager game, out PlayerHealth player, out PlayerInventory inventory,
        out HearthController hearth, out WaveManager wave, out Text text, out GameObject gameOver, out GameObject victory)
    {
        game = Object.FindFirstObjectByType<GameManager>();
        player = Object.FindFirstObjectByType<PlayerHealth>();
        inventory = Object.FindFirstObjectByType<PlayerInventory>();
        hearth = Object.FindFirstObjectByType<HearthController>();
        wave = Object.FindFirstObjectByType<WaveManager>();
        var canvas = GameObject.Find("Canvas");
        text = GameObject.Find("Canvas/HUD/HUDText")?.GetComponent<Text>();
        gameOver = FindChild(canvas, "GameOverPanel");
        victory = FindChild(canvas, "VictoryPanel");
        Require(game != null && game.enabled && game.CurrentState == GameState.Day && game.CurrentDay == 1
            && player != null && player.CurrentHealth == 100f && inventory != null
            && hearth != null && hearth.CurrentHealth == 100f && hearth.CurrentFuel == 100f
            && wave != null && wave.enabled && text != null && gameOver != null && victory != null,
            "Fresh scene and complete M10 references.");
    }

    private static GameObject FindChild(GameObject parent, string name)
    {
        if (parent == null) return null;
        Transform child = parent.transform.Find(name);
        return child != null ? child.gameObject : null;
    }

    private static void Advance(GameManager game, float? elapsed = null)
    {
        typeof(GameManager).GetMethod("AdvanceTime", Private).Invoke(game,
            new object[] { elapsed ?? game.RemainingTime });
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new System.Exception("M10 FAIL: " + message);
    }
}
