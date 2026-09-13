using UnityEngine;

public sealed class EndGameUI : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject victoryPanel;

    private void OnEnable()
    {
        if (gameManager == null || gameOverPanel == null || victoryPanel == null)
        {
            Debug.LogError("EndGameUI requires GameManager, GameOverPanel and VictoryPanel references.", this);
            enabled = false;
            return;
        }

        gameManager.StateChanged += ShowState;
        ShowState(gameManager.CurrentState);
    }

    private void OnDisable()
    {
        if (gameManager != null) gameManager.StateChanged -= ShowState;
    }

    private void ShowState(GameState state)
    {
        gameOverPanel.SetActive(state == GameState.GameOver);
        victoryPanel.SetActive(state == GameState.Victory);
    }
}
