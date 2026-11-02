using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WaterForecast : MonoBehaviour
{
    [Header("Timeline Settings")]
    [SerializeField] private float eventDuration = 60f;
    public float EventDuration => eventDuration;

    [Header("Weather Events")]
    [SerializeField] private List<GameObject> eventpoints = new();

    [Header("Random Backlog")]
    [SerializeField] private int initialEventCount = 3;

    [Header("WaterLevel")]
    [SerializeField] private Slider waterFill;

    [SerializeField] private List<TimelineEvents> weatherBacklog = new();
    
    [Header("Debug")]
    [SerializeField] private float currentTime;
    public float CurrentTime => currentTime;

    private readonly List<TimelineEvents> availableEvents = new();

    private bool timelinePaused;
    private Coroutine timelineCoroutine;

    private void Start()
    {
        if (eventDuration <= 0f)
        {
            Debug.LogWarning("[WaterForecast] Event duration must be greater than zero.", this);
            return;
        }

        BuildAvailableEvents();

        initialEventCount = 4;
        BuildInitialBacklog();

        if (weatherBacklog.Count == 0)
        {
            Debug.LogWarning("[WaterForecast] No valid weather events found.", this);
            return;
        }

        StartWeather();
    }

    private void Update()
    {
        if (waterFill != null && GameManager.Instance != null && GameManager.Instance.WaterLevel != null)
        {
            waterFill.value = GameManager.Instance.WaterLevel.CurrentWaterHeight;
        }
    }

    private void BuildAvailableEvents()
    {
        availableEvents.Clear();

        foreach (GameObject eventPoint in eventpoints)
        {
            if (eventPoint == null) continue;

            TimelineEvents weatherEvent = eventPoint.GetComponentInChildren<TimelineEvents>();
            if (weatherEvent == null) weatherEvent = eventPoint.GetComponent<TimelineEvents>();

            if (weatherEvent != null && !availableEvents.Contains(weatherEvent))
            {
                availableEvents.Add(weatherEvent);
            }
        }
    }

    private void BuildInitialBacklog()
    {
        weatherBacklog.Clear();
        int count = Mathf.Min(Mathf.Max(0, initialEventCount), availableEvents.Count);

        for (int i = 0; i < count; i++)
        {
            weatherBacklog.Add(availableEvents[i]);
        }
    }

    private IEnumerator ProgressTimeline()
    {
        while (true)
        {
            currentTime = 0f;

            while (currentTime < eventDuration)
            {
                if (!timelinePaused)
                {
                    currentTime += Time.deltaTime;
                }
                yield return null;
            }

            if (weatherBacklog.Count > 0)
                weatherBacklog.RemoveAt(0);

            AddOneRandomWeatherEvent();

            if (weatherBacklog.Count == 0)
            {
                timelineCoroutine = null;
                yield break;
            }

            currentTime = 0f;
            PlayCurrentWeather();
        }
    }

    private void AddOneRandomWeatherEvent()
    {
        if (availableEvents.Count == 0) return;
        int randomIndex = Random.Range(0, availableEvents.Count);
        weatherBacklog.Add(availableEvents[randomIndex]);
    }

    private void PlayCurrentWeather()
    {
        if (weatherBacklog.Count > 0 && weatherBacklog[0] != null)
        {
            weatherBacklog[0].TimelinePlayRain(this);
        }
    }

    // Public accessor for UIWaterForecast to query current/upcoming items safely
    public TimelineEvents GetWeatherEvent(int index)
    {
        if (index < 0 || index >= weatherBacklog.Count) return null;
        return weatherBacklog[index];
    }

    public void PauseTimeline() => timelinePaused = true;
    public void ResumeTimeline() { if (timelineCoroutine != null) timelinePaused = false; }

    private void SetRainState(rainStrength strength, bool playAudio)
    {
        if (GameManager.Instance == null) return;

        if (GameManager.Instance.VFXController != null)
        {
            if (strength == rainStrength.Sunny)
                GameManager.Instance.VFXController.StopRain();
            else
                GameManager.Instance.VFXController.PlayRain(strength);
        }

        if (GameManager.Instance.SoundManager != null)
        {
            if (playAudio)
                GameManager.Instance.SoundManager.Play("SFXRaining");
            else
                GameManager.Instance.SoundManager.Stop("SFXRaining");
        }
    }

    public void PlayWeakRain()   => SetRainState(rainStrength.Weak, true);
    public void PlayDecentRain() => SetRainState(rainStrength.Decent, true);
    public void PlayStrongRain() => SetRainState(rainStrength.Strong, true);

    public void StopRain()
    {
        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.VFXController != null)
                GameManager.Instance.VFXController.StopRain();

            if (GameManager.Instance.SoundManager != null)
                GameManager.Instance.SoundManager.Stop("SFXRaining");
        }
    }

    #region Start and Stop Weather
    public void StartWeather()
    {
        if (timelineCoroutine != null) return;
        if (weatherBacklog.Count == 0) return;

        timelinePaused = false;
        PlayCurrentWeather();

        timelineCoroutine = StartCoroutine(ProgressTimeline());
    }

    public void StopWeather()
    {
        timelinePaused = true;
        if (timelineCoroutine != null)
        {
            StopCoroutine(timelineCoroutine);
            timelineCoroutine = null;
        }
        StopRain();
    }
    #endregion
}