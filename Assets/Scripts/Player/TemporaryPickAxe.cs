using UnityEngine;
using UnityEngine.InputSystem;

public class TemporaryPickAxe : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private AttackBox attackBox;
    [SerializeField] private PlayerController controller;

    [Header("Stats")]
    [SerializeField] private int damage = 10;
    [SerializeField] private float attackSpeed = 1f;

    private bool canAttack = true;

    private static readonly int AttackHash =
        Animator.StringToHash("Attack");

    private static readonly int AttackSpeedHash =
        Animator.StringToHash("AttackSpeed");

    private void Awake()
    {
        //controller = GetComponent<PlayerController>();

        if (animator == null)
            animator = GetComponent<Animator>();

        if (attackBox != null)
            attackBox.Damage = damage;
    }

    private void Start()
    {
        animator.SetFloat(AttackSpeedHash, attackSpeed);

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
        if (!canAttack)
            return;

        canAttack = false;

        animator.SetTrigger(AttackHash);
        animator.SetFloat(AttackSpeedHash, attackSpeed);
    }

    public void EnableAttack()
    {
        canAttack = true;
    }

    public void UpgradeDamage(int amount)
    {
        damage += amount;

        if (attackBox != null)
            attackBox.Damage = damage;
    }

    public void UpgradeAttackSpeed(float amount)
    {
        attackSpeed += amount;

        animator.SetFloat(AttackSpeedHash, attackSpeed);
    }
}