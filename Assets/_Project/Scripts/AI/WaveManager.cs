using System.Collections.Generic;
using UnityEngine;

public sealed class WaveManager : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private EnemySpawner spawner;
    [SerializeField, Min(1)] private int baseEnemyCount = 3;
    [SerializeField, Min(0)] private int enemiesAddedPerNight = 2;
    [SerializeField, Min(0.1f)] private float spawnInterval = 1.5f;
    private readonly HashSet<EnemyHealth> living = new();
    private float nextSpawnTime;

    public int CurrentWave { get; private set; }
    public int TotalEnemiesScheduled { get; private set; }
    public int EnemiesSpawned { get; private set; }
    public int EnemiesAlive => living.Count;
    public bool IsWaveActive { get; private set; }

    private void OnEnable()
    {
        if (gameManager == null || spawner == null)
        {
            Debug.LogError("WaveManager requires GameManager and EnemySpawner references.", this);
            enabled = false;
            return;
        }
        gameManager.StateChanged += OnStateChanged;
        OnStateChanged(gameManager.CurrentState);
    }

    private void OnDisable()
    {
        if (gameManager != null) gameManager.StateChanged -= OnStateChanged;
        Cleanup();
    }

    private void OnStateChanged(GameState state)
    {
        if (state == GameState.Day) { Cleanup(); return; }
        if (state != GameState.Night || CurrentWave == gameManager.CurrentDay) return;
        CurrentWave = gameManager.CurrentDay;
        TotalEnemiesScheduled = Mathf.Max(1, baseEnemyCount) + (CurrentWave - 1) * Mathf.Max(0, enemiesAddedPerNight);
        EnemiesSpawned = 0;
        IsWaveActive = true;
        nextSpawnTime = Time.time + Mathf.Max(0.1f, spawnInterval);
    }

    private void Update()
    {
        if (!IsWaveActive || gameManager.CurrentState != GameState.Night
            || EnemiesSpawned >= TotalEnemiesScheduled || Time.time < nextSpawnTime) return;
        nextSpawnTime = Time.time + Mathf.Max(0.1f, spawnInterval);
        EnemyHealth enemy = spawner.Spawn();
        if (enemy == null)
        {
            Debug.LogError("Wave spawn failed: check prefab, Hearth and reachable NavMesh spawn points.", this);
            enabled = false;
            return;
        }
        living.Add(enemy);
        enemy.Died += OnEnemyDied;
        EnemiesSpawned++;
    }

    private void OnEnemyDied(EnemyHealth enemy)
    {
        if (!living.Remove(enemy)) return;
        enemy.Died -= OnEnemyDied;
        Destroy(enemy.gameObject);
        if (EnemiesSpawned == TotalEnemiesScheduled && living.Count == 0) IsWaveActive = false;
    }

    private void Cleanup()
    {
        IsWaveActive = false;
        foreach (EnemyHealth enemy in living)
        {
            if (enemy == null) continue;
            enemy.Died -= OnEnemyDied;
            enemy.gameObject.SetActive(false);
            Destroy(enemy.gameObject);
        }
        living.Clear();
        CurrentWave = TotalEnemiesScheduled = EnemiesSpawned = 0;
    }
}
