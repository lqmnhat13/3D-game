using UnityEngine;
using UnityEngine.UI;

public sealed class HUDController : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private HearthController hearth;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private WaveManager waveManager;
    [SerializeField] private Text hudText;

    private int lastPlayerHealth = int.MinValue;
    private int lastPlayerMaxHealth;
    private int lastHearthHealth;
    private int lastHearthMaxHealth;
    private int lastFuel;
    private int lastMaxFuel;
    private int lastWood;
    private int lastStone;
    private int lastDay;
    private int lastTime;
    private int lastEnemies;
    private GameState lastState = (GameState)(-1);

    private void Awake()
    {
        if (playerHealth != null && playerInventory != null && hearth != null
            && gameManager != null && waveManager != null && hudText != null) return;

        Debug.LogError("HUDController requires Player, Hearth, GameManager, WaveManager and Text references.", this);
        enabled = false;
    }

    private void Update()
    {
        int playerCurrent = Mathf.RoundToInt(playerHealth.CurrentHealth);
        int playerMax = Mathf.RoundToInt(playerHealth.MaxHealth);
        int hearthCurrent = Mathf.RoundToInt(hearth.CurrentHealth);
        int hearthMax = Mathf.RoundToInt(hearth.MaxHealth);
        int fuelCurrent = Mathf.RoundToInt(hearth.CurrentFuel);
        int fuelMax = Mathf.RoundToInt(hearth.MaxFuel);
        int time = Mathf.CeilToInt(Mathf.Max(0f, gameManager.RemainingTime));

        if (playerCurrent == lastPlayerHealth && playerMax == lastPlayerMaxHealth
            && hearthCurrent == lastHearthHealth && hearthMax == lastHearthMaxHealth
            && fuelCurrent == lastFuel && fuelMax == lastMaxFuel
            && playerInventory.Wood == lastWood && playerInventory.Stone == lastStone
            && gameManager.CurrentState == lastState && gameManager.CurrentDay == lastDay
            && time == lastTime && waveManager.EnemiesAlive == lastEnemies) return;

        lastPlayerHealth = playerCurrent;
        lastPlayerMaxHealth = playerMax;
        lastHearthHealth = hearthCurrent;
        lastHearthMaxHealth = hearthMax;
        lastFuel = fuelCurrent;
        lastMaxFuel = fuelMax;
        lastWood = playerInventory.Wood;
        lastStone = playerInventory.Stone;
        lastState = gameManager.CurrentState;
        lastDay = gameManager.CurrentDay;
        lastTime = time;
        lastEnemies = waveManager.EnemiesAlive;

        string phase = lastState switch
        {
            GameState.GameOver => "Game Over",
            GameState.Victory => "Victory",
            _ => lastState.ToString()
        };
        hudText.text = $"Player HP: {lastPlayerHealth} / {lastPlayerMaxHealth}\n"
            + $"Hearth HP: {lastHearthHealth} / {lastHearthMaxHealth}\n"
            + $"Fuel: {lastFuel} / {lastMaxFuel}\n"
            + $"Wood: {lastWood}\nStone: {lastStone}\n"
            + $"{phase} {lastDay}\nTime: {lastTime}\nEnemies: {lastEnemies}";
    }
}
