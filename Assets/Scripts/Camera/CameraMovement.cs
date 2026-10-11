using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float wasdSpeed = 25f;
    [SerializeField] private float dragSpeed = 0.5f;

    private Transform cameraTransform;

    private void Start()
    {
        if (Camera.main != null)
            cameraTransform = Camera.main.transform;
        else
            cameraTransform = transform;
    }

    private void Update()
    {
        HandleWASDMovement();
        HandleMouseDragMovement();
    }

    private void HandleWASDMovement()
    {
        if (GameManager.Instance == null || GameManager.Instance.InputManager == null) return;

        Vector2 inputVal = GameManager.Instance.InputManager.MoveInput;
        if (inputVal == Vector2.zero) return;

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDir = (forward * inputVal.y + right * inputVal.x).normalized;
        transform.position += moveDir * (wasdSpeed * Time.deltaTime);
    }

    private void HandleMouseDragMovement()
    {
        if (GameManager.Instance == null || GameManager.Instance.InputManager == null) return;

        // Check if our PanCamera action is currently held down
        if (GameManager.Instance.InputManager.IsPanPressed)
        {
            // Get mouse movement delta from the new Input System
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();

            if (mouseDelta != Vector2.zero)
            {
                // Invert the delta so the map drags with the cursor instead of pushing away
                Vector3 moveDir = new Vector3(-mouseDelta.x, 0f, -mouseDelta.y);

                // Convert relative to camera rotation if desired, or keep it screen-space relative
                Vector3 right = cameraTransform.right;
                Vector3 forward = cameraTransform.forward;
                right.y = 0f;
                forward.y = 0f;
                right.Normalize();
                forward.Normalize();

                Vector3 worldMove = (right * moveDir.x + forward * moveDir.z) * (dragSpeed * Time.deltaTime);
                transform.position += worldMove;
            }
        }
    }
}