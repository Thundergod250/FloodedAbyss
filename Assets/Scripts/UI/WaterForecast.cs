using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WaterForecast : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image currentTimeline;
    [SerializeField] private Image timeline;

    [Header("Timer")]
    [SerializeField] private float timer;

    [Header("Breakpoints")]
    [SerializeField] private List<Image> eventpoints = new List<Image>();

    private WaterLevel level;

    private void Start()
    {
        level = GameManager.Instance.waterLevel;

        StartCoroutine(ProgressTimeline());
    }

    private IEnumerator ProgressTimeline()
    {
        float elapsed = 0f;

        while (elapsed < timer)
        {
            elapsed += Time.deltaTime;

            float progress = elapsed / timer;

            RectTransform timelineRect = timeline.rectTransform;
            RectTransform currentRect = currentTimeline.rectTransform;

            float startX = 0f;
            float endX = timelineRect.rect.width;

            Vector2 position = currentRect.anchoredPosition;
            position.x = Mathf.Lerp(startX, endX, progress);

            currentRect.anchoredPosition = position;

            CheckBreakpoints();

            yield return null;
        }
    }

    private void CheckBreakpoints()
    {
        foreach (Image eventpoint in eventpoints)
        {
            if (Mathf.Abs(currentTimeline.rectTransform.anchoredPosition.x - eventpoint.rectTransform.anchoredPosition.x) < 1f) // if distance is below 1
            {
                Debug.Log("Current timeline reached breakpoint: " + eventpoint.name);

                eventpoint.GetComponent<TimelineEvents>().IncreaseWaterLevel(level.waterLevelTransform);
            }
        }
    }
}