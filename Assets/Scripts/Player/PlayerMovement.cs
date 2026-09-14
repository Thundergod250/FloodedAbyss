using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Values")]
    public float moveSpeed = 5f;
    public float jumpHeight = 2f;
    public float gravity = -9.81f; 
    public float swimUpSpeed = 5f;

    [Header("Swim")]
    [SerializeField] private float floatDepth;
    [SerializeField] private float floatForce;
    [SerializeField] private float surfacingSpeed;
    [SerializeField] private float waterGravity = -3f;
    [SerializeField] private float maxSinkSpeed = 3f;
    public bool isSwimming;

    private Transform waterSurface;

    private PlayerController controller;
    private CharacterController characterController;
    private Rigidbody rb;
    private Vector3 velocity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
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

        if (isSwimming)
        {
            ApplySwimming();
        }
        else
        {
            ApplyGravity();
        }
    }

    private void MovePlayer()
    {
        if (controller == null || controller.MoveAction == null) return;

        Vector2 input = controller.MoveAction.ReadValue<Vector2>();
        Vector3 move = transform.right * input.x + transform.forward * input.y;

        characterController.Move(move * (moveSpeed * Time.deltaTime));
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

    private void ApplySwimming()
    {
        if (waterSurface == null) return;

        float floatingY = waterSurface.position.y - floatDepth;

        if (controller.JumpAction != null && controller.JumpAction.IsPressed())
        {
            // Hold Space to swim upward
            velocity.y = swimUpSpeed;
        }
        else if (transform.position.y > floatingY)
        {
            // Reduced gravity underwater
            velocity.y += waterGravity * Time.deltaTime;

            // Prevent sinking too quickly
            velocity.y = Mathf.Max(velocity.y, -maxSinkSpeed);
        }
        else
        {
            // Buoyancy
            velocity.y = Mathf.MoveTowards(
                velocity.y,
                surfacingSpeed,
                floatForce * Time.deltaTime
            );
        }

        characterController.Move(velocity * Time.deltaTime);
    }

    public void SetWaterSurface(Transform surface)
    {
        waterSurface = surface;
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