using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WaterForecast : MonoBehaviour
{
    [Header("Weather UI")]
    [SerializeField] private Image currentWeatherImage;
    [SerializeField] private Image nextWeatherImage;
    [SerializeField] private Image followingWeatherImage;
    [SerializeField] private Image followingWeather2Image;

    [Header("Timeline Settings")]
    [SerializeField] private float eventDuration = 60f;

    [Header("Weather Events")]
    [SerializeField] private List<GameObject> eventpoints = new List<GameObject>();

    [Header("Random Backlog")]
    [SerializeField] private int initialEventCount = 3;
    //[SerializeField] private int generatedBacklogSize = 10;

    [Header("Rain FX")]
    [SerializeField] private ParticleSystem rainFX;
    [SerializeField] private int weakRain = 5000;
    [SerializeField] private int decentRain = 10000;
    [SerializeField] private int strongRain = 15000;

    [Header("WaterLevel")]
    [SerializeField] private Slider waterFill;

    // CHANGED: This list contains only current and upcoming events.
    [SerializeField]
    private List<TimelineEvents> weatherBacklog =
        new List<TimelineEvents>();

    private readonly List<TimelineEvents> availableEvents =
        new List<TimelineEvents>();

    private bool timelinePaused;
    private Coroutine timelineCoroutine;

    private void Start()
    {
        if (rainFX != null)
            rainFX.Stop();

        if (eventDuration <= 0f)
        {
            Debug.LogWarning(
                "[WaterForecast] Event duration must be greater than zero.",
                this);
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

        UpdateWeatherImages();

        if (currentWeatherImage != null)
        {
            Slider timelineSlider =
                currentWeatherImage.GetComponentInParent<Slider>();

            if (timelineSlider != null)
                timelineSlider.maxValue = eventDuration;
        }

        bool tutorialStillRunning = ObjectiveManager.Instance == null || !ObjectiveManager.Instance.AllObjectiveCompleted;

        SetTutorialPaused(tutorialStillRunning);
    }

    private void Update()
    {
        waterFill.value = GameManager.Instance.WaterLevel.CurrentWaterHeight;
    }

    private void BuildAvailableEvents()
    {
        availableEvents.Clear();

        foreach (GameObject eventPoint in eventpoints)
        {
            if (eventPoint == null)
                continue;

            // NEW: Try to find TimelineEvents in children first.
            TimelineEvents weatherEvent =
                eventPoint.GetComponentInChildren<TimelineEvents>();

            // NEW: If not found, check the same GameObject.
            if (weatherEvent == null)
                weatherEvent = eventPoint.GetComponent<TimelineEvents>();

            if (weatherEvent != null &&
                !availableEvents.Contains(weatherEvent))
            {
                availableEvents.Add(weatherEvent);
            }
        }
    }

    // CHANGED: Preserve the first events in Inspector order.
    private void BuildInitialBacklog()
    {
        weatherBacklog.Clear();

        int count = Mathf.Min(
            Mathf.Max(0, initialEventCount),
            availableEvents.Count
        );

        for (int i = 0; i < count; i++)
        {
            weatherBacklog.Add(availableEvents[i]);
        }

        if (weatherBacklog.Count < initialEventCount)
        {
            Debug.LogWarning(
                "[WaterForecast] Fewer unique weather events than requested.",
                this
            );
        }
    }

    private IEnumerator ProgressTimeline()
    {
        while (true)
        {
            float elapsed = 0f;

            while (elapsed < eventDuration)
            {
                if (!timelinePaused)
                {
                    elapsed += Time.deltaTime;
                    currentWeatherImage.GetComponentInParent<Slider>().value = elapsed;
                }
                yield return null;
            }

            // Remove the weather event that just finished.
            if (weatherBacklog.Count > 0)
                weatherBacklog.RemoveAt(0);

            // Add only one new random event.
            AddOneRandomWeatherEvent();

            if (weatherBacklog.Count == 0)
            {
                Debug.LogWarning(
                    "[WaterForecast] No weather events available.",
                    this
                );

                timelineCoroutine = null;
                yield break;
            }

            UpdateWeatherImages();
            PlayCurrentWeather();
        }
    }

    // NEW: Append randomized events without clearing the existing queue.
    private void AddOneRandomWeatherEvent()
    {
        if (availableEvents.Count == 0)
            return;

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

    private void UpdateWeatherImages()
    {
        SetWeatherImage(currentWeatherImage, 0);
        SetWeatherImage(nextWeatherImage, 1);
        SetWeatherImage(followingWeatherImage, 2);
        SetWeatherImage(followingWeather2Image, 3);
    }

    private void SetWeatherImage(Image targetImage, int eventIndex)
    {
        if (targetImage == null)
            return;

        if (eventIndex < 0 ||
            eventIndex >= weatherBacklog.Count ||
            weatherBacklog[eventIndex] == null)
        {
            targetImage.sprite = null;
            targetImage.enabled = false;
            return;
        }

        Sprite weatherSprite = weatherBacklog[eventIndex].WeatherSprite;

        targetImage.sprite = weatherSprite;
        targetImage.enabled = weatherSprite != null;
    }

    public void PauseTimeline()
    {
        timelinePaused = true;
    }

    public void ResumeTimeline()
    {
        if (timelineCoroutine != null)
            timelinePaused = false;
    }

    #region Rain Stuff

    private void PlayRainSFX()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.SoundManager.Play("SFXRaining");
    }

    private void StopRainSFX()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.SoundManager.Stop("SFXRaining");
    }

    private void SetRainEmission(float rate)
    {
        if (rainFX == null)
        {
            Debug.LogWarning(
                "[WaterForecast] Rain FX is not assigned.",
                this);
            return;
        }

        var emission = rainFX.emission;
        emission.rateOverTime = new ParticleSystem.MinMaxCurve(rate);

        if (!rainFX.isPlaying)
            rainFX.Play();

        if (rate > 0f)
            PlayRainSFX();
    }

    public void PlayWeakRain()
    {
        SetRainEmission(weakRain);
    }

    public void PlayDecentRain()
    {
        SetRainEmission(decentRain);
    }

    public void PlayStrongRain()
    {
        SetRainEmission(strongRain);
    }

    public void StopRain()
    {
        if (rainFX != null)
        {
            var emission = rainFX.emission;
            emission.rateOverTime = new ParticleSystem.MinMaxCurve(0f);

            rainFX.Stop(
                false,
                ParticleSystemStopBehavior.StopEmitting);
        }

        StopRainSFX();
    }

    #endregion

    #region Start and Stop Weather
    public void StartWeather()
    {
        if (timelineCoroutine != null)
            return;

        if (weatherBacklog.Count == 0)
        {
            Debug.LogWarning(
                "[WaterForecast] Cannot start: no weather events available.",
                this);
            return;
        }

        timelinePaused = false;

        UpdateWeatherImages();
        PlayCurrentWeather();

        timelineCoroutine = StartCoroutine(ProgressTimeline());
    }

    // NEW: Stop weather at the end of the game.
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

    public void SetTutorialPaused(bool paused)
    {
        if (paused)
        {
            timelinePaused = true;

            Debug.Log(
                "[WaterForecast] Tutorial active. Weather timeline paused.",
                this);

            return;
        }

        // Start the timeline if it has never been started.
        if (timelineCoroutine == null)
        {
            StartWeather();
        }
        else
        {
            ResumeTimeline();
        }

        Debug.Log(
            "[WaterForecast] Tutorial complete. Weather timeline enabled.",
            this);
    }
    #endregion
}