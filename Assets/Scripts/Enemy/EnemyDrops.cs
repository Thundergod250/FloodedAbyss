using UnityEngine;

public class EnemyDrops : MonoBehaviour
{
    private Health enemyHealth;

    [Header("Drops")]
    [SerializeField] private ResourceType resourceType;
    [SerializeField] private int amountToDrop;
    [SerializeField] private GameObject itemToDrop;

    private PlayerResources playerResources;

    private void Awake()
    {
        if (enemyHealth == null) enemyHealth = GetComponent<Health>();
    }

    private void OnEnable()
    {
        enemyHealth.EvtOnDied.AddListener(HandleDeath);
    }

    private void OnDisable()
    {
        enemyHealth.EvtOnDied.RemoveListener(HandleDeath);
    }

    private void HandleDeath()
    {
        if (playerResources != null)
        {
            playerResources.AddResource(resourceType, amountToDrop);
        }
    }

    public void GainPlayerReference(PlayerResources resourceScript)
    {
        playerResources = resourceScript;
    }
}