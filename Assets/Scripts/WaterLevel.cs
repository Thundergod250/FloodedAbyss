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
    [SerializeField] private float basePumpDrainSpeed = 3f;        // Downward pull speed of active pumps
    [SerializeField] private float defaultFloodRiseSpeed = 2f;     // Rise speed when zero pumps are active
    [SerializeField] private float initialFloodSpeed = 1f;         // Starting flood speed (1m/s)
    [SerializeField] private float floodAccelerationRate = 0.1f;    // Speed increase per sec after grace period
    [SerializeField] private float gracePeriodDuration = 30f;      // Grace period in seconds

    [Header("Tracking / Debug")]
    [SerializeField] private float currentWaterHeight;
    [SerializeField] private float currentFloodSpeed = 0f;
    [SerializeField] private float currentActiveThreshold = 0f;
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
    /// Updates the current tug-of-war target height and cumulative threshold.
    /// Moves water towards target (lowering or rising based on remaining active pumps).
    /// </summary>
    public void DrainToTargetHeight(float targetHeight, float pumpThreshold)
    {
        if (activeDrainCoroutine != null)
        {
            StopCoroutine(activeDrainCoroutine);
        }
        activeDrainCoroutine = StartCoroutine(AnimateTugOfWarDrain(targetHeight, pumpThreshold));
    }

    /// <summary>
    /// Called when all pumps turn inactive. Water smoothly rises back up to max floor.
    /// </summary>
    public void OnAllPumpsDeactivated(float maxWaterHeight)
    {
        if (activeDrainCoroutine != null)
        {
            StopCoroutine(activeDrainCoroutine);
        }
        activeDrainCoroutine = StartCoroutine(AnimateUncheckedFloodRise(maxWaterHeight));
    }

    private IEnumerator AnimateTugOfWarDrain(float targetY, float threshold)
    {
        if (waterLevelTransform == null) yield break;

        isTugOfWarActive = true;
        isGracePeriodActive = true;
        gracePeriodTimer = gracePeriodDuration;
        currentFloodSpeed = initialFloodSpeed; // Starts at 1m/s
        currentActiveThreshold = threshold;

        Debug.Log($"[WaterLevel] Updating Water Dynamics | Target Height: {targetY}m | Combined Threshold: {threshold}");

        while (isTugOfWarActive)
        {
            // 1. Grace Period Timer
            if (isGracePeriodActive)
            {
                gracePeriodTimer -= Time.deltaTime;
                if (gracePeriodTimer <= 0f)
                {
                    isGracePeriodActive = false;
                    gracePeriodTimer = 0f;
                    Debug.Log("[WaterLevel] Grace period ended! Flood acceleration active.");
                }
            }
            else
            {
                // 2. Flood Acceleration
                currentFloodSpeed += floodAccelerationRate * Time.deltaTime;
            }

            Vector3 pos = waterLevelTransform.position;

            // If water is higher than current target height supported by active pumps
            if (pos.y > targetY)
            {
                if (currentFloodSpeed <= currentActiveThreshold)
                {
                    // Pumps can handle it -> Drain water down towards targetY
                    pos.y = Mathf.MoveTowards(pos.y, targetY, basePumpDrainSpeed * Time.deltaTime);
                }
                else
                {
                    // Flood speed exceeded pump threshold -> Water rises
                    float excessRiseSpeed = currentFloodSpeed - currentActiveThreshold;
                    pos.y += excessRiseSpeed * Time.deltaTime;
                }
            }
            // If water is LOWER than target height (e.g. lost a pump, so safe floor raised from 20m up to 35m)
            else if (pos.y < targetY)
            {
                // Water smoothly rises back up to the new target level
                pos.y = Mathf.MoveTowards(pos.y, targetY, defaultFloodRiseSpeed * Time.deltaTime);
            }

            waterLevelTransform.position = pos;
            currentWaterHeight = pos.y;

            yield return null;
        }

        activeDrainCoroutine = null;
    }

    private IEnumerator AnimateUncheckedFloodRise(float maxWaterHeight)
    {
        if (waterLevelTransform == null) yield break;

        isTugOfWarActive = false;
        isGracePeriodActive = false;

        Debug.Log($"[WaterLevel] All pumps offline! Water rising back to {maxWaterHeight}m.");

        while (waterLevelTransform.position.y < maxWaterHeight)
        {
            Vector3 pos = waterLevelTransform.position;
            pos.y = Mathf.MoveTowards(pos.y, maxWaterHeight, defaultFloodRiseSpeed * Time.deltaTime);
            waterLevelTransform.position = pos;
            currentWaterHeight = pos.y;

            yield return null;
        }

        activeDrainCoroutine = null;
    }

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