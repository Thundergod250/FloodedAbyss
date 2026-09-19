using TMPro;
using UnityEngine;

public class UIInteraction : MonoBehaviour
{
    [SerializeField] private GameObject interactionPanel;
    [SerializeField] private TextMeshPro interactionText;

    public void SetUIActive(bool isActive)
    {
        if (interactionPanel != null) 
            interactionPanel.SetActive(isActive);
    }

    public void SetText(string text)
    {
        if (interactionText != null) 
            interactionText.text = text;
    }
}