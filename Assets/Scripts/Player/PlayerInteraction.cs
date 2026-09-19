using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float rayDistance = 3f;
    [SerializeField] private LayerMask interactableLayer = ~0; // Default: All layers

    [Header("Input Stuff")]
    private PlayerController playerController;
    private Interactables currentInteractable;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
    }

    private void Start()
    {
        if (playerController != null)
        {
            playerController.UseAction.started += UseItem;
            playerController.InteractAction.started += Interact;
        }
    }

    private void OnDestroy()
    {
        if (playerController != null)
        {
            playerController.UseAction.started -= UseItem;
            playerController.InteractAction.started -= Interact; // Fixed unsubscribing the correct action
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
            Interactables interactable = hit.collider.GetComponentInParent<Interactables>();
            if (interactable == null)
            {
                interactable = hit.collider.GetComponent<Interactables>();
            }

            if (interactable != null)
            {
                if (currentInteractable != interactable)
                {
                    currentInteractable = interactable;
                    GameManager.Instance.uiController?.ToggleInteractionPrompt(true);
                    GameManager.Instance.uiController?.uiInteraction.SetText(currentInteractable.interactionMessage); 
                }
                return;
            }
        }

        if (currentInteractable != null)
        {
            currentInteractable = null;
            GameManager.Instance.uiController?.ToggleInteractionPrompt(false);
        }
    }

    private void UseItem(InputAction.CallbackContext context)
    {
        Debug.Log("Use Item!");
    }

    private void Interact(InputAction.CallbackContext context)
    {
        if (currentInteractable != null) 
            currentInteractable.Interact();
    }
}