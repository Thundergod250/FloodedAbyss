using TMPro;
using UnityEngine;

public class UIDialoguePanel : UiModals
{
    [SerializeField] private TextMeshProUGUI dialogueTextComponent;

    public void ShowDialogue(string text)
    {
        if (dialogueTextComponent != null)
        {
            dialogueTextComponent.text = text;
        }

        if (GameManager.Instance != null && GameManager.Instance.uiController != null)
        {
            GameManager.Instance.uiController.OpenModal(UIController.UIState.Dialogue);
        }
    }
}