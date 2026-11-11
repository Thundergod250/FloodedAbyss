using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingResourceSpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    [SerializeField] private List<GameObject> floatingResourcePrefabs = new();
    [SerializeField] private float spawnIntervalMin = 2f;
    [SerializeField] private float spawnIntervalMax = 5f;

    [Header("Parenting")]
    [SerializeField] private Transform spawnedItemsParent; // Assign your dedicated container GameObject here

    [Header("Movement & Lifetime")]
    [SerializeField] private float minSpeed = 2f;
    [SerializeField] private float maxSpeed = 5f;
    [SerializeField] private float itemLifetime = 20f; // Despawns after this many seconds if uncollected

    private BoxCollider spawnerCollider;
    private Coroutine spawnRoutine;

    private void Awake()
    {
        spawnerCollider = GetComponent<BoxCollider>();
    }

    private void OnEnable()
    {
        // Restart the spawn loop every time this object becomes active.
        // (Start() only ever runs once, so a SetActive(false) → SetActive(true)
        // cycle would permanently kill the coroutine started there.)
        if (spawnRoutine != null) StopCoroutine(spawnRoutine);
        spawnRoutine = StartCoroutine(SpawnRoutine());
    }

    private void OnDisable()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            // Stop spawning if water level is 0 or lower.
            if (GameManager.Instance == null ||
                GameManager.Instance.WaterLevel == null ||
                GameManager.Instance.WaterLevel.CurrentWaterHeight < 1f)
            {
                yield return new WaitForSeconds(0.5f);
                continue;
            }

            float waitTime = Random.Range(spawnIntervalMin, spawnIntervalMax);
            yield return new WaitForSeconds(waitTime);

            // Check again before spawning in case water reached 0 while waiting.
            if (GameManager.Instance != null &&
                GameManager.Instance.WaterLevel != null &&
                GameManager.Instance.WaterLevel.CurrentWaterHeight >= 1f)
            {
                SpawnResource();
            }
        }
    }

    private void SpawnResource()
    {
        if (floatingResourcePrefabs.Count == 0) return;

        GameObject randomPrefab = floatingResourcePrefabs[Random.Range(0, floatingResourcePrefabs.Count)];
        if (randomPrefab == null) return;

        Vector3 spawnPos = transform.position;
        if (spawnerCollider != null)
        {
            Vector3 size = spawnerCollider.size;
            float randomX = Random.Range(-size.x / 2f, size.x / 2f);
            float randomY = Random.Range(-size.y / 2f, size.y / 2f);
            float randomZ = Random.Range(-size.z / 2f, size.z / 2f);

            spawnPos = spawnerCollider.transform.TransformPoint(new Vector3(randomX, randomY, randomZ));
        }

        // Instantiate under the designated parent transform (if assigned), otherwise spawn loose
        GameObject resourceGO = Instantiate(randomPrefab, spawnPos, Quaternion.identity, spawnedItemsParent);

        FloatingItem floatingItem = resourceGO.GetComponent<FloatingItem>();
        if (floatingItem != null)
        {
            float speed = Random.Range(minSpeed, maxSpeed);
            Vector3 moveDirection = transform.right;
            floatingItem.Initialize(moveDirection, speed, itemLifetime);
        }
    }
}