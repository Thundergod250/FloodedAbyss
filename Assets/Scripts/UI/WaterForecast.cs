using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WaterForecast : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image currentTimeline;
    [SerializeField] private Image timeline;
    [SerializeField] private TextMeshProUGUI waterLevelText;

    [Header("Timer")]
    [SerializeField] private float timer;

    [Header("Breakpoints")]
    [SerializeField] private List<Image> eventpoints = new List<Image>();

    [Header("Water Rise")]
    [SerializeField] private float baseWaterRiseRate = 1f;
    private float waterRiseRate;

    [Header("Pump Reduction")]
    private float pumpReductionRate;

    private bool timelinePaused = true;
    private bool pumpActivated = false;

    private WaterLevel level;

    private void Start()
    {
        level = GameManager.Instance.waterLevel;

        StartCoroutine(ProgressTimeline());
    }

    private void Update()
    {
        if (level == null)
            return;

        waterLevelText.text =
            level.waterLevelTransform.position.y.ToString("F1") + "m";

        if (!pumpActivated)
            return;

        // Water rise - pump reduction = actual water movement
        float netRate = waterRiseRate - pumpReductionRate;

        Vector3 position = level.waterLevelTransform.position;
        position.y += netRate * Time.deltaTime;

        level.waterLevelTransform.position = position;
    }

    public void IncreaseWaterRise(float amount)
    {
        waterRiseRate += amount;

        Debug.Log(
            $"Water rise increased by {amount}. " +
            $"Current rise rate: {waterRiseRate} m/s"
        );
    }

    public void SetPumpReduction(float amount)
    {
        pumpReductionRate = Mathf.Max(0f, amount);

        Debug.Log(
            $"Pump reduction set to {pumpReductionRate} m/s"
        );
    }

    public void StopPumpReduction()
    {
        pumpReductionRate = 0f;

        Debug.Log("Pump reduction stopped.");
    }

    private IEnumerator ProgressTimeline()
    {
        float elapsed = 0f;

        while (elapsed < timer)
        {
            if (!timelinePaused)
            {
                elapsed += Time.deltaTime;

                float progress = elapsed / timer;

                RectTransform timelineRect = timeline.rectTransform;
                RectTransform currentRect = currentTimeline.rectTransform;

                float startY = 0f;
                float endY = timelineRect.rect.height;

                Vector2 position = currentRect.anchoredPosition;
                position.y = Mathf.Lerp(startY, endY, progress);

                currentRect.anchoredPosition = position;

                CheckBreakpoints();
            }

            yield return null;
        }
    }

    private void CheckBreakpoints()
    {
        foreach (Image eventpoint in eventpoints)
        {
            if (Mathf.Abs(
                currentTimeline.rectTransform.anchoredPosition.y -
                eventpoint.rectTransform.anchoredPosition.y
            ) < 1f)
            {
                Debug.Log(
                    "Current timeline reached breakpoint: " +
                    eventpoint.name
                );

                eventpoint
                    .GetComponent<TimelineEvents>()
                    .IncreaseWaterLevel(this);
            }
        }
    }

    public void IncDecWaterLevel(float height)
    {
        level.waterLevelTransform.position = new Vector3(
            level.waterLevelTransform.position.x,
            height,
            level.waterLevelTransform.position.z
        );
    }

    public IEnumerator LowerWaterLevel(float targetHeight, float duration)
    {
        if (level == null)
            yield break;

        Transform waterTransform = level.waterLevelTransform;

        float startingHeight = waterTransform.position.y;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float progress = elapsed / duration;
            progress = Mathf.SmoothStep(0f, 1f, progress);

            float newHeight = Mathf.Lerp(
                startingHeight,
                targetHeight,
                progress
            );

            Vector3 position = waterTransform.position;
            position.y = newHeight;
            waterTransform.position = position;

            yield return null;
        }

        Vector3 finalPosition = waterTransform.position;
        finalPosition.y = targetHeight;
        waterTransform.position = finalPosition;
    }

    public float GetCurrentWaterHeight()
    {
        return level.waterLevelTransform.position.y;
    }

    public void EnableTimeline()
    {
        timelinePaused = false;
        pumpActivated = true;

        // Always start at 1 m/s
        waterRiseRate = baseWaterRiseRate;
    }
}