using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private UIController playerUI; 
    private PlayerInput playerInput;
    
    public InputAction MoveAction { get; private set; }
    public InputAction JumpAction { get; private set; }
    public InputAction UseAction { get; private set; }
    public InputAction InteractAction { get; private set; }
    public InputAction AttackAction { get; private set; }
    public InputAction LookAction { get; private set; }

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
    }

    private void OnEnable()
    {
        playerInput.actions.Enable();
    }

    private void OnDisable()
    {
        playerInput.actions.Disable();
    }
}