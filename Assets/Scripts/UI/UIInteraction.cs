using UnityEngine;

public class UIInteraction : MonoBehaviour
{
    [SerializeField] private GameObject interactionPanel;

    public void SetUIActive(bool isActive)
    {
        if (interactionPanel != null)
        {
            interactionPanel.SetActive(isActive);
        }
    }
}