using UnityEngine;
using TMPro;

public class UIController : MonoBehaviour
{
    public static UIController Instance { get; private set; }

    [SerializeField] private UIInteraction uiInteraction;

    [Header("Dialogue System")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueTextComponent;

    [Header("Shop System")]
    [SerializeField] private GameObject shopPanel;

    [Header("Crafting System")]
    [SerializeField] private GameObject craftingPanel;

    [Header("Notification System")]
    [SerializeField] private UINotification notificationPanel;
    
    private PlayerController playerController;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private PlayerController GetPlayerController()
    {
        if (playerController == null)
        {
            if (GameManager.Instance != null && GameManager.Instance.playerController != null)
            {
                playerController = GameManager.Instance.playerController;
            }
            else
            {
                playerController = FindAnyObjectByType<PlayerController>();
            }
        }
        return playerController;
    }

    public void ToggleInteractionPrompt(bool isVisible)
    {
        if (uiInteraction != null)
        {
            uiInteraction.SetUIActive(isVisible);
        }
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
        {
            pc.SetInputActive(false);
        }
    }

    public void HideCrafting()
    {
        if (craftingPanel != null) craftingPanel.SetActive(false);

        PlayerController pc = GetPlayerController();
        if (pc != null)
        {
            pc.SetInputActive(true);
        }
    }
}