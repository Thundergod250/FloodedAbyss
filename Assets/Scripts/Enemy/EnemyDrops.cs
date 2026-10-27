using UnityEngine;

public class EnemyDrops : MonoBehaviour
{
    private Health enemyHealth;

    [Header("Drops")]
    [SerializeField] private ResourceType resourceType;
    [SerializeField] private int amountToDropOnDeath;
    [SerializeField] private int amountToDropOnHit;
    [SerializeField] private GameObject itemToDrop;

    private PlayerResources playerResources;

    private void Awake()
    {
        if (enemyHealth == null) enemyHealth = GetComponent<Health>();

    }

    private void Start()
    {
        playerResources = GameManager.Instance.playerController.GetComponent<PlayerResources>();

    }

    private void OnEnable()
    {
        // enemyHealth.EvtOnDied.AddListener(HandleDeath);
        enemyHealth.EvtOnHit.AddListener(HandleHit);
    }

    private void OnDisable()
    {
        //enemyHealth.EvtOnDied.RemoveListener(HandleDeath);
        enemyHealth.EvtOnHit.RemoveListener(HandleHit);
    }

    private void HandleDeath()
    {
        if (playerResources != null)
        {
            playerResources.AddResource(resourceType, amountToDropOnDeath);
        }
    }

    private void HandleHit()
    {
        if (playerResources != null)
        {
            playerResources.AddResource(resourceType, amountToDropOnHit);
        }
    }

    public void GainPlayerReference(PlayerResources resourceScript)
    {
        playerResources = resourceScript;
    }
}