using Unity.VisualScripting;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class WaterLevel : MonoBehaviour
{
    [Header("Water Surface Reference")]
    public Transform waterLevelTransform;

    [Header("Swimming Settings")]
    public float floatDepth = 2;
    public float floatForce = 10;
    public float waterGravity = -10f;
    public float minimumFloatDepth = 0;

    [Header("Draining / Rising Structures")]
    [SerializeField] private Transform[] buildingsToRaise;
    [SerializeField] private float heightChangePerStage = 5f;
    [SerializeField] private float moveSpeed = 2f;

    private GameObject playerRefTest;
    private PlayerMovement playerMovement;

    private void Start()
    {
        if (GameManager.Instance != null && GameManager.Instance.playerController != null)
        {
            playerRefTest = GameManager.Instance.playerController.gameObject;
            playerMovement = playerRefTest.GetComponent<PlayerMovement>();
        }
    }

    private void Update()
    {
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

    public void LowerWaterOrRaiseBuildings()
    {
        StopAllCoroutines();
        StartCoroutine(AnimateDrainStage());
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
    }
}