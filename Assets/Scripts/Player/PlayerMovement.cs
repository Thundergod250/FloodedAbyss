using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Values")]
    public float moveSpeed = 5f;
    public float jumpHeight = 2f;
    public float gravity = -9.81f;

    private PlayerController controller;
    private CharacterController characterController;
    private Vector3 velocity;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        controller = GetComponent<PlayerController>();
    }

    private void Start()
    {
        if (controller != null && controller.JumpAction != null)
        {
            controller.JumpAction.started += Jump;
        }
    }

    private void OnDestroy()
    {
        if (controller != null && controller.JumpAction != null)
        {
            controller.JumpAction.started -= Jump;
        }
    }

    private void Update()
    {
        MovePlayer();
        ApplyGravity();
    }

    private void MovePlayer()
    {
        if (controller == null || controller.MoveAction == null) return;

        Vector2 input = controller.MoveAction.ReadValue<Vector2>();
        Vector3 move = new Vector3(input.x, 0f, input.y);

        characterController.Move(move * moveSpeed * Time.deltaTime);
    }

    private void ApplyGravity()
    {
        if (characterController.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }

    private void Jump(InputAction.CallbackContext ctx)
    {
        if (characterController.isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            Debug.Log("Jump");
        }
    }
}