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
    [SerializeField] private Slider weatherTimerSlider;

    [Header("Timeline Settings")]
    [SerializeField] private float eventDuration = 60f;

    [Header("Weather Events")]
    [SerializeField] private List<GameObject> eventpoints = new();

    [Header("Random Backlog")]
    [SerializeField] private int initialEventCount = 3;

    [Header("WaterLevel")]
    [SerializeField] private Slider waterFill;

    [SerializeField]
    private List<TimelineEvents> weatherBacklog = new();

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

        if (weatherTimerSlider == null && currentWeatherImage != null)
        {
            weatherTimerSlider = currentWeatherImage.GetComponentInParent<Slider>();
        }

        if (weatherTimerSlider != null)
        {
            weatherTimerSlider.maxValue = eventDuration;
            weatherTimerSlider.value = 0f;
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
            float elapsed = 0f;

            if (weatherTimerSlider != null)
            {
                weatherTimerSlider.maxValue = eventDuration;
            }

            while (elapsed < eventDuration)
            {
                if (!timelinePaused)
                {
                    elapsed += Time.deltaTime;
                    if (weatherTimerSlider != null)
                    {
                        weatherTimerSlider.value = elapsed;
                    }
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

            UpdateWeatherImages();
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

    private void UpdateWeatherImages()
    {
        SetWeatherImage(currentWeatherImage, 0);
        SetWeatherImage(nextWeatherImage, 1);
        SetWeatherImage(followingWeatherImage, 2);
        SetWeatherImage(followingWeather2Image, 3);
    }

    private void SetWeatherImage(Image targetImage, int eventIndex)
    {
        if (targetImage == null) return;

        if (eventIndex < 0 || eventIndex >= weatherBacklog.Count || weatherBacklog[eventIndex] == null)
        {
            targetImage.sprite = null;
            targetImage.enabled = false;
            return;
        }

        Sprite weatherSprite = weatherBacklog[eventIndex].WeatherSprite;
        targetImage.sprite = weatherSprite;
        targetImage.enabled = weatherSprite != null;
    }

    public void PauseTimeline() => timelinePaused = true;
    public void ResumeTimeline() { if (timelineCoroutine != null) timelinePaused = false; }

    #region Rain Delegation to VFXController

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

    public void PlayWeakRain()
    {
        GameManager.Instance.VFXController.PlayRain(rainStrength.Weak);
        PlayRainSFX();
    }

    public void PlayDecentRain()
    {
        GameManager.Instance.VFXController.PlayRain(rainStrength.Decent);
        PlayRainSFX();
    }

    public void PlayStrongRain()
    {
        GameManager.Instance.VFXController.PlayRain(rainStrength.Strong);
        PlayRainSFX();
    }

    public void StopRain()
    {
        GameManager.Instance.VFXController.StopRain();
        StopRainSFX();
    }

    #endregion

    #region Start and Stop Weather
    public void StartWeather()
    {
        if (timelineCoroutine != null) return;
        if (weatherBacklog.Count == 0) return;

        timelinePaused = false;
        UpdateWeatherImages();
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