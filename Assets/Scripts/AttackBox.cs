using UnityEngine;

public class AttackBox : MonoBehaviour
{
    [SerializeField] private int damage = 10;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerController>(out _))
            return;

        if (other.TryGetComponent(out Health health))
        {
            health.TakeDamage(damage);
        }
    }
}