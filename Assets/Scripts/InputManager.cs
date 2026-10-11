using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class InputManager : MonoBehaviour
{
    [Header("Input Asset")]
    [SerializeField] private InputActionAsset inputActionsAsset;

    // Cached Input Actions
    private InputAction moveAction;
    private InputAction interactAction;
    private InputAction scrollAction;
    private InputAction panCameraAction;

    // Public properties for GameManager or other systems to read easily
    public Vector2 MoveInput { get; private set; }
    public Vector2 ScrollInput { get; private set; }
    public bool IsPanPressed { get; private set; }

    // Events for single-frame triggers
    public event Action OnInteractPerformed;
    public event Action OnPauseTimePressed;
    public event Action OnBuildPressed;

    private void Awake()
    {
        SetupInputs();
    }

    private void SetupInputs()
    {
        if (inputActionsAsset == null)
        {
            Debug.LogError("Input Actions Asset is missing on InputManager!");
            return;
        }

        var playerMap = inputActionsAsset.FindActionMap("Player");
        if (playerMap == null)
        {
            Debug.LogError("Player Action Map not found!");
            return;
        }

        // Cache individual actions
        moveAction = playerMap.FindAction("Move");
        interactAction = playerMap.FindAction("Interact");
        scrollAction = playerMap.FindAction("Scroll");
        panCameraAction = playerMap.FindAction("PanCamera");

        // Hook up event callbacks
        if (interactAction != null)
            interactAction.performed += ctx => OnInteractPerformed?.Invoke();

        var pauseAction = playerMap.FindAction("PauseTime");
        if (pauseAction != null)
            pauseAction.performed += ctx => OnPauseTimePressed?.Invoke();

        var buildAction = playerMap.FindAction("Build");
        if (buildAction != null)
            buildAction.performed += ctx => OnBuildPressed?.Invoke();

        if (panCameraAction != null)
        {
            panCameraAction.started += ctx => IsPanPressed = true;
            panCameraAction.canceled += ctx => IsPanPressed = false;
        }
    }

    private void OnEnable()
    {
        if (inputActionsAsset != null)
            inputActionsAsset.Enable();
    }

    private void OnDisable()
    {
        if (inputActionsAsset != null)
            inputActionsAsset.Disable();
    }

    private void Update()
    {
        // Continuously poll value-based inputs
        if (moveAction != null)
            MoveInput = moveAction.ReadValue<Vector2>();

        if (scrollAction != null)
            ScrollInput = scrollAction.ReadValue<Vector2>();
    }
}