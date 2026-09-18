using UnityEngine;
using UnityEngine.InputSystem;

public class UIDebug : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private GameObject debugButtonsPanel;

    [Header("Input Action")]
    [SerializeField] private InputActionReference debugToggleAction;

    private void OnEnable()
    {
        if (debugToggleAction != null)
        {
            debugToggleAction.action.Enable();
            debugToggleAction.action.performed += OnToggleDebug;
        }
    }

    private void OnDisable()
    {
        if (debugToggleAction != null)
        {
            debugToggleAction.action.performed -= OnToggleDebug;
            debugToggleAction.action.Disable();
        }
    }

    private void OnToggleDebug(InputAction.CallbackContext context)
    {
        if (debugButtonsPanel != null)
        {
            // Toggle active state
            bool isCurrentlyActive = debugButtonsPanel.activeSelf;
            debugButtonsPanel.SetActive(!isCurrentlyActive);

            Debug.Log($"Debug UI toggled: {!isCurrentlyActive}");
        }
    }
}