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
        mainMenu = GameManager.Instance.uiController.GetComponent<MainMenuUI>();

        hasPlayerLost = false;
        hasPlayerWon = false;
    }

    void Update()
    {
        WaterLevel wl = GameManager.Instance.WaterLevel;
        if (wl.CurrentWaterHeight >= waterHeightBeforeLosing && !hasPlayerLost)
        {
            hasPlayerLost = true;
            mainMenu.OpenLostWinScreen(false);

            Time.timeScale = 0;
        }

        if (wl.GreatFloodCompleted&& !hasPlayerLost && !hasPlayerWon)
        {
            hasPlayerWon = true;
            mainMenu.OpenLostWinScreen(true);

            Time.timeScale = 0f;
        }
    }
}
