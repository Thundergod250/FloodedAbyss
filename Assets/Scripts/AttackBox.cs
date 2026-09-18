using UnityEngine;

public class AttackBox : MonoBehaviour
{
    public int Damage = 10;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerController>(out _))
            return;

        if (other.TryGetComponent(out Health health))
        {
            health.TakeDamage(Damage);
        }
    }
}