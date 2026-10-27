using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class ClickInteractor : MonoBehaviour
{
    [Header("Raycast Settings")]
    [SerializeField] private LayerMask interactableLayer = -1;
    [SerializeField] private float maxClickDistance = 1000f;
    [SerializeField] private float dragThreshold = 15f;

    private Vector2 clickStartPosition;
    private bool hasDragged;

    private void Update()
    {
        if (Mouse.current == null) return;

        // 1. Record start position on press
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            clickStartPosition = Mouse.current.position.ReadValue();
            hasDragged = false;
        }

        // 2. Check if the mouse moved far enough to be considered a drag/pan
        if (Mouse.current.leftButton.isPressed)
        {
            Vector2 currentPosition = Mouse.current.position.ReadValue();
            if (!hasDragged && Vector2.Distance(clickStartPosition, currentPosition) > dragThreshold)
            {
                hasDragged = true;
            }
        }

        // 3. On release, evaluate click
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            if (hasDragged) return;

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

            TryInteract();
        }
    }

    private void TryInteract()
    {
        if (Camera.main == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        
        if (Physics.Raycast(ray, out RaycastHit hit, maxClickDistance, interactableLayer))
        {
            Interactables interactable = hit.collider.GetComponent<Interactables>();
            if (interactable != null)
            {
                interactable.Interact();
            }
        }
    }
}