using UnityEngine;

public class LoseWinGameManager : MonoBehaviour
{
    [Header("Lose Panel")]
    [SerializeField] private int waterHeightBeforeLosing;

    private MainMenuUI mainMenu;

    private bool hasPlayerLost;
    private bool hasPlayerWon;

    private void Start()
    {
        mainMenu = GameManager.Instance.uiController
            .GetComponent<MainMenuUI>();

        hasPlayerLost = false;
        hasPlayerWon = false;

        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (hasPlayerLost || hasPlayerWon)
            return;

        WaterLevel wl = GameManager.Instance.WaterLevel;

        if (wl == null)
            return;

        if (wl.GreatFloodCompleted)
        {
            WinGame();
        }
        else if (wl.CurrentWaterHeight >= waterHeightBeforeLosing)
        {
            LoseGame();
        }
    }

    private void WinGame()
    {
        hasPlayerWon = true;

        mainMenu.OpenLostWinScreen(true);

        Time.timeScale = 0f;
    }

    private void LoseGame()
    {
        hasPlayerLost = true;

        mainMenu.OpenLostWinScreen(false);

        Time.timeScale = 0f;
    }
}
