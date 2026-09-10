using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLook : MonoBehaviour
{
    [SerializeField] PlayerController controller;
    [SerializeField] bool invertY = false;
    [SerializeField] float maxPitch = 89f;
    [SerializeField] float lookSensitivity = 1f;
    [SerializeField] Transform vCam;

    InputAction lookAction;
    float pitch = 0;
    private void Awake()
    {
        if (controller == null) controller = GetComponent<PlayerController>();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; 
        Cursor.visible = false;
    }

    void Update()
    {
        ProcessLook();
    }

    private void ProcessLook()
    {
        Vector2 lookInput = controller.LookAction.ReadValue<Vector2>();

        // Up / Down
        pitch += lookInput.y * lookSensitivity * (invertY ? -1f : 1f);
        pitch = Mathf.Clamp(pitch, -maxPitch, maxPitch);

        vCam.localRotation = Quaternion.Euler(pitch, 0f, 0f);

        // Left / Right
        transform.Rotate(
            0f,
            lookInput.x * lookSensitivity,
            0f
        );
    }
}
