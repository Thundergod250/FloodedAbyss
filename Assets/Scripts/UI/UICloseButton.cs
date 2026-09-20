using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UICloseButton : MonoBehaviour
{
    private Button closeButton;

    private void Awake() => closeButton = GetComponent<Button>();

    private void OnEnable()
    {
        if (closeButton != null) 
            closeButton.onClick.AddListener(OnCloseClicked);
    }

    private void OnDisable()
    {
        if (closeButton != null) 
            closeButton.onClick.RemoveListener(OnCloseClicked);
    }

    private void OnCloseClicked()
    {
        if (GameManager.Instance != null && GameManager.Instance.uiController != null)
            GameManager.Instance.uiController.CloseAllModals();
        else
            Debug.LogWarning("[UICloseButton] GameManager or UIController instance is missing!");
    }
}