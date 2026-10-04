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
    public bool isClimbing;

    [Header("Movement-Water")]
    public float underwaterMoveSpeed;
    public float swimUpSpeed = 5f;
    public float sinkSpeed = 3f;
    public bool isSwimming;
    public bool wasSwimSprinting;
    public float surfaceCheckDistance = 0.5f;
    public float surfaceJumpHeight = 2f;

    private bool surfaceJumping;
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

        if (isClimbing)
        {
            velocity.y = 0f;
        }
        else if (isSwimming)
        {
            ApplySwimming();
            stamina.DrainSwimming();
            oxygen.DrainSwimSprint();
        }
        else
        {
            ApplyGravity();
        }
    }

    private void MovePlayer()
    {
        if (controller == null || controller.MoveAction == null) return;

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

        // Exclude vertical velocity.y while climbing
        Vector3 finalMove =
            move * currentSpeed +
            (isClimbing ? Vector3.zero : Vector3.up * velocity.y);

        characterController.Move(finalMove * Time.deltaTime);
    }

    private void ApplyGravity()
    {
        if (characterController.isGrounded)
        {
            velocity.y = -2f;
           // surfaceJumping = false;
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
            velocity.y = Mathf.Max(velocity.y, maxFallSpeed);
        }
    }

    #region Swimming-Related
    private void ApplySwimming()
    {
        if (waterSurface == null) return;

        bool jumpHeld =
            controller.IsInputActive &&
            controller.JumpAction != null &&
            controller.JumpAction.IsPressed();

        if (jumpHeld)
            velocity.y = swimUpSpeed;
        else
            velocity.y = -sinkSpeed;

        bool swimSprinting =
            controller.IsInputActive &&
            controller.RunAction.IsPressed();

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
        {
            currentSpeed = underwaterMoveSpeed;
        }

        wasSwimSprinting = swimSprinting;

        characterController.Move(velocity * Time.deltaTime);
    }

    public void SetWaterSurface(Transform surface) => waterSurface = surface;

    private bool IsAtWaterSurface()
    {
        if (waterSurface == null)
            return false;

        float waterY = waterSurface.position.y;
        float playerY = transform.position.y;

        Debug.Log($"Player Y: {playerY} | Water Y: {waterY}");

        return playerY >= waterY - surfaceCheckDistance;
    }
    #endregion

    private void Jump(InputAction.CallbackContext ctx)
    {
        if (controller != null && !controller.IsInputActive)
            return;

        if (isClimbing)
            return;

        if (isSwimming && characterController.isGrounded)     // Don't allow jumping when underwater and touching the floor
         {
             return;
         }


        // Normal ground jump
        if (characterController.isGrounded)
        {
            if (stamina.UseJump())
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                Debug.Log("Ground Jump");
            }

            return;
        }


        /*if (isSwimming && IsAtWaterSurface()) // Jump out of water
        {
            if (stamina.UseJump())
            {
                velocity.y = Mathf.Sqrt(surfaceJumpHeight * -2f * gravity);

                isSwimming = false;
                wasSwimSprinting = false;

                Debug.Log("Surface Jump!");
            }
        }*/
    }
}