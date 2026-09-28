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

        currentMaximumWaterCapacity = maximumWaterLevel;
        targetMaximumWaterCapacity = maximumWaterLevel;

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

        Vector3 position = level.waterLevelTransform.position;

        position.y += waterRiseRate * Time.deltaTime;

        position.y = Mathf.Min(
            position.y,
            currentMaximumWaterCapacity
        );

        level.waterLevelTransform.position = position;
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

            progress = Mathf.SmoothStep(
                0f,
                1f,
                progress
            );

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

    public IEnumerator AdjustWaterLevelOverTime(float adjustment)
    {
        if (level == null)
            yield break;

        Transform waterTransform = level.waterLevelTransform;

        float startingHeight = waterTransform.position.y;
        float targetHeight = startingHeight + adjustment;

        float duration = 5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float progress = elapsed / duration;

            progress = Mathf.SmoothStep(
                0f,
                1f,
                progress
            );

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
        if (pumpActivated)
            return;

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