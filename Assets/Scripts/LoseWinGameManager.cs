using UnityEngine;

public class LoseWinGameManager : MonoBehaviour
{
    [Header("Lose Panel")]
    [SerializeField] private int waterHeightBeforeLosing;
    private GameObject losePanel;
    private GameObject winPanel;

    private bool hasPlayerLost;
    private bool hasPlayerWon;

    private void Start()
    {
        if(losePanel== null) losePanel = GameManager.Instance.uiController.GetComponent<MainMenuUI>().losePanel;
        if(winPanel == null) winPanel = GameManager.Instance.uiController.GetComponent<MainMenuUI>().winPanel;

        hasPlayerLost = false;
        hasPlayerWon = false;
        losePanel.SetActive(false);
        winPanel.SetActive(false);
    }

    void Update()
    {
        WaterLevel wl = GameManager.Instance.WaterLevel;
        if (wl.CurrentWaterHeight >= waterHeightBeforeLosing && !hasPlayerLost)
        {
            losePanel.SetActive(true);
            Time.timeScale = 0;

            hasPlayerLost = true;
        }

        if (wl.GreatFloodCompleted&& !hasPlayerLost && !hasPlayerWon)
        {
            hasPlayerWon = true;
            winPanel.SetActive(true);

            Time.timeScale = 0f;
        }
    }
}
