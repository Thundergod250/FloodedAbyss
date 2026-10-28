using UnityEngine;

public class TemporaryBullet : MonoBehaviour
{
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private int damage = 10;

    private PlayerResources playerResources;

    public void SetPlayerResources(PlayerResources resourceScript)
    {
        playerResources = resourceScript;
    }

    public void SetDamage(int damageValue)
    {
        damage = damageValue;
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<Health>(out Health goHealth))
        {
            goHealth.TakeDamage(damage);
        }

        if (collision.gameObject.TryGetComponent<EnemyDrops>(out EnemyDrops drops))
        {
            drops.GainPlayerReference(playerResources);
        }

        Debug.Log($"Hit: {collision.gameObject.name} for {damage} damage");

        Destroy(gameObject);
    }
}