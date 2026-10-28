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

    private void Awake()
    {
        spawnerCollider = GetComponent<BoxCollider>();
    }

    private void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            float waitTime = Random.Range(spawnIntervalMin, spawnIntervalMax);
            yield return new WaitForSeconds(waitTime);

            SpawnResource();
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