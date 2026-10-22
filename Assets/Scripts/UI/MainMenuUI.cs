using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string mainMenuSceneName = "Main Menu";
    [SerializeField] private string gameSceneName = "MainScene";

    public void LoadMainMenu()
    {
        LoadScene(mainMenuSceneName);
    }

    // NEW: Start the game by loading the gameplay scene.
    public void StartGame()
    {
        LoadScene(gameSceneName);
    }

    // NEW: Load a scene by its name with basic validation.
    private void LoadScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("[MainMenuManager] Scene name is empty!", this);
            return;
        }

        // NEW: Check that the scene is included in the project's build scene list.
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

    // NEW: Quit the application.
    public void QuitGame()
    {
        Debug.Log("[MainMenuManager] Quitting game.");

        Application.Quit();
    }
}
