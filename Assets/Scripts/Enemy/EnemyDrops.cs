using UnityEngine;

public class EnemyDrops : MonoBehaviour
{
    [SerializeField] private Health enemyHealth;

    [Header("Drops")]
    [SerializeField] private GameObject itemToDrop;

    private void Awake()
    {
        if (enemyHealth == null) enemyHealth = GetComponent<Health>();
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
