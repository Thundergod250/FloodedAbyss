using UnityEngine;

public class UIController : MonoBehaviour
{
    public static UIController Instance { get; private set; }

    [SerializeField] private UIInteraction uiInteraction;

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
}