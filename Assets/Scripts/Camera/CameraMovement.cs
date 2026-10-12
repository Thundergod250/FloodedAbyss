using UnityEngine;
using Unity.Cinemachine; // Cinemachine 3.x namespace

public class CameraMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float wasdSpeed = 25f;
    [SerializeField] private float dragSpeed = 0.5f;

    [Header("Map Boundaries (Relative to 0,0,0)")]
    [SerializeField] private float minX = -50f;
    [SerializeField] private float maxX = 50f;
    [SerializeField] private float minZ = -50f;
    [SerializeField] private float maxZ = 50f;

    [Header("Zoom Settings")]
    [SerializeField] private CinemachineFollow cinemachineFollow; // Reference to Cinemachine 3.x Follow component
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float minZoomY = 10f;
    [SerializeField] private float maxZoomY = 50f;
    [SerializeField] private float zoomDamping = 10f;

    private Transform cameraTransform;
    private float targetZoomY;

    private void Start()
    {
        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
        else
        {
            cameraTransform = transform;
        }

        // Initialize target zoom from current Cinemachine Follow offset
        if (cinemachineFollow != null)
        {
            targetZoomY = cinemachineFollow.FollowOffset.y;
        }
    }

    private void Update()
    {
        HandleWASDMovement();
        HandleMouseDragMovement();
        HandleZoom();
        ClampPosition();
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
        transform.position += moveDir * (wasdSpeed * Time.unscaledDeltaTime);
    }

    private void HandleMouseDragMovement()
    {
        if (GameManager.Instance == null || GameManager.Instance.InputManager == null) return;

        if (GameManager.Instance.InputManager.IsPanPressed)
        {
            Vector2 mouseDelta = UnityEngine.InputSystem.Mouse.current.delta.ReadValue();

            if (mouseDelta != Vector2.zero)
            {
                Vector3 moveDir = new Vector3(-mouseDelta.x, 0f, -mouseDelta.y);

                Vector3 right = cameraTransform.right;
                Vector3 forward = cameraTransform.forward;
                right.y = 0f;
                forward.y = 0f;
                right.Normalize();
                forward.Normalize();

                Vector3 worldMove = (right * moveDir.x + forward * moveDir.z) * (dragSpeed * Time.unscaledDeltaTime);
                transform.position += worldMove;
            }
        }
    }

    private void HandleZoom()
    {
        if (GameManager.Instance == null || GameManager.Instance.InputManager == null || cinemachineFollow == null) return;

        Vector2 scrollInput = GameManager.Instance.InputManager.ScrollInput;

        if (scrollInput.y != 0)
        {
            // Calculate new target Y offset based on scroll wheel direction
            targetZoomY -= Mathf.Sign(scrollInput.y) * zoomSpeed;
            targetZoomY = Mathf.Clamp(targetZoomY, minZoomY, maxZoomY);
        }

        // Smoothly interpolate current Follow Offset Y using unscaledDeltaTime
        Vector3 currentOffset = cinemachineFollow.FollowOffset;
        currentOffset.y = Mathf.Lerp(currentOffset.y, targetZoomY, Time.unscaledDeltaTime * zoomDamping);
        
        cinemachineFollow.FollowOffset = currentOffset;
    }

    private void ClampPosition()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.z = Mathf.Clamp(pos.z, minZ, maxZ);
        transform.position = pos;
    }
}