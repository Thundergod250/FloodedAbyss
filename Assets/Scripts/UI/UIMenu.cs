using UnityEngine;

public class UIMenu : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject objectivePanel;

    public void OpenMenu()
    {
        menuPanel.SetActive(!menuPanel.activeSelf);
        UpdatePauseState();
    }

    public void OpenSettings()
    {
        settingsPanel.SetActive(!settingsPanel.activeSelf);
        UpdatePauseState();
    }

    public void OpenObjective()
    {
        objectivePanel.SetActive(!objectivePanel.activeSelf);
    }

    public void CloseMenus()
    {
        menuPanel.SetActive(false);
        settingsPanel.SetActive(false);

        UpdatePauseState();
    }

    private void UpdatePauseState()
    {
        bool shouldPause =
            menuPanel.activeSelf ||
            settingsPanel.activeSelf;

        Time.timeScale = shouldPause ? 0f : 1f;
    }
}
