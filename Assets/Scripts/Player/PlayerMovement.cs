using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpHeight = 2f;
    public float gravity = -9.81f; 
    public float maxFallSpeed = -9.81f; 
    public float runSpeed = 8f;

    [Header("Movement-Water")]
    public float underwaterMoveSpeed;
    public float swimUpSpeed = 5f;
    public float sinkSpeed = 3f;
    public bool isSwimming;
    public bool wasSwimSprinting;

    private float currentSpeed;
    private Transform waterSurface;
    private PlayerController controller;
    private CharacterController characterController;
    private PlayerStamina stamina;
    private PlayerOxygen oxygen;
    private Vector3 velocity; 

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        controller = GetComponent<PlayerController>();
        stamina = GetComponent<PlayerStamina>();
        oxygen = GetComponent<PlayerOxygen>();
        
        velocity.y = -2f;
        if (controller != null && controller.JumpAction != null) 
            controller.JumpAction.started += Jump;
    }

    private void OnDestroy()
    {
        if (controller != null && controller.JumpAction != null) 
            controller.JumpAction.started -= Jump;
    }

    private void Update()
    {
        MovePlayer();   
        if (isSwimming)
        {
            ApplySwimming(); 
            stamina.DrainSwimming();
            oxygen.DrainSwimSprint();
        }
        else
            ApplyGravity();
    }

    private void MovePlayer()
    {
        if (controller == null || controller.MoveAction == null) return;

        // Ignore movement input if gameplay controls are inactive (e.g. UI modal open)
        Vector2 input = controller.IsInputActive ? controller.MoveAction.ReadValue<Vector2>() : Vector2.zero;

        Vector3 move =
            transform.right * input.x +
            transform.forward * input.y;

        if (isSwimming)
            currentSpeed = underwaterMoveSpeed;
        else
        {
            bool running = controller.IsInputActive && controller.RunAction.IsPressed();

            if (running)
            {
                currentSpeed = runSpeed;
                stamina.DrainRunning();
            }
            else
                currentSpeed = moveSpeed;
        }

        Vector3 finalMove =
            move * currentSpeed +
            Vector3.up * velocity.y;

        characterController.Move(finalMove * Time.deltaTime);
    }

    private void ApplyGravity()
    {
        if (characterController.isGrounded)
            velocity.y = -2f;
        else
        {
            velocity.y += gravity * Time.deltaTime;
            velocity.y = Mathf.Max(velocity.y, maxFallSpeed);
        }
    }

    private void ApplySwimming()
    {
        if (waterSurface == null) return;

        if (controller.IsInputActive && controller.JumpAction != null && controller.JumpAction.IsPressed())
            velocity.y = swimUpSpeed;
        else
            velocity.y = -sinkSpeed;

        bool swimSprinting = controller.IsInputActive && controller.RunAction.IsPressed();

        if (swimSprinting && !wasSwimSprinting)
        {
            if (!oxygen.StartSwimSprint()) 
                swimSprinting = false;
        }

        if (swimSprinting && oxygen.GetCurrentOxygen() > 0f)
        {
            currentSpeed = runSpeed;
            oxygen.DrainSwimSprint();
        }
        else
            currentSpeed = underwaterMoveSpeed;

        wasSwimSprinting = swimSprinting;
        characterController.Move(velocity * Time.deltaTime);
    }

    public void SetWaterSurface(Transform surface) => waterSurface = surface;

    private void Jump(InputAction.CallbackContext ctx)
    {
        // Guard against jumping when gameplay controls are disabled
        if (controller != null && !controller.IsInputActive) return;

        if (characterController.isGrounded)
        {
            if (stamina.UseJump())
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                Debug.Log("Jump");
            }
        }
    }
}