using UnityEngine;

public class EnemyDrops : MonoBehaviour
{
    [SerializeField] private EnemyHealth enemyHealth;

    [Header("Drops")]
    [SerializeField] private GameObject itemToDrop;

    private void Awake()
    {
        if (enemyHealth == null) enemyHealth = GetComponent<EnemyHealth>();
    }

    private void OnEnable()
    {
        enemyHealth.OnDied += HandleDeath;
    }

    private void OnDisable()
    {
        enemyHealth.OnDied -= HandleDeath;
    }

    private void HandleDeath()
    {
        Instantiate(itemToDrop, transform.position, Quaternion.identity);
    }
}
