using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIController : MonoBehaviour
{
    public enum UIState
    {
        HUD,
        Dialogue,
        Shop,
        Crafting,
        Building,
        SkillTree,
        Structure,
        AutoMiner
    }

    [System.Serializable]
    public struct ModalReference
    {
        public UIState state;
        public UiModals modalScript;
    }

    [Header("Dependencies")]
    [SerializeField] private PlayerController playerController;

    [Header("Input Settings")]
    [SerializeField] private InputActionReference escapeAction;

    [Header("UI Modals Mapping")]
    [SerializeField] private List<ModalReference> modalList;

    [Header("Persistent UI References")]
    public UIInteraction uiInteraction;
    public UINotification uiNotification;
    public UIHUD UIHUD;

    private Dictionary<UIState, UiModals> modalDictionary;
    private UIState currentState = UIState.HUD;

    public UIState CurrentState => currentState;

    private void Awake() => InitializeDictionary();

    private void OnEnable()
    {
        if (escapeAction != null)
        {
            escapeAction.action.Enable();
            escapeAction.action.performed += OnEscapePerformed;
        }
    }

    private void OnDisable()
    {
        if (escapeAction != null)
        {
            escapeAction.action.performed -= OnEscapePerformed;
            escapeAction.action.Disable();
        }
    }

    private void Start() => OpenModal(UIState.HUD);

    private void OnEscapePerformed(InputAction.CallbackContext context)
    {
        if (currentState != UIState.HUD)
            CloseAllModals();
    }

    private void InitializeDictionary()
    {
        modalDictionary = new Dictionary<UIState, UiModals>();

        foreach (var item in modalList)
        {
            if (item.modalScript != null && !modalDictionary.ContainsKey(item.state))
                modalDictionary.Add(item.state, item.modalScript);
        }
    }

    public T GetModal<T>(UIState state) where T : UiModals
    {
        if (modalDictionary != null && modalDictionary.TryGetValue(state, out UiModals modal))
            return modal as T;

        Debug.LogWarning($"[UIController] Modal for state {state} not found or invalid type.");
        return null;
    }

    public void OpenModal(UIState newState)
    {
        currentState = newState;

        foreach (var modal in modalDictionary.Values)
        {
            if (modal != null)
                modal.SetModalActive(false);
        }

        if (modalDictionary.TryGetValue(newState, out UiModals targetModal))
        {
            if (targetModal != null)
                targetModal.SetModalActive(true);
        }

        bool isGameplay = (newState == UIState.HUD);
        SetCursorState(!isGameplay);

        if (playerController != null)
            playerController.SetInputActive(isGameplay);
    }

    private void SetCursorState(bool showCursor)
    {
        Cursor.visible = showCursor;
        Cursor.lockState = showCursor ? CursorLockMode.None : CursorLockMode.Locked;
    }

    public void CloseAllModals() => OpenModal(UIState.HUD);
}