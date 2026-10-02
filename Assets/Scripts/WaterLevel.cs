using System.Collections;
using UnityEngine;

public class WaterLevel : MonoBehaviour
{
    [Header("Water Surface Reference")]
    public Transform waterLevelTransform;

    [Header("Draining / Rising Structures")]
    [SerializeField] private Transform[] buildingsToRaise;
    [SerializeField] private float heightChangePerStage = 5f;
    [SerializeField] private float moveSpeed = 2f;

    [Header("Tug-Of-War Mechanics")]
    [SerializeField] private float basePumpDrainSpeed = 3f;      // Downward pull speed of an active pump
    [SerializeField] private float initialFloodSpeed = 1f;       // Starting upward force of flood (1m/s)
    [SerializeField] private float floodAccelerationRate = 0.1f;  // How fast flood speeds up after grace period
    [SerializeField] private float gracePeriodDuration = 30f;    // Saving grace period in seconds before flood accelerates

    [Header("Tracking / Debug")]
    [SerializeField] private float currentWaterHeight;
    [SerializeField] private float currentFloodSpeed = 0f;
    [SerializeField] private float gracePeriodTimer = 0f;
    [SerializeField] private bool isTugOfWarActive = false;
    [SerializeField] private bool isGracePeriodActive = false;

    private GameObject playerRefTest;
    private PlayerMovement playerMovement;
    private Coroutine activeDrainCoroutine;

    public float CurrentWaterHeight => waterLevelTransform != null ? waterLevelTransform.position.y : currentWaterHeight;

    private void Start()
    {
        if (GameManager.Instance != null && GameManager.Instance.playerController != null)
        {
            playerRefTest = GameManager.Instance.playerController.gameObject;
            playerMovement = playerRefTest.GetComponent<PlayerMovement>();
        }

        if (waterLevelTransform != null)
        {
            currentWaterHeight = waterLevelTransform.position.y;
        }
    }

    private void Update()
    {
        if (waterLevelTransform != null)
        {
            currentWaterHeight = waterLevelTransform.position.y;
        }

        if (playerRefTest == null || waterLevelTransform == null) return;

        // Player Swimming Check
        float depth = waterLevelTransform.position.y - playerRefTest.transform.position.y;

        if (depth > 0f)
        {
            if (playerMovement != null)
            {
                playerMovement.isSwimming = true;
                playerMovement.SetWaterSurface(waterLevelTransform);
            }
        }
        else
        {
            if (playerMovement != null)
            {
                playerMovement.isSwimming = false;
            }
        }
    }

    /// <summary>
    /// Starts the Tug-of-War sequence when a pump activates.
    /// Includes a 30-second grace period before flood acceleration kicks in.
    /// </summary>
    /// <param name="targetHeight">Lowest depth the pump is trying to reach.</param>
    public void DrainToTargetHeight(float targetHeight)
    {
        if (activeDrainCoroutine != null)
        {
            StopCoroutine(activeDrainCoroutine);
        }
        activeDrainCoroutine = StartCoroutine(AnimateTugOfWarDrain(targetHeight));
    }

    private IEnumerator AnimateTugOfWarDrain(float targetY)
    {
        if (waterLevelTransform == null) yield break;

        isTugOfWarActive = true;
        isGracePeriodActive = true;
        gracePeriodTimer = gracePeriodDuration;
        currentFloodSpeed = initialFloodSpeed; // Starts flood at 1m/s

        Debug.Log($"[WaterLevel] Pump Activated! Target: {targetY}m | Grace Period Started: {gracePeriodDuration}s");

        while (isTugOfWarActive)
        {
            // 1. Handle Saving Grace Period Countdown
            if (isGracePeriodActive)
            {
                gracePeriodTimer -= Time.deltaTime;
                if (gracePeriodTimer <= 0f)
                {
                    isGracePeriodActive = false;
                    gracePeriodTimer = 0f;
                    Debug.Log("[WaterLevel] Grace period ended! Flood is now accelerating!");
                }
            }
            else
            {
                // 2. Accelerate the flood over time AFTER grace period ends
                currentFloodSpeed += floodAccelerationRate * Time.deltaTime;
            }

            // 3. Calculate net movement speed (Pump Pull Down (-) vs Flood Push Up (+))
            float netSpeed = currentFloodSpeed - basePumpDrainSpeed;

            // 4. Move water surface
            Vector3 pos = waterLevelTransform.position;
            pos.y += netSpeed * Time.deltaTime;

            // 5. Clamp so water doesn't drain BELOW target line
            if (pos.y < targetY)
            {
                pos.y = targetY;
            }

            waterLevelTransform.position = pos;
            currentWaterHeight = pos.y;

            yield return null;
        }

        activeDrainCoroutine = null;
    }

