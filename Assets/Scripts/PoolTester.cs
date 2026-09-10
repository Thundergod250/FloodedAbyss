using UnityEngine;
using System.Collections;

public class PoolTester : MonoBehaviour
{
    [Header("Prefabs to test")]
    public GameObject[] prefabs;

    [Header("Spawn Settings")]
    public int spawnCount = 10; // how many to spawn at start
    public Vector3 spawnArea = new(10, 0, 10); // area to randomize positions

    private void Start()
    {
        StartCoroutine(TestRoutine());
    }

    private IEnumerator TestRoutine()
    {
        for (int i = 0; i < spawnCount; i++)
        {
            // Pick random prefab
            GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];

            // Random position inside area
            Vector3 pos = new Vector3(
                Random.Range(-spawnArea.x, spawnArea.x),
                spawnArea.y,
                Random.Range(-spawnArea.z, spawnArea.z)
            );

            // Spawn via Pool
            GameObject obj = Pool.Instantiate(prefab, pos, Quaternion.identity);

            // Random despawn delay (1–5 seconds)
            float delay = Random.Range(1f, 5f);
            Pool.Destroy(obj, delay);

            yield return new WaitForSeconds(0.5f); // stagger spawns
        }
    }
}