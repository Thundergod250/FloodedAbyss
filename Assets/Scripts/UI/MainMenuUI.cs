using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;


public class MainMenuUI : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string mainMenuSceneName = "Main Menu";
    [SerializeField] private string gameSceneName = "MainScene";

    [Header("Player Statistics")]
    [SerializeField] private Image structuresDestroyed;
    [SerializeField] private Image workersDied;
    [SerializeField] private Image timePlayed;
    [SerializeField] private GameObject playerStatsGO;
    public TextMeshProUGUI structuresDestroyedText;
    public TextMeshProUGUI workersDiedText;
    public TextMeshProUGUI timePlayedText;


    public GameObject losePanel;
    public GameObject winPanel;

    public void LoadMainMenu()
    {
        if(Time.timeScale == 0)
        {
            Time.timeScale = 1;
        }

        LoadScene(mainMenuSceneName);
    }

    public void StartGame()
    {
        if (Time.timeScale == 0)
        {
            Time.timeScale = 1;
        }
        LoadScene(gameSceneName);
    }

    public void OpenLostWinScreen(bool win)
    {
        if (win)
        {
            winPanel.SetActive(true);
            playerStatsGO.SetActive(true);
        }
        else
        {
            losePanel.SetActive(true);
            playerStatsGO.SetActive(true);
        }

        ApplyValuesToStats();
    }

    private void LoadScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("[MainMenuManager] Scene name is empty!", this);
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError(
                $"[MainMenuManager] Scene '{sceneName}' cannot be loaded. " +
                "Check the scene name and ensure it is included in Build Settings.",
                this
            );
            return;
        }

        SceneManager.LoadScene(sceneName);
    }

    private void ApplyValuesToStats()
    {
        structuresDestroyedText.text = GameManager.Instance.EndGameStatsManager.StructuresDestroyedTracker.ToString();
        workersDiedText.text = GameManager.Instance.EndGameStatsManager.WorkersDiedTracker.ToString();
    }

    public void QuitGame()
    {
        Debug.Log("[MainMenuManager] Quitting game.");

        Application.Quit();
    }
}
