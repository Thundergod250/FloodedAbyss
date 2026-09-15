using UnityEngine;
using TMPro;

public class UIController : MonoBehaviour
{
    public static UIController Instance { get; private set; }

    [Header("Interaction Prompt")]
    [SerializeField] private UIInteraction uiInteraction;

    [Header("Dialogue System")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueTextComponent;

    [Header("Shop System")]
    [SerializeField] private GameObject shopPanel;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
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
    }

    public void HideShop()
    {
        if (shopPanel != null) shopPanel.SetActive(false);
    }
}