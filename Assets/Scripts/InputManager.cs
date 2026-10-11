using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class InputManager : MonoBehaviour
{
    [Header("Input Asset")]
    [SerializeField] private InputActionAsset inputActionsAsset;

    // Cached Input Actions
    private InputAction moveAction;
    private InputAction interactAction;
    private InputAction pauseTimeAction;
    private InputAction time1Action;
    private InputAction time2Action;
    private InputAction time3Action;
    private InputAction lookAction;
    private InputAction buildAction;
    private InputAction debugAction;
    private InputAction escapeAction;
    private InputAction scrollAction;
    private InputAction panCameraAction;

    // Public properties for continuous axes/states
    public Vector2 MoveInput { get; private set; }
    public Vector2 LookInput { get; private set; }
    public Vector2 ScrollInput { get; private set; }
    public bool IsPanPressed { get; private set; }

    // UnityEvents for single-frame button triggers
    [Header("Input Events")]
    public UnityEvent OnInteract = new();
    public UnityEvent OnPauseTime = new();
    public UnityEvent OnTime1 = new();
    public UnityEvent OnTime2 = new();
    public UnityEvent OnTime3 = new();
    public UnityEvent OnBuild = new();
    public UnityEvent OnDebug = new();
    public UnityEvent OnEscape = new();

    private void Awake()
    {
        SetupInputs();
    }

    private void SetupInputs()
    {
        if (inputActionsAsset == null)
        {
            Debug.LogError("[InputManager] Input Actions Asset is missing!");
            return;
        }

        var playerMap = inputActionsAsset.FindActionMap("Player");
        if (playerMap == null)
        {
            Debug.LogError("[InputManager] Player Action Map not found!");
            return;
        }

        // Cache individual actions
        moveAction = playerMap.FindAction("Move");
        interactAction = playerMap.FindAction("Interact");
        pauseTimeAction = playerMap.FindAction("PauseTime");
        time1Action = playerMap.FindAction("Time1");
        time2Action = playerMap.FindAction("Time2");
        time3Action = playerMap.FindAction("Time3");
        lookAction = playerMap.FindAction("Look");
        buildAction = playerMap.FindAction("Build");
        debugAction = playerMap.FindAction("Debug");
        escapeAction = playerMap.FindAction("Escape");
        scrollAction = playerMap.FindAction("Scroll");
        panCameraAction = playerMap.FindAction("PanCamera");

        // Hook up UnityEvent callbacks for button presses
        if (interactAction != null)
            interactAction.performed += _ => OnInteract?.Invoke();

        if (pauseTimeAction != null)
            pauseTimeAction.performed += _ => OnPauseTime?.Invoke();

        if (time1Action != null)
            time1Action.performed += _ => OnTime1?.Invoke();

        if (time2Action != null)
            time2Action.performed += _ => OnTime2?.Invoke();

        if (time3Action != null)
            time3Action.performed += _ => OnTime3?.Invoke();

        if (buildAction != null)
            buildAction.performed += _ => OnBuild?.Invoke();

        if (debugAction != null)
            debugAction.performed += _ => OnDebug?.Invoke();

        if (escapeAction != null)
            escapeAction.performed += _ => OnEscape?.Invoke();

        // Handle hold states for camera panning
        if (panCameraAction != null)
        {
            panCameraAction.started += _ => IsPanPressed = true;
            panCameraAction.canceled += _ => IsPanPressed = false;
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
        // Continuously poll value-based inputs if enabled
        if (moveAction != null && moveAction.enabled)
            MoveInput = moveAction.ReadValue<Vector2>();

        if (lookAction != null && lookAction.enabled)
            LookInput = lookAction.ReadValue<Vector2>();

        if (scrollAction != null && scrollAction.enabled)
            ScrollInput = scrollAction.ReadValue<Vector2>();
    }

    public void SetMovementActive(bool isActive)
    {
        if (moveAction != null)
        {
            if (isActive) moveAction.Enable(); 
            else { moveAction.Disable(); MoveInput = Vector2.zero; }
        }

        if (scrollAction != null)
        {
            if (isActive) scrollAction.Enable(); 
            else { scrollAction.Disable(); ScrollInput = Vector2.zero; }
        }

        if (panCameraAction != null)
        {
            if (isActive) panCameraAction.Enable(); 
            else { panCameraAction.Disable(); IsPanPressed = false; }
        }
    }
}