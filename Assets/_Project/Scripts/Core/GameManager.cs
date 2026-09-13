using UnityEngine;

public sealed class GameManager : MonoBehaviour
{
    [SerializeField, Min(0.1f)] private float dayDuration = 60f;
    [SerializeField, Min(0.1f)] private float nightDuration = 60f;
    [SerializeField, Min(0f)] private float nightFuelDrainPerSecond = 1f;
    [SerializeField, Min(0f)] private float dayLightIntensity = 2f;
    [SerializeField, Min(0f)] private float nightLightIntensity = 0.1f;
    [SerializeField, Min(1)] private int requiredNightsToWin = 5;
    [SerializeField] private HearthController hearth;
    [SerializeField] private Light directionalLight;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerCombat playerCombat;
    [SerializeField] private PlayerInteractor playerInteractor;
    [SerializeField] private BuildingSystem buildingSystem;
    [SerializeField] private WaveManager waveManager;
    private bool subscribed;

    public GameState CurrentState { get; private set; } = GameState.Day;
    public int CurrentDay { get; private set; } = 1;
    public float RemainingTime { get; private set; }
    public event System.Action<GameState> StateChanged;

    private bool IsTerminal => CurrentState == GameState.GameOver || CurrentState == GameState.Victory;

    private void OnEnable()
    {
        Subscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void Start()
    {
        if (hearth == null || directionalLight == null || playerHealth == null
            || playerMovement == null || playerCombat == null || playerInteractor == null
            || buildingSystem == null || waveManager == null)
        {
            Debug.LogError("GameManager requires Hearth, light, Player gameplay and WaveManager references.", this);
            enabled = false;
            return;
        }

        Subscribe();
        CurrentDay = 1;
        EnterState(GameState.Day);
    }

    private void Update()
    {
        if (IsTerminal) return;
        AdvanceTime(Time.deltaTime);
    }

    private void AdvanceTime(float elapsed)
    {
        if (IsTerminal || !float.IsFinite(elapsed) || elapsed <= 0f) return;
        while (elapsed > 0f)
        {
            float step = Mathf.Min(elapsed, RemainingTime);
            if (CurrentState == GameState.Night)
                hearth.ConsumeFuel(nightFuelDrainPerSecond * step);

            RemainingTime -= step;
            elapsed -= step;
            if (RemainingTime <= 0f)
            {
                if (CurrentState == GameState.Night)
                {
                    if (CurrentDay >= requiredNightsToWin)
                    {
                        if (playerHealth.IsDead || hearth.IsDestroyed) GameOver();
                        else Victory();
                        return;
                    }
                    CurrentDay++;
                    EnterState(GameState.Day);
                }
                else
                {
                    EnterState(GameState.Night);
                }
            }
        }
    }

    private void EnterState(GameState state)
    {
        if (IsTerminal) return;
        CurrentState = state;
        RemainingTime = Mathf.Max(0.1f, state == GameState.Day ? dayDuration : nightDuration);
        directionalLight.intensity = state == GameState.Day ? dayLightIntensity : nightLightIntensity;
        StateChanged?.Invoke(state);
    }

    private void GameOver()
    {
        EndGame(GameState.GameOver);
    }

    private void Victory()
    {
        if (playerHealth.IsDead || hearth.IsDestroyed) GameOver();
        else EndGame(GameState.Victory);
    }

    private void EndGame(GameState state)
    {
        if (IsTerminal) return;
        CurrentState = state;
        RemainingTime = 0f;
        playerCombat.GetComponent<UnityEngine.InputSystem.PlayerInput>().DeactivateInput();
        waveManager.enabled = false;
        playerMovement.enabled = false;
        playerCombat.enabled = false;
        playerInteractor.enabled = false;
        buildingSystem.enabled = false;
        StateChanged?.Invoke(state);
    }

    private void Subscribe()
    {
        if (subscribed || playerHealth == null || hearth == null) return;
        playerHealth.Died += OnPlayerDied;
        hearth.Destroyed += OnHearthDestroyed;
        subscribed = true;
    }

    private void Unsubscribe()
    {
        if (!subscribed) return;
        playerHealth.Died -= OnPlayerDied;
        hearth.Destroyed -= OnHearthDestroyed;
        subscribed = false;
    }

    private void OnPlayerDied(PlayerHealth _) => GameOver();
    private void OnHearthDestroyed(HearthController _) => GameOver();

    private void OnValidate()
    {
        dayDuration = Mathf.Max(0.1f, dayDuration);
        nightDuration = Mathf.Max(0.1f, nightDuration);
        nightFuelDrainPerSecond = Mathf.Max(0f, nightFuelDrainPerSecond);
        dayLightIntensity = Mathf.Max(0f, dayLightIntensity);
        nightLightIntensity = Mathf.Max(0f, nightLightIntensity);
        requiredNightsToWin = Mathf.Max(1, requiredNightsToWin);
    }
}
