using UnityEngine;

public class LoseGameManager : MonoBehaviour
{
    [Header("Lose Panel")]
    [SerializeField] private int waterHeightBeforeLosing;
    private GameObject losePanel;

    private bool hasPlayerLost;

    private void Start()
    {
        if(losePanel== null)
        {
            losePanel = GameManager.Instance.uiController.GetComponentInChildren<MainMenuUI>().gameObject;
        }
        hasPlayerLost = false;
        losePanel.SetActive(false);
    }

    void Update()
    {
        if (GameManager.Instance.WaterLevel.CurrentWaterHeight == waterHeightBeforeLosing && !hasPlayerLost)
        {
            Time.timeScale = 0;
            hasPlayerLost = true;
            losePanel.SetActive(true);
        }

    }
}
