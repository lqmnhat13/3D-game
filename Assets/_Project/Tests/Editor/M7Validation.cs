using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public static class M7Validation
{
    private static IEnumerator checks;
    private static Keyboard keyboard;
    private static Mouse mouse;
    private static InputDevice[] originalDevices;
    private static bool background;

    [MenuItem("Tools/Validation/Run M7 (in Play Mode)")]
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
        var build = player.GetComponent<BuildingSystem>();
        var inventory = player.GetComponent<PlayerInventory>();
        var game = Object.FindFirstObjectByType<GameManager>();
        var hearth = Object.FindFirstObjectByType<HearthController>();
        var prefab = AssetDatabase.LoadAssetAtPath<PlaceableBuilding>("Assets/_Project/Prefabs/Buildings/WoodenFence.prefab");
        var crawler = AssetDatabase.LoadAssetAtPath<EnemyHealth>("Assets/_Project/Prefabs/Enemies/ShadowCrawler.prefab");
        Require(build != null && build.enabled && game.CurrentDay == 1 && game.CurrentState == GameState.Day, "M7 references and fresh Day 1.");
        game.enabled = false;
        originalDevices = System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Where(InputSystem.devices,
            d => d.enabled && (d is Mouse || d is Keyboard)));
        foreach (var device in originalDevices) InputSystem.DisableDevice(device);
        keyboard = InputSystem.AddDevice<Keyboard>();
        mouse = InputSystem.AddDevice<Mouse>();
        player.GetComponent<PlayerInput>().SwitchCurrentControlScheme("Keyboard&Mouse", keyboard, mouse);
        inventory.AddResource(ResourceType.Wood, 20);
        PointAt(new Vector3(-6.2f, 0, -5.7f));
        Press(Key.B);
        float until = Time.time + .2f;
        while (Time.time < until) yield return null;
        Press();
        Require(build.IsBuildMode, "B toggles build mode via PlayerInput.");
        var preview = GameObject.Find("Fence Preview");
        Require(preview != null && Vector3.Distance(preview.transform.position, new Vector3(-6, 0, -6)) < .05f, "Preview follows mouse and snaps.");
        Require(preview.GetComponentsInChildren<Collider>().Length == 0 && preview.GetComponentsInChildren<NavMeshObstacle>().Length == 0
            && preview.GetComponentsInChildren<Fence>().Length == 0, "Preview has no gameplay components.");
        PointAt(new Vector3(-5.6f, 0, -4.8f));
        until = Time.time + .2f; while (Time.time < until) yield return null;
        Require(Vector3.Distance(preview.transform.position, new Vector3(-6, 0, -5)) < .05f, "Preview updates to second snapped position.");
        Debug.Log("M7 PASS A: B input, preview follows two mouse positions, unit snapping, visual-only preview.");

        int before = inventory.Wood;
        InputSystem.QueueStateEvent(mouse, new MouseState { position = Camera.main.WorldToScreenPoint(new Vector3(-5.6f, 0, -4.8f)), buttons = 1 });
        until = Time.time + .2f; while (Time.time < until) yield return null;
        PointAt(new Vector3(-5.6f, 0, -4.8f));
        var fences = Object.FindObjectsByType<Fence>(FindObjectsSortMode.None);
        Require(fences.Length == 1 && inventory.Wood == before - 5, "Mouse placement creates exactly one Fence, costs exactly five Wood.");
        Require(Vector3.Distance(fences[0].transform.position, new Vector3(-6, 0, -5)) < .05f, "Click places at preview position.");
        Debug.Log("M7 PASS B: left-click placement, Wood " + before + " -> " + inventory.Wood + ", one Fence.");

        inventory.SpendResource(ResourceType.Wood, inventory.Wood - 4);
        Require(!build.TryPlace(Down(-10, -5)) && inventory.Wood == 4 && CountFences() == 1, "Insufficient Wood leaves resources and Fence count unchanged.");
        Debug.Log("M7 PASS C: four Wood rejects five-Wood Fence; Wood unchanged.");
        inventory.AddResource(ResourceType.Wood, 20);
        before = inventory.Wood;
        Require(!build.TryPlace(Down(4, 4)), "Hearth overlap rejected without a Hearth collider.");
        Require(!build.TryPlace(Down(fences[0].transform.position.x, fences[0].transform.position.z)), "Existing Fence overlap rejected.");
        Require(!build.TryPlace(Down(0, 0)), "Player overlap rejected.");
        Require(!build.TryPlace(Down(100, 100)), "Non-Ground rejected.");
        Require(!build.TryPlace(Down(24.4f, -5)), "Footprint beyond Ground edge rejected after snapping.");
        Require(inventory.Wood == before && CountFences() == 1, "All invalid attempts preserve Wood and count.");
        Debug.Log("M7 PASS D: Hearth, Fence, Player, non-Ground and snapped edge rejected; no resource loss.");

        var character = player.GetComponent<CharacterController>();
        var originalPosition = player.transform.position;
        character.enabled = false;
        player.transform.position = fences[0].transform.position + new Vector3(0, 1, -2);
        character.enabled = true;
        for (int i = 0; i < 30; i++) character.Move(Vector3.forward * .1f);
        Require(player.transform.position.z < fences[0].transform.position.z - .6f, "Fence collider physically stops Player.");
        character.enabled = false; player.transform.position = originalPosition; character.enabled = true;
        Debug.Log("M7 PASS physics: CharacterController cannot walk through Fence.");

        var fence = fences[0];
        Require(fence.CurrentHealth == 50 && fence.MaxHealth == 50 && !fence.IsDestroyed, "Fence starts at max 50.");
        fence.TakeDamage(-5); fence.TakeDamage(float.NaN); fence.TakeDamage(float.PositiveInfinity);
        Require(fence.CurrentHealth == 50, "Invalid damage ignored.");
        fence.TakeDamage(12); Require(fence.CurrentHealth == 38, "Exact damage 50 -> 38.");
        fence.TakeDamage(1000); fence.TakeDamage(1000);
        Require(fence.CurrentHealth == 0 && fence.IsDestroyed && !fence.gameObject.activeSelf, "Overkill clamps, repeated destruction ignored, immediately deactivated.");
        yield return null; yield return null;
        Require(fence == null && CountFences() == 0, "Destroyed Fence removed.");
        Debug.Log("M7 PASS E: initial 50; exact 12 damage -> 38; overkill/repeated damage -> zero, deactivated then removed.");

        build.SetBuildMode(false);
        var start = player.transform.position;
        Press(Key.W);
        until = Time.time + .35f; while (Time.time < until) yield return null;
        Press();
        Require(Vector3.Distance(start, player.transform.position) > .5f, "W moves Player.");
        var controller = player.GetComponent<CharacterController>();
        controller.enabled = false; player.transform.position = new Vector3(2, 1, 0); controller.enabled = true;
        before = inventory.Wood;
        Press(Key.E);
        until = Time.time + .2f; while (Time.time < until) yield return null;
        Press();
        Require(inventory.Wood > before && !GameObject.Find("Tree_Test"), "E gathers tree via existing Interact.");
        controller.enabled = false; player.transform.position = new Vector3(-10, 1, -10); controller.enabled = true;
        Debug.Log("M7 PASS H controls: real InputSystem W movement and E gathering.");

        var enemy = Object.Instantiate(crawler, new Vector3(4, 1, -8), Quaternion.identity);
        enemy.GetComponent<EnemyController>().SetHearth(hearth);
        until = Time.time + .2f; while (Time.time < until) yield return null;
        build.SetBuildMode(true);
        Require(build.TryPlace(Down(4, 0)), "Place dynamic alternate-route Fence.");
        build.SetBuildMode(false);
        fence = Object.FindFirstObjectByType<Fence>();
        until = Time.time + .4f; while (Time.time < until) yield return null;
        Require(NavMesh.Raycast(new Vector3(4, 0, -4), new Vector3(4, 0, 4), out _, NavMesh.AllAreas), "Carving blocks direct NavMesh segment.");
        var path = new NavMeshPath();
        Require(NavMesh.CalculatePath(new Vector3(4, 0, -4), new Vector3(4, 0, 4), NavMesh.AllAreas, path)
            && path.status == NavMeshPathStatus.PathComplete, "Alternate route stays complete.");
        float maxDetour = 0;
        until = Time.time + 10f;
        while (hearth.CurrentHealth == 100 && Time.time < until)
        {
            maxDetour = Mathf.Max(maxDetour, Mathf.Abs(enemy.transform.position.x - 4));
            yield return null;
        }
        Require(hearth.CurrentHealth == 90 && maxDetour > 1.4f && fence.CurrentHealth == 50, "Enemy detours around intact Fence and attacks Hearth.");
        Debug.Log("M7 PASS F: dynamically carved route, complete alternate path; lateral detour " + maxDetour.ToString("F2") + "; Fence 50, Hearth 100 -> 90.");
        Object.Destroy(enemy.gameObject); Object.Destroy(fence.gameObject); hearth.Heal(100);
        until = Time.time + .5f; while (Time.time < until) yield return null;

        // Runtime-only wall crosses the existing NavMesh; no bake or saved Scene changes.
        for (int x = -24; x <= 24; x += 3) Object.Instantiate(prefab, new Vector3(x, 0, 0), Quaternion.identity);
        until = Time.time + .5f; while (Time.time < until) yield return null;
        NavMesh.CalculatePath(new Vector3(4, 0, -4), new Vector3(4, 0, 4), NavMesh.AllAreas, path);
        Require(path.status == NavMeshPathStatus.PathPartial, "Wall makes Hearth route partial.");
        var spawner = Object.FindFirstObjectByType<EnemySpawner>();
        var spawnerData = new SerializedObject(spawner);
        var pointsProperty = spawnerData.FindProperty("spawnPoints");
        var savedPoints = new Transform[pointsProperty.arraySize];
        for (int i = 0; i < savedPoints.Length; i++) savedPoints[i] = (Transform)pointsProperty.GetArrayElementAtIndex(i).objectReferenceValue;
        pointsProperty.arraySize = 1;
        pointsProperty.GetArrayElementAtIndex(0).objectReferenceValue = GameObject.Find("SpawnPoint_South").transform;
        spawnerData.ApplyModifiedPropertiesWithoutUndo();
        enemy = spawner.Spawn();
        pointsProperty.arraySize = savedPoints.Length;
        for (int i = 0; i < savedPoints.Length; i++) pointsProperty.GetArrayElementAtIndex(i).objectReferenceValue = savedPoints[i];
        spawnerData.ApplyModifiedPropertiesWithoutUndo();
        Require(enemy != null, "Existing EnemySpawner permits a Fence-blocked partial route.");
        Fence attacked = null;
        float lastHealth = 50, lastHit = 0;
        int hits = 0;
        until = Time.time + 15f;
        while (Time.time < until)
        {
            if (attacked == null && hits == 0)
                foreach (var candidate in Object.FindObjectsByType<Fence>(FindObjectsSortMode.None))
                    if (candidate.CurrentHealth < 50) { attacked = candidate; break; }
            if (attacked != null && attacked.CurrentHealth < lastHealth)
            {
                Require(lastHealth - attacked.CurrentHealth == 10, "Fence damage follows enemy attackDamage 10.");
                if (hits > 0) Require(Time.time - lastHit >= .95f, "Fence attack cooldown at least one second.");
                Debug.Log("M7 Fence hit " + (hits + 1) + ": " + lastHealth + " -> " + attacked.CurrentHealth + ", interval=" + (Time.time - lastHit).ToString("F3"));
                lastHealth = attacked.CurrentHealth; lastHit = Time.time; hits++;
            }
            if (hits > 0 && attacked == null) break;
            yield return null;
        }
        // Destruction can remove the object between editor ticks; four observed hits plus removal proves final hit.
        Require(attacked == null && hits >= 4 && lastHealth <= 10, "AI destroys blocking Fence after timed hits.");
        Require(hearth.CurrentHealth == 100, "Hearth protected while Fence blocks route.");
        until = Time.time + 8f;
        while (hearth.CurrentHealth == 100 && Time.time < until) yield return null;
        Require(hearth.CurrentHealth == 90 && enemy.transform.position.z > 1, "Enemy resumes through opened gap and attacks Hearth.");
        NavMesh.CalculatePath(new Vector3(4, 0, -4), new Vector3(4, 0, 4), NavMesh.AllAreas, path);
        Require(path.status == NavMeshPathStatus.PathComplete, "Navigation restored after destruction.");
        Debug.Log("M7 PASS G: partial route -> Fence attacks at 10 damage / >=1s -> destroyed -> complete path -> Hearth attack.");
        Object.Destroy(enemy.gameObject);
        foreach (var remaining in Object.FindObjectsByType<Fence>(FindObjectsSortMode.None)) Object.Destroy(remaining.gameObject);
        hearth.Heal(100); hearth.AddFuel(100);
        game.enabled = true;
        Debug.Log("M7 PASS A-H (controls/navigation): exit Play to restore runtime setup; run M6Validation in a fresh session for wave regression.");
    }

    private static Ray Down(float x, float z) => new(new Vector3(x, 10, z), Vector3.down);
    private static int CountFences() => Object.FindObjectsByType<Fence>(FindObjectsSortMode.None).Length;
    private static void Press(params Key[] keys) => InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
    private static void PointAt(Vector3 point) => InputSystem.QueueStateEvent(mouse, new MouseState { position = Camera.main.WorldToScreenPoint(point) });
    private static void Require(bool condition, string message)
    {
        if (!condition) throw new System.Exception("M7 FAIL: " + message);
    }
}
