using UnityEngine;
using UnityEngine.InputSystem;

public class BasePickAxe : Equipment
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

    private void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (attackBox != null)
            attackBox.Damage = damage;
        
        if (animator != null)
            animator.SetFloat(AttackSpeedHash, attackSpeed);

        // Bind in Start() to ensure PlayerController.Awake() has already run
        RegisterInput();
    }

    private void OnEnable()
    {
        // On re-enabling the tool, register if controller is ready
        RegisterInput();
    }

    private void OnDisable()
    {
        UnregisterInput();
    }

    private void OnDestroy()
    {
        UnregisterInput();
    }

    private void RegisterInput()
    {
        if (controller != null && controller.AttackAction != null)
        {
            controller.AttackAction.started -= OnAttack; // Avoid duplicate binding
            controller.AttackAction.started += OnAttack;
        }
    }

    private void UnregisterInput()
    {
        if (controller != null && controller.AttackAction != null)
        {
            controller.AttackAction.started -= OnAttack;
        }
    }

    private void OnAttack(InputAction.CallbackContext context)
    {
        // Block attack if player controls are disabled (e.g., UI modal is open)
        if (controller != null && !controller.IsInputActive)
            return;

        if (!canAttack)
            return;

        canAttack = false;

        if (animator != null)
        {
            animator.SetTrigger(AttackHash);
            animator.SetFloat(AttackSpeedHash, attackSpeed);
        }
    }

    public void EnableAttack() => canAttack = true;

    public void UpgradeDamage(int amount)
    {
        damage += amount;
        if (attackBox != null)
            attackBox.Damage = damage;
    }

    public void UpgradeAttackSpeed(float amount)
    {
        attackSpeed += amount;
        if (animator != null)
            animator.SetFloat(AttackSpeedHash, attackSpeed);
    }
    
    public void SetDamage(int newDamage)
    {
        damage = newDamage;
        if (attackBox != null)
            attackBox.Damage = damage;
    }

    public void SetAttackSpeed(float newSpeed)
    {
        attackSpeed = newSpeed;
        if (animator != null)
            animator.SetFloat(AttackSpeedHash, attackSpeed);
    }
}