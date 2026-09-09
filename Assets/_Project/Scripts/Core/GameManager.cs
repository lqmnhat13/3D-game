using UnityEngine;

public sealed class GameManager : MonoBehaviour
{
    [SerializeField, Min(0.1f)] private float dayDuration = 60f;
    [SerializeField, Min(0.1f)] private float nightDuration = 60f;
    [SerializeField, Min(0f)] private float nightFuelDrainPerSecond = 1f;
    [SerializeField, Min(0f)] private float dayLightIntensity = 2f;
    [SerializeField, Min(0f)] private float nightLightIntensity = 0.1f;
    [SerializeField] private HearthController hearth;
    [SerializeField] private Light directionalLight;

    public GameState CurrentState { get; private set; } = GameState.Day;
    public int CurrentDay { get; private set; } = 1;
    public float RemainingTime { get; private set; }

    private void Start()
    {
        if (hearth == null || directionalLight == null)
        {
            Debug.LogError("GameManager requires Hearth and Directional Light references.", this);
            enabled = false;
            return;
        }

        CurrentDay = 1;
        EnterState(GameState.Day);
    }

    private void Update()
    {
        AdvanceTime(Time.deltaTime);
    }

    private void AdvanceTime(float elapsed)
    {
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
        CurrentState = state;
        RemainingTime = Mathf.Max(0.1f, state == GameState.Day ? dayDuration : nightDuration);
        directionalLight.intensity = state == GameState.Day ? dayLightIntensity : nightLightIntensity;
    }

    private void OnValidate()
    {
        dayDuration = Mathf.Max(0.1f, dayDuration);
        nightDuration = Mathf.Max(0.1f, nightDuration);
        nightFuelDrainPerSecond = Mathf.Max(0f, nightFuelDrainPerSecond);
        dayLightIntensity = Mathf.Max(0f, dayLightIntensity);
        nightLightIntensity = Mathf.Max(0f, nightLightIntensity);
    }
}
