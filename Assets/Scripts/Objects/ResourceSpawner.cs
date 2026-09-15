using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class ResourceSpawner : MonoBehaviour
{
    [Header("Spawner")]
    [SerializeField] private List<GameObject> objectsToSpawn = new List<GameObject>();
    [SerializeField] private float timerToSpawn;
    [SerializeField] private WaterSurface targetSurface; 
    [SerializeField] private BoxCollider spawnArea;

    public bool isSpawning;
    private Vector3 GetRandomPointInBox()
    {
        Vector3 center = spawnArea.bounds.center;
        Vector3 size = spawnArea.bounds.size;

        float x = Random.Range(center.x - size.x / 2f, center.x + size.x / 2f);

        float y = Random.Range(center.y - size.y / 2f, center.y + size.y / 2f);

        float z = Random.Range(center.z - size.z / 2f, center.z + size.z / 2f);

        return new Vector3(x, y, z);
    }

    void Start()
    {
        targetSurface = GameManager.Instance.GetComponent<WaterLevel>().waterLevelTransform.gameObject.GetComponent<WaterSurface>();

        ChangeSpawnerState(true);
        StartCoroutine(SpawnCycle(timerToSpawn));
    }

    public IEnumerator SpawnCycle(float timer)
    {
        while (isSpawning)
        {
            Vector3 randomPoint = GetRandomPointInBox();

            int randomIndex = Random.Range(0, objectsToSpawn.Count);

            yield return new WaitForSeconds(timer);

            GameObject spawnedResource = Pool.Instantiate(objectsToSpawn[randomIndex], randomPoint, Quaternion.identity);

            spawnedResource.GetComponent<FloatingObject>().targetSurface = targetSurface;
        }
    }


    public void ChangeSpawnerState(bool state)
    {
        isSpawning = state;
    }
}
