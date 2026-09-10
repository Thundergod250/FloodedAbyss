using UnityEngine;
using UnityEngine.InputSystem;

public class TemporaryAttack : MonoBehaviour
{
    private PlayerController controller;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;

    private void Awake()
    {
        if (controller == null)
            controller = GetComponent<PlayerController>();
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

    private void Shoot(InputAction.CallbackContext context)
    {
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        rb.AddForce(firePoint.forward * 10f, ForceMode.Impulse);
    }
}
