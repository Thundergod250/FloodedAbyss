using UnityEngine;

public class LoseGameManager : MonoBehaviour
{
    [Header("Lose Panel")]
    [SerializeField] private GameObject losePanel;

    private bool hasPlayerLost;

    private void Start()
    {
        hasPlayerLost = false;
        losePanel.SetActive(false);
    }

    void Update()
    {
        if (GameManager.Instance.WaterLevel.CurrentWaterHeight == 300 && !hasPlayerLost)
        {
            Time.timeScale = 0;
            hasPlayerLost = true;
            losePanel.SetActive(true);
        }

    }
}
