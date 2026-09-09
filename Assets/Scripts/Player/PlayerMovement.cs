using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Input Stuff")]
    [SerializeField] private PlayerInput input;
    [SerializeField] private InputAction moveAction;
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
        jumpAction.started += Jump;
    }

    private void OnDisable()
    {
        jumpAction.started -= Jump;
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
