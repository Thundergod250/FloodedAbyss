using System.Collections.Generic;
using Unity.VisualScripting;
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
    public List<Transform> spawnPoints = new List<Transform>();
    public Transform mineLoc;

    [Header("Ore Amount")]
    public int minimumOres = 5;
    public int maximumOres = 10;


    [Header("Mine Area")]
    [SerializeField] private GameObject minePrefab;
    [SerializeField] private Transform exitLocation;
    private GameObject spawnedMine;

    /*    private float currentTime;
        private bool timerRunning = false;*/

    private List<GameObject> spawnedOres = new List<GameObject>();
    private PlayerController playerController;


    private void Start()
    {
        /*        currentTime = timeLimit;

                if (waterLevelTransform != null)
                {
                    startingWaterHeight = waterLevelTransform.position.y;
                }*/

        playerController = GameManager.Instance.playerController;
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

        SpawnMine();

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

        TeleportPlayer(exitLocation);

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

        if (spawnPoints.Count == 0)
        {
            Debug.LogWarning("No ore spawn points assigned.");
            return;
        }

        int oreAmount = Random.Range(
            minimumOres,
            maximumOres + 1
        );

        oreAmount = Mathf.Min(oreAmount, spawnPoints.Count);

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

            newOre.transform.SetParent(spawnedMine.transform, true);

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

    private void TeleportPlayer(Transform desintation)
    {
        playerController.GetComponent<CharacterController>().enabled = false;
        playerController.gameObject.transform.position = desintation.transform.position;
        playerController.GetComponent<CharacterController>().enabled = true;
    }

    private void SpawnMine()
    {
        Vector3 spawnPosition = new Vector3(mineLoc.transform.position.x, GameManager.Instance.waterLevel.waterLevelTransform.transform.position.y + 36, mineLoc.transform.position.z);

        spawnedMine = Instantiate(minePrefab, spawnPosition, mineLoc.rotation);

        Debug.Log("mineLoc position: " + mineLoc.position);
        Debug.Log("Spawned mine position: " + spawnedMine.transform.position);

        Transform spawnPoint = spawnedMine.GetComponentInChildren<MineOreSpawns>().PlayerSpawnPoint;

        if (spawnPoint != null)
        {
            TeleportPlayer(spawnPoint);
        }
        else
        {
            Debug.LogWarning("PlayerSpawnPoint was not found inside the mine prefab.");
        }

        EstablishOreLocations(spawnedMine);

        spawnedMine.GetComponentInChildren<MineExit>().mineManager = this;
    }

    private void EstablishOreLocations(GameObject mine)
    {
        MineOreSpawns oreSpawns = mine.GetComponentInChildren<MineOreSpawns>();

        if (oreSpawns == null)
        {
            Debug.LogWarning("MineOreSpawns was not found on the mine.");
            return;
        }

        spawnPoints = oreSpawns.oreSpawnlocations;
    }
}