using UnityEngine;
using UnityEngine.InputSystem;

public class TemporaryAttack : MonoBehaviour
{
    private PlayerController controller;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;

    [Header("Damage Stats")]
    [SerializeField] private int baseDamage = 10;
    public int CurrentDamage { get; private set; }

    private void Awake()
    {
        if (controller == null)
            controller = GetComponent<PlayerController>();

        CurrentDamage = baseDamage;
    }

    private void Start()
    {
        if (controller != null)
        {
            controller.AttackAction.started += Shoot;
        }
    }

    private void OnDestroy()
    {
        if (controller != null)
        {
            controller.AttackAction.started -= Shoot;
        }
    }

    public void AddDamage(int amount)
    {
        CurrentDamage += amount;
        Debug.Log($"Damage upgraded! New Damage: {CurrentDamage}");
    }

    private void Shoot(InputAction.CallbackContext context)
    {
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

        Rigidbody rb = projectile.GetComponent<Rigidbody>();

        TemporaryBullet bulletScript = projectile.GetComponent<TemporaryBullet>();
        if (bulletScript != null)
        {
            bulletScript.SetPlayerResources(GetComponent<PlayerResources>());
            bulletScript.SetDamage(CurrentDamage);
        }

        rb.AddForce(firePoint.forward * 10f, ForceMode.Impulse);
    }
}