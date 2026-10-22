using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WaterForecast : MonoBehaviour
{
    [Header("Timeline UI")]
    [SerializeField] private Image currentTimeline;
    [SerializeField] private Image timeline;

    [Header("Timeline Settings")]
    [SerializeField] private float timer = 60f;

    [Header("Timeline Events")]
    [SerializeField] private List<Image> eventpoints = new List<Image>();

    [Header("Rain FX")]
    [SerializeField] private ParticleSystem rainFX;
    [SerializeField] private int weakRain = 5000;
    [SerializeField] private int decentRain = 10000;
    [SerializeField] private int strongRain = 15000;

    private readonly HashSet<TimelineEvents> triggeredEvents =
        new HashSet<TimelineEvents>();

    private bool timelinePaused = false; 
    private bool timelineCompleted = false;

    private Coroutine timelineCoroutine;

    private void Start()
    {
        rainFX.Stop();

        if (currentTimeline == null || timeline == null)
        {
            Debug.LogWarning("[WaterForecast] Timeline UI references are missing.", this);
            return;
        }

        if (timer <= 0f)
        {
            Debug.LogWarning("[WaterForecast] Timer must be greater than zero.",this);
            return;
        }

        // CHANGED: Reset the marker to its starting position.
        Vector2 position = currentTimeline.rectTransform.anchoredPosition;
        position.y = 0f;
        currentTimeline.rectTransform.anchoredPosition = position;

        // CHANGED: Start the timeline automatically.
        timelineCoroutine = StartCoroutine(ProgressTimeline());
    }

    private IEnumerator ProgressTimeline()
    {
        float elapsed = 0f;

        RectTransform timelineRect = timeline.rectTransform;
        RectTransform currentRect = currentTimeline.rectTransform;

        float startY = 0f;
        float endY = timelineRect.rect.height;

        // NEW: Check for an event at the starting position.
        CheckBreakpoints(currentRect.anchoredPosition.y, 0f);

        while (elapsed < timer)
        {
            if (!timelinePaused)
            {
                float previousElapsed = elapsed;

                elapsed += Time.deltaTime;

                float progress = Mathf.Clamp01(elapsed / timer);

                Vector2 position = currentRect.anchoredPosition;
                position.y = Mathf.Lerp(startY, endY, progress);
                currentRect.anchoredPosition = position;

                // CHANGED: Check every event crossed during this frame.
                CheckBreakpoints(position.y, previousElapsed);
            }

            yield return null;
        }

        // NEW: Finish at the exact endpoint.
        Vector2 finalPosition = currentRect.anchoredPosition;
        finalPosition.y = endY;
        currentRect.anchoredPosition = finalPosition;

        CheckBreakpoints(endY, timer);

        timelineCompleted = true;
        timelinePaused = true;
        timelineCoroutine = null;

        Debug.Log("[WaterForecast] Timeline completed.", this);
    }

    private void CheckBreakpoints(float currentY, float previousElapsed)
    {
        float endY = timeline.rectTransform.rect.height;
        float previousY = Mathf.Lerp(0f, endY, previousElapsed / timer);

        foreach (Image eventpoint in eventpoints)
        {
            if (eventpoint == null)
                continue;

            TimelineEvents timelineEvent =
                eventpoint.GetComponent<TimelineEvents>();

            if (timelineEvent == null ||
                triggeredEvents.Contains(timelineEvent))
                continue;

            float eventY = eventpoint.rectTransform.anchoredPosition.y;

            // CHANGED: Trigger only when the marker crosses the event position.
            bool crossedEvent =
                eventY >= previousY && eventY <= currentY;

            if (crossedEvent)
            {
                triggeredEvents.Add(timelineEvent);

                Debug.Log(
                    "[WaterForecast] Timeline reached event: " + eventpoint.name,
                    eventpoint
                );

                timelineEvent.TimelinePlayRain(this);
            }
        }
    }

    public void PauseTimeline()
    {
        timelinePaused = true;
    }

    public void ResumeTimeline()
    {
        if (!timelineCompleted && timelineCoroutine != null)
        {
            timelinePaused = false;
        }
    }

    #region Rain Stuff
    public void PlayWeakRain()
    {
        var emission = rainFX.emission;
        emission.rateOverTime = weakRain;

        rainFX.Play();
    }

    public void PlayDecentRain()
    {
        var emission = rainFX.emission;
        emission.rateOverTime = decentRain;

        rainFX.Play();
    }

    public void PlayStrongRain()
    {
        var emission = rainFX.emission;
        emission.rateOverTime = strongRain; 

        rainFX.Play();
    }

    public void StopRain()
    {
        rainFX.Stop();
    }

    #endregion
}