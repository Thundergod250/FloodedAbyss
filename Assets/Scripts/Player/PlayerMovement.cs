using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpHeight = 2f;
    public float gravity = -9.81f; 
    public float runSpeed = 8f;

    [Header("Movement-Water")]
    public float underwaterMoveSpeed;
    public float swimUpSpeed = 5f;
    public float surfacingSpeed; 
    public float sinkSpeed = 3f;
    public bool isSwimming;
    public bool wasSwimSprinting;

    private float currentSpeed;
    private WaterLevel wLevel;
    private Transform waterSurface;
    private PlayerController controller;
    private CharacterController characterController;
    private PlayerStamina stamina;
    private PlayerOxygen oxygen;
    private Vector3 velocity; 
    private bool wasRunning;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        controller = GetComponent<PlayerController>();
        stamina = GetComponent<PlayerStamina>();
        oxygen = GetComponent<PlayerOxygen>();
    }

    private void Start()
    {
        wLevel = GameManager.Instance.GetComponent<WaterLevel>();

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

        Vector2 input = controller.MoveAction.ReadValue<Vector2>();
        Vector3 move = transform.right * input.x + transform.forward * input.y;

        if (isSwimming)
        {
            currentSpeed = underwaterMoveSpeed;
            stamina.DrainSwimming();
        }
        else
        {
            bool running = controller.RunAction.IsPressed();

            if (running && !wasRunning)
            {
                if (!stamina.StartRunning())
                {
                    running = false;
                }
            }

            if (running && !stamina.IsExhausted)
            {
                currentSpeed = runSpeed;
                stamina.DrainRunning();
            }
            else
            {
                currentSpeed = moveSpeed;
            }

            wasRunning = running;
        }

        if (stamina.IsExhausted)
        {
            currentSpeed *= stamina.ExhaustionSpeedMultiplier;
        }

        characterController.Move(move * (currentSpeed * Time.deltaTime));
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

        float floatArea = waterSurface.position.y - wLevel.floatDepth;
        wLevel.minimumFloatDepth = waterSurface.transform.position.y - (wLevel.floatDepth + 0.5f);

        if (controller.RunAction != null && controller.RunAction.IsPressed())
        {
            velocity.y = -swimUpSpeed;
        }
        else if (controller.JumpAction != null && controller.JumpAction.IsPressed())
        {
            velocity.y = swimUpSpeed;
        }
        else if (transform.position.y < wLevel.minimumFloatDepth)
        {
            velocity.y = -sinkSpeed;
        }
        else if (transform.position.y > floatArea)
        {
            velocity.y += wLevel.waterGravity * Time.deltaTime;
            velocity.y = Mathf.Max(velocity.y, -sinkSpeed);
        }
        else if (transform.position.y < floatArea)
        {
            velocity.y = surfacingSpeed;
        }
        else
        {
            velocity.y = Mathf.MoveTowards(
                velocity.y,
                0f,
                wLevel.floatForce * Time.deltaTime
            );
        }

        bool swimSprinting = controller.RunAction.IsPressed();

        if (swimSprinting && !wasSwimSprinting)
        {
            if (!oxygen.StartSwimSprint())
            {
                swimSprinting = false;
            }
        }

        if (swimSprinting && oxygen.CurrentOxygen > 0f)
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

    public void SetWaterSurface(Transform surface)
    {
        waterSurface = surface;
    }

    private void Jump(InputAction.CallbackContext ctx)
    {
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