    /// <summary>
    /// Stops the active flood tug-of-war.
    /// </summary>
    public void StopTugOfWar()
    {
        isTugOfWarActive = false;
        isGracePeriodActive = false;
        if (activeDrainCoroutine != null)
        {
            StopCoroutine(activeDrainCoroutine);
            activeDrainCoroutine = null;
        }
    }

    public void IncreaseWaterLevel(float amount)
    {
        ChangeWaterLevel(amount);
    }

    public void DecreaseWaterLevel(float amount)
    {
        ChangeWaterLevel(-Mathf.Abs(amount));
    }

    public void SetWaterLevel(float targetY)
    {
        if (waterLevelTransform != null)
        {
            Vector3 pos = waterLevelTransform.position;
            pos.y = targetY;
            waterLevelTransform.position = pos;
            currentWaterHeight = targetY;
        }
    }

    public void ChangeWaterLevel(float deltaY)
    {
        if (waterLevelTransform != null)
        {
            Vector3 pos = waterLevelTransform.position;
            pos.y += deltaY;
            waterLevelTransform.position = pos;
            currentWaterHeight = pos.y;
        }
    }

    public void LowerWaterOrRaiseBuildings()
    {
        if (activeDrainCoroutine != null)
        {
            StopCoroutine(activeDrainCoroutine);
        }
        activeDrainCoroutine = StartCoroutine(AnimateDrainStage());
    }

    private IEnumerator AnimateDrainStage()
    {
        Vector3 targetWaterPos = waterLevelTransform != null
            ? waterLevelTransform.position + (Vector3.down * heightChangePerStage)
            : Vector3.zero;

        Vector3[] targetBuildingPos = new Vector3[buildingsToRaise != null ? buildingsToRaise.Length : 0];
        for (int i = 0; i < targetBuildingPos.Length; i++)
        {
            if (buildingsToRaise[i] != null)
            {
                targetBuildingPos[i] = buildingsToRaise[i].position + (Vector3.up * heightChangePerStage);
            }
        }

        bool complete = false;

        while (!complete)
        {
            complete = true;

            if (waterLevelTransform != null)
            {
                waterLevelTransform.position = Vector3.MoveTowards(
                    waterLevelTransform.position,
                    targetWaterPos,
                    moveSpeed * Time.deltaTime
                );

                currentWaterHeight = waterLevelTransform.position.y;

                if (Vector3.Distance(waterLevelTransform.position, targetWaterPos) > 0.01f)
                {
                    complete = false;
                }
            }

            if (buildingsToRaise != null)
            {
                for (int i = 0; i < buildingsToRaise.Length; i++)
                {
                    if (buildingsToRaise[i] != null)
                    {
                        buildingsToRaise[i].position = Vector3.MoveTowards(
                            buildingsToRaise[i].position,
                            targetBuildingPos[i],
                            moveSpeed * Time.deltaTime
                        );

                        if (Vector3.Distance(buildingsToRaise[i].position, targetBuildingPos[i]) > 0.01f)
                        {
                            complete = false;
                        }
                    }
                }
            }

            yield return null;
        }
    }
}