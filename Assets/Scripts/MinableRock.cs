using UnityEngine;

[RequireComponent(typeof(Health))]
public class MinableRock : MonoBehaviour
{
    [Header("Drop Settings")]
    [SerializeField] private ResourceType resourceType;
    [SerializeField] private int amountToDropOnHit = 1;
    [SerializeField] private int amountToDropOnDeath = 5;

    [Header("Physical Drops (Optional)")]
    [SerializeField] private GameObject orePrefab;
    [SerializeField] private int oreDropCountOnDeath = 3;
    [SerializeField] private float dropScatterForce = 3f;

    private Health rockHealth;
    private PlayerResources playerResources;

    private void Awake()
    {
        rockHealth = GetComponent<Health>();
    }

    private void Start()
    {
        if (GameManager.Instance != null && GameManager.Instance.playerController != null)
        {
            playerResources = GameManager.Instance.playerController.GetComponent<PlayerResources>();
        }
    }

    private void OnEnable()
    {
        if (rockHealth != null)
        {
            rockHealth.EvtOnHit.AddListener(HandleHit);
            rockHealth.EvtOnDied.AddListener(HandleDeath);
        }
    }

    private void OnDisable()
    {
        if (rockHealth != null)
        {
            rockHealth.EvtOnHit.RemoveListener(HandleHit);
            rockHealth.EvtOnDied.RemoveListener(HandleDeath);
        }
    }

    /// <summary>
    /// Call this from mining tools or picks to damage the rock.
    /// </summary>
    public void TakeMineDamage(int damage)
    {
        if (rockHealth != null)
        {
            rockHealth.TakeDamage(damage);
        }
    }

    private void HandleHit()
    {
        if (playerResources != null && amountToDropOnHit > 0)
        {
            playerResources.AddResource(resourceType, amountToDropOnHit);
        }
    }

    private void HandleDeath()
    {
        if (playerResources != null && amountToDropOnDeath > 0)
        {
            playerResources.AddResource(resourceType, amountToDropOnDeath);
        }

        if (orePrefab != null)
        {
            SpawnPhysicalOres();
        }

        gameObject.SetActive(false);
    }

    private void SpawnPhysicalOres()
    {
        for (int i = 0; i < oreDropCountOnDeath; i++)
        {
            Vector3 spawnPos = transform.position + Vector3.up * 0.5f;
            GameObject ore = Pool.Instantiate(orePrefab, spawnPos, Quaternion.identity);

            if (ore.TryGetComponent<Rigidbody>(out Rigidbody rb))
            {
                Vector3 randomDir = Random.insideUnitSphere + Vector3.up;
                rb.AddForce(randomDir * dropScatterForce, ForceMode.Impulse);
            }
        }
    }
}