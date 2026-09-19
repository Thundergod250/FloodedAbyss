using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
    public enum UIState
    {
        HUD,
        Dialogue,
        Shop,
        Crafting,
        Building
    }

    [System.Serializable]
    public struct ModalReference
    {
        public UIState state;
        public UiModals modalScript;
    }

    [Header("Dependencies")]
    [SerializeField] private PlayerController playerController;

    [Header("UI Modals Mapping")]
    [SerializeField] private List<ModalReference> modalList;

    [Header("Persistent UI References")]
    public UIInteraction uiInteraction;
    public UINotification notificationPanel;
    public UIHUD UIHUD;

    private Dictionary<UIState, UiModals> modalDictionary;
    private UIState currentState = UIState.HUD;

    private void Awake()
    {
        InitializeDictionary();
    }

    private void Start()
    {
        OpenModal(UIState.HUD);
    }

    private void InitializeDictionary()
    {
        modalDictionary = new Dictionary<UIState, UiModals>();

        foreach (var item in modalList)
        {
            if (item.modalScript != null && !modalDictionary.ContainsKey(item.state))
            {
                modalDictionary.Add(item.state, item.modalScript);
            }
        }
    }
    
    /// Centralized function to change UI state.
    /// Hides all other panels, opens target state, manages cursor and player inputs.
    public void OpenModal(UIState newState)
    {
        currentState = newState;

        // 1. Close ALL registered modal panels via their helper method
        foreach (var modal in modalDictionary.Values)
        {
            if (modal != null)
            {
                modal.SetModalActive(false);
            }
        }

        // 2. Open ONLY the target modal/HUD state
        if (modalDictionary.TryGetValue(newState, out UiModals targetModal))
        {
            if (targetModal != null)
            {
                targetModal.SetModalActive(true);
            }
        }

        // 3. Centralized Cursor & Player Input Control
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

    public void CloseAllModals()
    {
        OpenModal(UIState.HUD);
    }
}