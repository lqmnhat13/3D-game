using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public static class M8Validation
{
    private static IEnumerator checks;
    private static Keyboard keyboard;
    private static Mouse mouse;
    private static InputDevice[] originalDevices;
    private static bool background;

    [MenuItem("Tools/Validation/Run M8 (in Play Mode)")]
    public static void Run()
    {
        Require(Application.isPlaying && checks == null, "Start in a fresh Play session.");
        background = Application.runInBackground;
        Application.runInBackground = true;
        checks = Check();
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
        if (keyboard != null) InputSystem.RemoveDevice(keyboard);
        if (mouse != null) InputSystem.RemoveDevice(mouse);
        keyboard = null;
        mouse = null;
        if (originalDevices != null)
            foreach (var device in originalDevices) if (device.added) InputSystem.EnableDevice(device);
        originalDevices = null;
        Application.runInBackground = background;
    }

    private static IEnumerator Check()
    {
        var player = GameObject.Find("Player");
        var health = player.GetComponent<PlayerHealth>();
        var combat = player.GetComponent<PlayerCombat>();
        var build = player.GetComponent<BuildingSystem>();
        var inventory = player.GetComponent<PlayerInventory>();
        var input = player.GetComponent<PlayerInput>();
        var game = Object.FindFirstObjectByType<GameManager>();
        var hearth = Object.FindFirstObjectByType<HearthController>();
        var crawler = AssetDatabase.LoadAssetAtPath<EnemyHealth>("Assets/_Project/Prefabs/Enemies/ShadowCrawler.prefab");
        Require(health != null && combat != null && build != null && input != null && health.CurrentHealth == 100f,
            "Player M8 components and initial Health 100.");
        health.TakeDamage(-5f); health.TakeDamage(float.NaN); health.TakeDamage(float.PositiveInfinity);
        health.Heal(-5f); health.Heal(float.NaN); health.Heal(float.PositiveInfinity);
        Require(health.CurrentHealth == 100f, "Invalid health amounts are ignored.");
        health.TakeDamage(20f); health.Heal(5f);
        Require(health.CurrentHealth == 85f, "Exact Player damage and healing.");
        health.Heal(100f);
        Require(health.CurrentHealth == 100f, "Player healing clamps at maximum.");
        Debug.Log("M8 PASS health: invalid amounts ignored; 100 -> 80 -> 85; excess healing -> 100.");
        game.enabled = false;

        originalDevices = System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Where(InputSystem.devices,
            d => d.enabled && (d is Mouse || d is Keyboard)));
        foreach (var device in originalDevices) InputSystem.DisableDevice(device);
        keyboard = InputSystem.AddDevice<Keyboard>();
        mouse = InputSystem.AddDevice<Mouse>();
        input.SwitchCurrentControlScheme("Keyboard&Mouse", keyboard, mouse);

        EnemyHealth enemy = Spawn(crawler, player.transform.position + Vector3.forward * 1.5f, false);
        Click(); yield return null; Release(); yield return null;
        Require(enemy.CurrentHealth == 15f, "Attack input deals exactly 15 damage in range.");
        Debug.Log("M8 PASS A: left click damaged ShadowCrawler 30 -> 15.");

        for (int i = 0; i < 5; i++) { Click(); yield return null; Release(); yield return null; }
        Require(enemy.CurrentHealth == 15f, "Attack spam is rejected during cooldown.");
        Debug.Log("M8 PASS C: five attacks during cooldown caused no extra damage.");
        float until = Time.time + .7f;
        while (Time.time < until) yield return null;
        Click(); yield return null; Release(); yield return null;
        Require(enemy.CurrentHealth == 0f && enemy.IsDead
            && enemy.GetComponent<EnemyController>().State == EnemyState.Dead, "Second timed attack kills enemy and enters Dead.");
        Debug.Log("M8 PASS B: attack after 0.6s cooldown damaged 15 -> 0 and set EnemyState.Dead.");

        enemy = Spawn(crawler, player.transform.position + Vector3.forward * 2.1f, false);
        until = Time.time + .7f; while (Time.time < until) yield return null;
        Click(); yield return null; Release(); yield return null;
        Require(enemy.CurrentHealth == 30f, "Enemy center outside range receives no damage.");
        Debug.Log("M8 PASS D: target at 2.1 units remained at 30 Health.");
        Object.Destroy(enemy.gameObject);

        var near = Spawn(crawler, player.transform.position + Vector3.right, false);
        var far = Spawn(crawler, player.transform.position + Vector3.forward * 1.7f, false);
        Click(); yield return null; Release(); yield return null;
        Require(near.CurrentHealth == 15f && far.CurrentHealth == 30f, "Only nearest of two enemies is damaged.");
        Debug.Log("M8 PASS E: nearest target 30 -> 15; farther target stayed 30.");
        Object.Destroy(near.gameObject); Object.Destroy(far.gameObject);

        var buildTarget = Spawn(crawler, player.transform.position + Vector3.right, false);
        inventory.AddResource(ResourceType.Wood, 5);
        PointAt(new Vector3(-6, 0, -5));
        Press(Key.B); yield return null; Press(); yield return null;
        Require(build.IsBuildMode, "B enters Build Mode.");
        int beforeWood = inventory.Wood;
        Click(); yield return null; Release(); yield return null;
        Require(buildTarget.CurrentHealth == 30f && inventory.Wood == beforeWood - 5
            && Object.FindObjectsByType<Fence>(FindObjectsSortMode.None).Length == 1,
            "Build Mode click places Fence without attacking.");
        Press(Key.B); yield return null; Press(); yield return null;
        Require(!build.IsBuildMode, "B exits Build Mode.");
        until = Time.time + .7f; while (Time.time < until) yield return null;
        Click(); yield return null; Release(); yield return null;
        Require(buildTarget.CurrentHealth == 15f && Object.FindObjectsByType<Fence>(FindObjectsSortMode.None).Length == 1,
            "Attack resumes outside Build Mode without another placement.");
        Debug.Log("M8 PASS F: Build Mode click placed one Fence/cost 5/no damage; after exit click dealt 15.");
        Object.Destroy(buildTarget.gameObject);
        foreach (var fence in Object.FindObjectsByType<Fence>(FindObjectsSortMode.None)) Object.Destroy(fence.gameObject);
        yield return null;

        var controller = player.GetComponent<CharacterController>();
        controller.enabled = false; player.transform.position = new Vector3(2, 1, 0); controller.enabled = true;
        int beforeGather = inventory.Wood;
        Press(Key.E); yield return null; Press(); yield return null;
        Require(inventory.Wood > beforeGather && !GameObject.Find("Tree_Test"), "Existing E interaction still gathers Wood.");
        Debug.Log("M8 PASS I interaction: E still gathers Wood.");

        controller.enabled = false; player.transform.position = Vector3.up; controller.enabled = true;
        enemy = Spawn(crawler, player.transform.position + Vector3.forward * 1.5f, true);
        float beforeHealth = health.CurrentHealth;
        until = Time.time + .5f;
        while (health.CurrentHealth == beforeHealth && Time.time < until) yield return null;
        Require(health.CurrentHealth == beforeHealth - 10f, "Nearby enemy deals exact configured 10 damage.");
        float afterFirstHit = health.CurrentHealth;
        until = Time.time + .8f; while (Time.time < until) yield return null;
        Require(health.CurrentHealth == afterFirstHit, "Enemy cannot damage Player before one-second cooldown.");
        until = Time.time + .3f;
        while (health.CurrentHealth == afterFirstHit && Time.time < until) yield return null;
        Require(health.CurrentHealth == afterFirstHit - 10f, "Enemy attacks again after cooldown.");
        Debug.Log("M8 PASS G: enemy damage 100 -> 90 -> 80; interval respected >= 1s.");

        int deaths = 0;
        health.Died += _ => deaths++;
        health.TakeDamage(75f);
        Require(health.CurrentHealth == 5f, "Player prepared at 5 Health.");
        until = Time.time + 1.1f;
        while (!health.IsDead && Time.time < until) yield return null;
        Require(health.CurrentHealth == 0f && health.IsDead && deaths == 1, "Enemy overkill clamps at zero and death occurs once.");
        health.TakeDamage(100f);
        Require(health.CurrentHealth == 0f && deaths == 1, "Repeated damage does not repeat death.");
        Object.Destroy(enemy.gameObject);

        var helplessTarget = Spawn(crawler, player.transform.position + Vector3.right, false);
        until = Time.time + .7f; while (Time.time < until) yield return null;
        Require(!combat.TryAttack() && helplessTarget.CurrentHealth == 30f, "Dead Player cannot attack.");
        Vector3 deadPosition = player.transform.position;
        Press(Key.W); until = Time.time + .3f; while (Time.time < until) yield return null; Press(); yield return null;
        Require(Vector3.Distance(deadPosition, player.transform.position) < .01f, "Dead Player cannot move.");
        Debug.Log("M8 PASS H: Player death once at zero; M9 GameOver stopped combat, movement and gameplay input.");
        Object.Destroy(helplessTarget.gameObject);

        hearth.Heal(100f); hearth.AddFuel(100f);
        game.enabled = true;
        Debug.Log("M8 PASS A-I core: exit Play Mode to discard runtime health/resources/objects; run M5/M6/M7 validations fresh.");
    }

    private static EnemyHealth Spawn(EnemyHealth prefab, Vector3 position, bool activeController)
    {
        var enemy = Object.Instantiate(prefab, position, Quaternion.identity);
        enemy.GetComponent<EnemyController>().enabled = activeController;
        if (activeController) enemy.GetComponent<EnemyController>().SetHearth(Object.FindFirstObjectByType<HearthController>());
        return enemy;
    }

    private static void Click() => InputSystem.QueueStateEvent(mouse,
        new MouseState { position = mouse.position.ReadValue(), buttons = 1 });
    private static void Release() => InputSystem.QueueStateEvent(mouse,
        new MouseState { position = mouse.position.ReadValue() });
    private static void Press(params Key[] keys) => InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
    private static void PointAt(Vector3 point) => InputSystem.QueueStateEvent(mouse,
        new MouseState { position = Camera.main.WorldToScreenPoint(point) });
    private static void Require(bool condition, string message)
    {
        if (!condition) throw new System.Exception("M8 FAIL: " + message);
    }
}
