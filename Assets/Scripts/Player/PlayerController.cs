using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private PlayerInput playerInput;

    public InputAction MoveAction { get; private set; }
    public InputAction JumpAction { get; private set; }

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();

        // Bind actions once
        MoveAction = playerInput.actions["Move"];
        JumpAction = playerInput.actions["Jump"];
    }

    private void OnEnable()
    {
        playerInput.actions.Enable();
    }

    private void OnDisable()
    {
        playerInput.actions.Disable();
    }

    public void SetInteractionUI(bool isVisible)
    {
        if (UIController.Instance != null)
        {
            UIController.Instance.ToggleInteractionPrompt(isVisible);
        }
    }
}