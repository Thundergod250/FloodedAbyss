using UnityEngine;
using UnityEngine.InputSystem;

public class TemporaryAttack : MonoBehaviour
{
    [SerializeField] private PlayerController controller;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;

    private void Awake()
    {
        if (controller == null)
            controller = GetComponent<PlayerController>();
    }

    private void OnEnable()
    {
        if (controller != null)
            controller.AttackAction.started += Shoot;
    }

    private void OnDisable()
    {
        if (controller != null)
            controller.AttackAction.started -= Shoot;
    }

    private void Shoot(InputAction.CallbackContext context)
    {
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position,  firePoint.rotation);

        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        rb.AddForce(firePoint.forward * 10f, ForceMode.Impulse);
    }
}
