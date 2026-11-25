using TMPro;
using UnityEngine;

public class EndGameStatsManager : MonoBehaviour
{
    [Header("Player's Statistics")]
    [SerializeField] private int structuresDestroyed;
    [SerializeField] private int workersDied;
    [SerializeField] private float timePlayed;

    private MainMenuUI mainMenu;
    private TextMeshProUGUI timePlayedText;
    private TextMeshProUGUI structuresDestroyedText;
    private TextMeshProUGUI workersDiedText;

    private string TimeToMinuteSecond()
    {
        int minutes = Mathf.FloorToInt(timePlayed / 60);
        int seconds = Mathf.FloorToInt(timePlayed % 60);

        return $"{minutes:00}:{seconds:00}";
    }

    #region AddValuesToStats
    public void StructuresDestroyed(int val)
    {
        structuresDestroyed += val;
    }

    public void WorkersDied(int val)
    {
        workersDied += val;
    }

    public void TimePlayed()
    {
        timePlayed += Time.deltaTime;
    }
    #endregion

    private void Start()
    {
        mainMenu = GameManager.Instance.uiController.GetComponent<MainMenuUI>();
        timePlayedText = mainMenu.timePlayedText;
    }

    private void Update()
    {
        TimePlayed();
        timePlayedText.text = TimeToMinuteSecond();
    }


}
