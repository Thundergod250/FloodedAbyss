using System;
using System.Collections;
using UnityEngine;

public class StructurePumpFinalStand : MonoBehaviour
{
    [Header("Final Stand Variables")]
    [SerializeField] private bool finalStand;
    [SerializeField] private int finalStandWaterLevel;
    [SerializeField] private int finalStandDuration;
    [SerializeField] private bool finalStandActivated;

    [Header("Reset Final Stand")]
    [SerializeField] private bool resetFinalStand;

    private bool finalStandRunning;

    private void Update()
    {
        resetFinalStand = GameManager.Instance.WaterLevel.CurrentWaterHeight
                          <= finalStandWaterLevel - 5;

        if (resetFinalStand)
            finalStandActivated = false;

        if (finalStandActivated || finalStandRunning)
            return;

        finalStand = GameManager.Instance.WaterLevel.CurrentWaterHeight >= finalStandWaterLevel;

        if (finalStand)
            StartCoroutine(StillWater());
    }

    private IEnumerator StillWater()
    {
        if (finalStandRunning)
            yield break;

        finalStandRunning = true;

        WaterLevel waterLevel = GameManager.Instance.WaterLevel;

        // Save the original maximum height.
        float originalMaxHeight = waterLevel.MaxWaterHeight;

        float elapsedTime = 0f;

        // Temporarily cap the water.
        while (elapsedTime < finalStandDuration)
        {
            waterLevel.SetOceanHeight(finalStandWaterLevel);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Remove the temporary cap.
        waterLevel.SetOceanHeight(Mathf.RoundToInt(originalMaxHeight));

        finalStandActivated = true;
        finalStandRunning = false;
    }
}
