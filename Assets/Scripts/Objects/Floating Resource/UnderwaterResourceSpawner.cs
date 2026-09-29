using UnityEngine;

public class UnderwaterResourceSpawner : MonoBehaviour
{
[Header("Spawn Area Settings")]
    [Tooltip("The 3D bounds area where debris can spawn.")]
    [SerializeField] private Vector3 spawnBoundsSize = new Vector3(20f, 10f, 30f);

    [Header("Prefabs & Timing")]
    [SerializeField] private GameObject[] debrisPrefabs;
    [SerializeField] private float spawnInterval = 1.5f;

    [Header("Movement Settings")]
    [Tooltip("The direction all debris will move towards.")]
    [SerializeField] private Vector3 travelDirection = Vector3.forward;
    [SerializeField] private float minSpeed = 2f;
    [SerializeField] private float maxSpeed = 5f;

    [Header("Fail-Safe Cleanup")]
    [Tooltip("Backup lifetime (in seconds) if debris misses the cleanup collider.")]
    [SerializeField] private float backupLifetime = 30f;

    private float spawnTimer;

    private void Update()
    {
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            SpawnDebris();
        }
    }

    private void SpawnDebris()
    {
        if (debrisPrefabs == null || debrisPrefabs.Length == 0) return;

        // 1. Choose a random prefab
        GameObject selectedPrefab = debrisPrefabs[Random.Range(0, debrisPrefabs.Length)];

        // 2. Get a random position inside the 3D bounding box
        Vector3 randomLocalPoint = new Vector3(
            Random.Range(-spawnBoundsSize.x / 2f, spawnBoundsSize.x / 2f),
            Random.Range(-spawnBoundsSize.y / 2f, spawnBoundsSize.y / 2f),
            Random.Range(-spawnBoundsSize.z / 2f, spawnBoundsSize.z / 2f)
        );

        Vector3 spawnPosition = transform.TransformPoint(randomLocalPoint);
        Quaternion randomRotation = Random.rotation;

        // 3. Instantiate object
        GameObject spawnedItem = Instantiate(selectedPrefab, spawnPosition, randomRotation);

        // 4. Set up movement parameters
        UnderwaterResource debrisComponent = spawnedItem.GetComponent<UnderwaterResource>();
        if (debrisComponent == null)
        {
            debrisComponent = spawnedItem.AddComponent<UnderwaterResource>();
        }

        float randomSpeed = Random.Range(minSpeed, maxSpeed);
        debrisComponent.Initialize(travelDirection, randomSpeed, backupLifetime);
    }

    // Visualize the spawn box in the Unity Scene View
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 0.7f, 1f, 0.4f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(Vector3.zero, spawnBoundsSize);
        Gizmos.DrawWireCube(Vector3.zero, spawnBoundsSize);

        // Draw an arrow pointing in the travel direction
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(Vector3.zero, travelDirection.normalized * 5f);
    }
}
