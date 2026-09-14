using System.Collections;
using UnityEngine;

public class NPC : Item
{
    [Header("Dialogue Configuration")]
    [TextArea(2, 5)]
    [SerializeField] private string dialogueMessage = "Hello there! Stay standard, traveler.";
    [SerializeField] private float cooldownDuration = 2.0f;

    private bool isTalking = false;
    private bool isOnCooldown = false;

    public override void Activate()
    {
        if (isOnCooldown) return;

        if (!isTalking)
        {
            OpenDialogue();
        }
        else
        {
            CloseDialogue();
        }
    }

    private void OpenDialogue()
    {
        isTalking = true;

        if (UIController.Instance != null)
        {
            UIController.Instance.ShowDialogue(dialogueMessage);
        }
    }

    private void CloseDialogue()
    {
        isTalking = false;

        if (UIController.Instance != null)
        {
            UIController.Instance.HideDialogue();
        }

        StartCoroutine(CooldownRoutine());
    }

    private IEnumerator CooldownRoutine()
    {
        isOnCooldown = true;
        yield return new WaitForSeconds(cooldownDuration);
        isOnCooldown = false;
    }
}