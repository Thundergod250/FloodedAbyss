using System.Collections.Generic;
using UnityEngine;

public class MineManager : MonoBehaviour
{
    [Header("Timer")]
    public float timeLimit = 60f;

    [Header("Water")]
/*    public Transform waterLevelTransform;
    public float startingWaterHeight;
    public float maximumWaterHeight;*/

    [Header("Ore Prefabs")]
    public GameObject tinOre;
    public GameObject copperOre;
    public GameObject ironOre;

    [Header("Ore Spawn Points")]
    public Transform[] spawnPoints;

    [Header("Ore Amount")]
    public int minimumOres = 5;
    public int maximumOres = 10;

/*    private float currentTime;
    private bool timerRunning = false;*/

    private List<GameObject> spawnedOres = new List<GameObject>();


    private void Start()
    {
/*        currentTime = timeLimit;

        if (waterLevelTransform != null)
        {
            startingWaterHeight = waterLevelTransform.position.y;
        }*/
    }


    private void Update()
    {
/*        if (!timerRunning)
            return;

        currentTime -= Time.deltaTime;

        if (currentTime <= 0f)
        {
            currentTime = 0f;
            timerRunning = false;
        }

        UpdateWaterLevel();*/
    }

    public void EnterMine()
    {
        Debug.Log("Player entered the mine.");

        //currentTime = timeLimit;

        //ResetWaterLevel();

        GenerateOres();

        //StartTimer();
    }


    public void LeaveMine()
    {
        Debug.Log("Player left the mine.");

        //StopTimer();

        ClearOres();

        //currentTime = timeLimit;

        //ResetWaterLevel();
    }

/*    public void StartTimer()
    {
        timerRunning = true;
    }


    public void StopTimer()
    {
        timerRunning = false;
    }


    public float GetRemainingTime()
    {
        return currentTime;
    }

    private void UpdateWaterLevel()
    {
        if (waterLevelTransform == null)
            return;

        float timePercentage = 1f - (currentTime / timeLimit);

        float newWaterHeight = Mathf.Lerp(
            startingWaterHeight,
            maximumWaterHeight,
            timePercentage
        );

        Vector3 waterPosition = waterLevelTransform.position;

        waterPosition.y = newWaterHeight;

        waterLevelTransform.position = waterPosition;
    }


    private void ResetWaterLevel()
    {
        if (waterLevelTransform == null)
            return;

        Vector3 waterPosition = waterLevelTransform.position;

        waterPosition.y = startingWaterHeight;

        waterLevelTransform.position = waterPosition;
    }
*/

    public void GenerateOres()
    {

        ClearOres();

        if (spawnPoints.Length == 0)
        {
            Debug.LogWarning("No ore spawn points assigned.");
            return;
        }

        int oreAmount = Random.Range(
            minimumOres,
            maximumOres + 1
        );

        oreAmount = Mathf.Min(oreAmount, spawnPoints.Length);

        List<Transform> availableSpawnPoints =
            new List<Transform>(spawnPoints);

        for (int i = 0; i < oreAmount; i++)
        {
            int randomSpawnIndex =
                Random.Range(0, availableSpawnPoints.Count);

            Transform spawnPoint =
                availableSpawnPoints[randomSpawnIndex];

            availableSpawnPoints.RemoveAt(randomSpawnIndex);

            GameObject orePrefab = GetRandomOre();

            if (orePrefab == null)
                continue;

            GameObject newOre = Instantiate(
                orePrefab,
                spawnPoint.position,
                spawnPoint.rotation
            );

            spawnedOres.Add(newOre);
        }

        Debug.Log("Generated " + oreAmount + " ores.");
    }


    private GameObject GetRandomOre()
    {
        int randomOre = Random.Range(0, 3);

        switch (randomOre)
        {
            case 0:
                return tinOre;

            case 1:
                return copperOre;

            case 2:
                return ironOre;

            default:
                return tinOre;
        }
    }

    public void ClearOres()
    {
        foreach (GameObject ore in spawnedOres)
        {
            if (ore != null)
            {
                Destroy(ore);
            }
        }

        spawnedOres.Clear();
    }
}