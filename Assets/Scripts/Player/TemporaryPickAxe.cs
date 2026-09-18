using UnityEngine;
using UnityEngine.InputSystem;

public class TemporaryPickAxe : MonoBehaviour
{
    [SerializeField] private Animator animator;

    [SerializeField] private string attackStateName = "Attack";
    private PlayerController controller;

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
        animator.Play(attackStateName, 0, 0f);
    }
}