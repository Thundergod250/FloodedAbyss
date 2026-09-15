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

    public bool isSpawning;

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
            yield return new WaitForSeconds(timer);

            GameObject spawnedResource = Instantiate(objectsToSpawn[0], transform.position, Quaternion.identity);

            spawnedResource.GetComponent<FloatingObject>().targetSurface = targetSurface;
        }
    }

    public void ChangeSpawnerState(bool state)
    {
        isSpawning = state;
    }

}
