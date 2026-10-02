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
    [SerializeField] private float maximumWaterLevel = 95f;
    [SerializeField] private float currentMaximumWaterCapacity;

    private float targetMaximumWaterCapacity;
    private float waterRiseRate = 1f;

    private bool timelinePaused = true;
    private bool pumpActivated = false;

    private Coroutine maximumCapacityCoroutine;
    private WaterLevel level;

    private void Start()
    {
        level = GameManager.Instance.waterLevel;

        if (level != null)
        {
            maximumWaterLevel = level.CurrentWaterHeight;
            currentMaximumWaterCapacity = maximumWaterLevel;
            targetMaximumWaterCapacity = maximumWaterLevel;
        }

        StartCoroutine(ProgressTimeline());
    }

    private void Update()
    {
        if (level == null) return;

        // Display tracked water height directly from WaterLevel
        waterLevelText.text = level.CurrentWaterHeight.ToString("F1") + "m";

        if (!pumpActivated) return;

        // If below max capacity, continuously increase water level via WaterLevel script
        if (level.CurrentWaterHeight < currentMaximumWaterCapacity)
        {
            float amountToRise = waterRiseRate * Time.deltaTime;

            // Cap the rise amount so it doesn't overshoot maximum capacity
            if (level.CurrentWaterHeight + amountToRise > currentMaximumWaterCapacity)
            {
                level.SetWaterLevel(currentMaximumWaterCapacity);
            }
            else
            {
                level.IncreaseWaterLevel(amountToRise);
            }
        }
    }

    public void IncreaseWaterRise(float amount)
    {
        waterRiseRate += amount;

        Debug.Log(
            $"Water rise increased by {amount}. " +
            $"Current water rise rate: {waterRiseRate}/sec"
        );
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

                position.y = Mathf.Lerp(
                    startY,
                    endY,
                    progress
                );

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

                TimelineEvents timelineEvent =
                    eventpoint.GetComponent<TimelineEvents>();

                if (timelineEvent != null)
                {
                    timelineEvent.IncreaseWaterLevel(this);
                }
            }
        }
    }

    /// <summary>
    /// Instantly sets the water height using WaterLevel.
    /// </summary>
    public void IncDecWaterLevel(float height)
    {
        if (level != null)
        {
            level.SetWaterLevel(height);
        }
    }

    /// <summary>
    /// Smoothly lowers the water height to targetHeight over duration using WaterLevel calls.
    /// </summary>
    public IEnumerator LowerWaterLevel(float targetHeight, float duration)
    {
        if (level == null) yield break;

        float startingHeight = level.CurrentWaterHeight;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.SmoothStep(0f, 1f, elapsed / duration);

            float newHeight = Mathf.Lerp(startingHeight, targetHeight, progress);
            level.SetWaterLevel(newHeight);

            yield return null;
        }

        level.SetWaterLevel(targetHeight);
    }

    /// <summary>
    /// Smoothly adjusts the water height by an offset over 5 seconds using WaterLevel calls.
    /// </summary>
    public IEnumerator AdjustWaterLevelOverTime(float adjustment)
    {
        if (level == null) yield break;

        float startingHeight = level.CurrentWaterHeight;
        float targetHeight = startingHeight + adjustment;

        float duration = 5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.SmoothStep(0f, 1f, elapsed / duration);

            float newHeight = Mathf.Lerp(startingHeight, targetHeight, progress);
            level.SetWaterLevel(newHeight);

            yield return null;
        }

        level.SetWaterLevel(targetHeight);
    }

    public float GetCurrentWaterHeight()
    {
        return level != null ? level.CurrentWaterHeight : 0f;
    }

    public void EnableTimeline()
    {
        if (pumpActivated) return;

        timelinePaused = false;
        pumpActivated = true;
        waterRiseRate = baseWaterRiseRate;

        Debug.Log(
            $"Timeline started. Water rise rate: {waterRiseRate}/sec"
        );
    }

    private IEnumerator AnimateMaximumWaterCapacity()
    {
        float duration = 5f;

        while (!Mathf.Approximately(
            currentMaximumWaterCapacity,
            targetMaximumWaterCapacity))
        {
            float startingCapacity = currentMaximumWaterCapacity;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                float progress = elapsed / duration;
                progress = Mathf.SmoothStep(0f, 1f, progress);

                currentMaximumWaterCapacity = Mathf.Lerp(
                    startingCapacity,
                    targetMaximumWaterCapacity,
                    progress
                );

                yield return null;
            }

            currentMaximumWaterCapacity = targetMaximumWaterCapacity;
        }

        maximumCapacityCoroutine = null;
    }

    public void ChangeMaximumWater(float amount)
    {
        targetMaximumWaterCapacity += amount;

        if (maximumCapacityCoroutine == null)
        {
            maximumCapacityCoroutine = StartCoroutine(
                AnimateMaximumWaterCapacity()
            );
        }
    }
}