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
    /// Smoothly drains/lowers the water level to a target height (e.g. from 87 down to 78).
    /// </summary>
    public void DrainToTargetHeight(float targetHeight, float drainSpeed = 2f)
    {
        if (activeDrainCoroutine != null)
        {
            StopCoroutine(activeDrainCoroutine);
        }
        activeDrainCoroutine = StartCoroutine(AnimateWaterToHeight(targetHeight, drainSpeed));
    }

    /// <summary>
    /// Smoothly lowers the water by a specific relative offset.
    /// </summary>
    public void LowerWaterByOffset(float amountToDrain, float drainSpeed = 2f)
    {
        float targetHeight = CurrentWaterHeight - amountToDrain;
        DrainToTargetHeight(targetHeight, drainSpeed);
    }

    private IEnumerator AnimateWaterToHeight(float targetY, float speed)
    {
        if (waterLevelTransform == null) yield break;

        Debug.Log($"[WaterLevel] Starting drain sequence towards {targetY}m.");

        while (Mathf.Abs(waterLevelTransform.position.y - targetY) > 0.01f)
        {
            Vector3 currentPos = waterLevelTransform.position;
            currentPos.y = Mathf.MoveTowards(currentPos.y, targetY, speed * Time.deltaTime);
            waterLevelTransform.position = currentPos;
            currentWaterHeight = currentPos.y;

            yield return null;
        }

        Vector3 finalPos = waterLevelTransform.position;
        finalPos.y = targetY;
        waterLevelTransform.position = finalPos;
        currentWaterHeight = targetY;

        Debug.Log($"[WaterLevel] Water level successfully reached: {currentWaterHeight}m");
        activeDrainCoroutine = null;
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