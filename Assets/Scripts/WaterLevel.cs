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

    [Header("Tracking / Debug")]
    [SerializeField] private float currentWaterHeight;

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
    /// Increases the water level height by the specified amount.
    /// </summary>
    /// <param name="amount">Units to raise the water level.</param>
    public void IncreaseWaterLevel(float amount)
    {
        ChangeWaterLevel(amount);
    }

    /// <summary>
    /// Decreases the water level height by the specified amount.
    /// </summary>
    /// <param name="amount">Units to lower the water level.</param>
    public void DecreaseWaterLevel(float amount)
    {
        ChangeWaterLevel(-Mathf.Abs(amount));
    }

    /// <summary>
    /// Sets the water level directly to a specific world Y position.
    /// </summary>
    /// <param name="targetY">Target Y height for water level.</param>
    public void SetWaterLevel(float targetY)
    {
        if (waterLevelTransform != null)
        {
            Vector3 pos = waterLevelTransform.position;
            pos.y = targetY;
            waterLevelTransform.position = pos;
            currentWaterHeight = targetY;

            Debug.Log($"[WaterLevel] Water level directly set to: {currentWaterHeight}");
        }
    }

    /// <summary>
    /// Core function handling offset adjustments to the water level height.
    /// </summary>
    /// <param name="deltaY">Amount to alter Y position (positive to raise, negative to lower).</param>
    public void ChangeWaterLevel(float deltaY)
    {
        if (waterLevelTransform != null)
        {
            Vector3 pos = waterLevelTransform.position;
            pos.y += deltaY;
            waterLevelTransform.position = pos;
            currentWaterHeight = pos.y;

            Debug.Log($"[WaterLevel] Water level changed by {deltaY}. Current Height: {currentWaterHeight}");
        }
    }

    /// <summary>
    /// Smoothly lowers water level by 'heightChangePerStage' and raises structures simultaneously.
    /// </summary>
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

            // Lower water plane
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

            // Raise assigned structures
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

        Debug.Log($"[WaterLevel] Stage transition complete. Final Water Height: {currentWaterHeight}");
    }
}