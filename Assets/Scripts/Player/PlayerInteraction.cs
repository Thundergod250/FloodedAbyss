using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Raycast Settings")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float rayDistance = 3f;
    [SerializeField] private LayerMask interactableLayer = ~0; // Default: All layers

    [Header("Input Stuff")]
    [SerializeField] private PlayerInput input;
    [SerializeField] private InputAction useAction;
    [SerializeField] private InputAction interactAction;

    private PlayerController playerController;
    private Interactables currentInteractable;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();

        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<Camera>();
        }

        if (input == null)
        {
            input = GetComponent<PlayerInput>();
        }

        useAction = input.actions.FindAction("Use");
        interactAction = input.actions.FindAction("Interact");
    }

    private void OnEnable()
    {
        useAction.started += UseItem;
        interactAction.started += Interact;
    }

    private void OnDisable()
    {
        useAction.started -= UseItem;
        interactAction.started -= Interact;
    }

    private void Update()
    {
        CheckForInteractable();
    }

    private void CheckForInteractable()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, interactableLayer))
        {
            Interactables interactable = hit.collider.GetComponent<Interactables>();

            if (interactable != null)
            {
                if (currentInteractable != interactable)
                {
                    currentInteractable = interactable;
                    playerController.SetInteractionUI(true);
                }
                return;
            }
        }

        if (currentInteractable != null)
        {
            currentInteractable = null;
            playerController.SetInteractionUI(false);
        }
    }

    private void UseItem(InputAction.CallbackContext context)
    {
        Debug.Log("Use Item!");
    }

    private void Interact(InputAction.CallbackContext context)
    {
        if (currentInteractable != null)
        {
            currentInteractable.Interact();
        }
    }
}