using UnityEngine;
using TMPro;

public class UIController : MonoBehaviour
{
    [Header("Persistent UI")]
    public UIInteraction uiInteraction;
    public UINotification notificationPanel;
    
    [Header("Modals")]
    public GameObject dialoguePanel;
    public GameObject shopPanel;
    public GameObject craftingPanel;
    public TextMeshProUGUI dialogueTextComponent;
    public GameObject buildPanel;
    private BuildPanelUI buildPanelUI;

    private PlayerController playerController;

    private PlayerController GetPlayerController()
    {
        if (playerController == null)
        {
            if (GameManager.Instance != null && GameManager.Instance.playerController != null)
                playerController = GameManager.Instance.playerController;
            else
                playerController = FindAnyObjectByType<PlayerController>();
        }
        return playerController;
    }

    public void ToggleInteractionPrompt(bool isVisible)
    {
        if (uiInteraction != null)
            uiInteraction.SetUIActive(isVisible);
    }

    public void ShowDialogue(string text)
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        if (dialogueTextComponent != null) dialogueTextComponent.text = text;
    }

    public void HideDialogue()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
    }

    public void ShowShop()
    {
        if (shopPanel != null) shopPanel.SetActive(true);

        PlayerController pc = GetPlayerController();
        if (pc != null)
        {
            pc.SetInputActive(false);
        }
    }

    public void HideShop()
    {
        if (shopPanel != null) shopPanel.SetActive(false);

        PlayerController pc = GetPlayerController();
        if (pc != null)
        {
            pc.SetInputActive(true);
        }
    }

    public void ShowCrafting()
    {
        if (craftingPanel != null) craftingPanel.SetActive(true);

        PlayerController pc = GetPlayerController();
        if (pc != null)
            pc.SetInputActive(false);
    }

    public void HideCrafting()
    {
        if (craftingPanel != null) craftingPanel.SetActive(false);

        PlayerController pc = GetPlayerController();
        if (pc != null)
            pc.SetInputActive(true);
    }

    public void ShowBuildPanel(ItemBuildable buildableBase)
    {
        if (buildPanel != null)
        {
            if (buildPanelUI == null) buildPanelUI = buildPanel.GetComponent<BuildPanelUI>();

            if (buildPanelUI != null)
            {
                buildPanelUI.OpenPanel(buildableBase);
            }
            else
            {
                buildPanel.SetActive(true);
            }
        }

        PlayerController pc = GetPlayerController();
        if (pc != null)
        {
            pc.SetInputActive(false);
        }
    }

    public void HideBuildPanel()
    {
        if (buildPanel != null) buildPanel.SetActive(false);

        PlayerController pc = GetPlayerController();
        if (pc != null)
        {
            pc.SetInputActive(true);
        }
    }
}