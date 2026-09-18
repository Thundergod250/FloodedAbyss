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
            int currentHp = health.CurrentHealth; 
            int maxHp = health.MaxHealth;         

            Debug.Log($"Inflicted {Damage} Damage to ({currentHp}/{maxHp}) {other.gameObject.name}");

            health.TakeDamage(Damage);
        }
    }
}