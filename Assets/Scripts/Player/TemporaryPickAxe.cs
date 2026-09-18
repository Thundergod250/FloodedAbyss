using UnityEngine;
using UnityEngine.InputSystem;

public class TemporaryPickAxe : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private PlayerController controller;

    private static readonly int AttackHash = Animator.StringToHash("Attack");

    private void Awake()
    {
        if (controller == null)
            controller = GetComponent<PlayerController>();

        if (animator == null)
            animator = GetComponent<Animator>();
    }

    private void Start()
    {
        if (controller != null)
            controller.AttackAction.started += OnAttack;
    }

    private void OnDestroy()
    {
        if (controller != null)
            controller.AttackAction.started -= OnAttack;
    }

    private void OnAttack(InputAction.CallbackContext context)
    {
        animator.SetTrigger(AttackHash);
    }
}