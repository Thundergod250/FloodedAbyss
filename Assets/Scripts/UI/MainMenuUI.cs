using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string mainMenuSceneName = "Main Menu";
    [SerializeField] private string gameSceneName = "MainScene";

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

    public void QuitGame()
    {
        Debug.Log("[MainMenuManager] Quitting game.");

        Application.Quit();
    }
}
