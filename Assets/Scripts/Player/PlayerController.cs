using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private PlayerInput playerInput;

    public InputAction MoveAction { get; private set; }
    public InputAction JumpAction { get; private set; }
    public InputAction UseAction { get; private set; }
    public InputAction InteractAction { get; private set; }
    public InputAction AttackAction { get; private set; }
    public InputAction LookAction { get; private set; }
    public InputAction RunAction { get; private set; }

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();

        // Bind actions once
        MoveAction = playerInput.actions["Move"];
        JumpAction = playerInput.actions["Jump"];
        UseAction = playerInput.actions["Use"];
        InteractAction = playerInput.actions["Interact"];
        AttackAction = playerInput.actions["Shoot"];
        LookAction = playerInput.actions["Look"];
        RunAction = playerInput.actions["Run"];
    }

    private void Start()
    {
        SetInputActive(true);
    }

    private void OnEnable()
    {
        playerInput.actions.Enable();
    }

    private void OnDisable()
    {
        playerInput.actions.Disable();
    }

    public void SetInputActive(bool active)
    {
        if (active)
        {
            playerInput.actions.Enable();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            playerInput.actions.Disable();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void SetInteractionUI(bool isVisible) => GameManager.Instance.uiController?.ToggleInteractionPrompt(isVisible);
}