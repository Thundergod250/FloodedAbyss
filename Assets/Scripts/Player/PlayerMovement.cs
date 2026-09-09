using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Input Stuff")]
    [SerializeField] private PlayerInput input;
    [SerializeField] private InputAction moveAction;
    [SerializeField] private InputAction useAction;
    [SerializeField] private InputAction interactAction;
    [SerializeField] private InputAction jumpAction;

    [Header("References")]
    [SerializeField] private Rigidbody rb;

    [Header("Values")]
    public float moveSpeed;
    public float jumpForce;

    private void Awake()
    {
        input = GetComponent<PlayerInput>();
        moveAction = input.actions.FindAction("Move");
        useAction = input.actions.FindAction("Use");
        interactAction = input.actions.FindAction("Interact");
        jumpAction = input.actions.FindAction("Jump");
    }
    private void FixedUpdate()
    {
        MovePlayer();
    }

    public void MovePlayer()
    {
        Vector2 direction = moveAction.ReadValue<Vector2>();

        Vector3 movement = new Vector3(direction.x, 0f, direction.y);

        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }

    private void OnEnable()
    {
        useAction.started += UseItem;
        interactAction.started += Interact;
        jumpAction.started += Jump;
    }

    private void OnDisable()
    {
        useAction.started -= UseItem;
        interactAction.started -= Interact;
        jumpAction.started -= Jump;
    }

    private void UseItem(InputAction.CallbackContext context)
    {
        Debug.Log("Use Item!"); // Put condition if something is held 
    }

    private void Interact(InputAction.CallbackContext context)
    {
        Debug.Log("Interacted with item"); // Put condition if looking at item. Raycast?
    }

    private void Jump(InputAction.CallbackContext context)
    {
        if (gameObject.transform.position.y < 100f)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            Debug.Log("Jump");
        }
    }
}
