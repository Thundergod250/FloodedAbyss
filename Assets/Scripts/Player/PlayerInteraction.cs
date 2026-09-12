using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float rayDistance = 3f;
    [SerializeField] private LayerMask interactableLayer = ~0; // Default: All layers

    [Header("Input Stuff")]
    private PlayerController controller;

    private PlayerController playerController;
    private Interactables currentInteractable;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();

        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<Camera>();
        }

        if (controller == null)
        {
            controller = GetComponent<PlayerController>();
        }
    }

    private void Start()
    {
        if (controller != null)
        {
            controller.UseAction.started += UseItem;
            controller.InteractAction.started += Interact;
        }
    }

    private void OnDestroy()
    {
        if (controller != null)
        {
            controller.UseAction.started -= UseItem;
            controller.InteractAction.started -= UseItem;
        }
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