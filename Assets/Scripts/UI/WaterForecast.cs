using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeatherType
{
    Sunny,
    Drizzle,
    Stormy,
    Typhoon
}

[System.Serializable]
public struct WeatherDefinition
{
    [Header("Weather Settings")]
    [SerializeField] private WeatherType weatherType;
    [SerializeField] private Sprite weatherSprite;

    [Header("Spawn Settings")]
    [SerializeField, Min(0f)] private float spawnWeight;

    public WeatherType Type => weatherType;
    public Sprite Sprite => weatherSprite;
    public float SpawnWeight => spawnWeight;
}

public class WaterForecast : MonoBehaviour
{
    // Must match the child GameObject names on the SoundManager prefab.
    private const string RainSFXName = "SFXRaining"; // Drizzle
    private const string SunnySFXName = "SFXSunny";
    private const string StormSFXName = "SFXStorm";
    private const string TyphoonSFXName = "SFXTyphoon";

    [Header("Timeline Settings")]
    [SerializeField] private float eventDuration = 60f;
    public float EventDuration => eventDuration;

    [Header("Available Weather Types Pool")]
    [SerializeField] private List<WeatherDefinition> weatherDatabase = new();

    [Header("Initial Weather Backlog")]
    [SerializeField] private WeatherDefinition[] initialWeatherBacklog = new WeatherDefinition[8];

    private List<WeatherDefinition> weatherBacklog = new List<WeatherDefinition>();

    [Header("Active Weather Water Adjustment")]
    [SerializeField] private float sunnyVal;
    [SerializeField] private float drizzlVal;
    [SerializeField] private float stormyVal;
    [SerializeField] private float typhoonVal;
    private float storedWaterAdjustment; // storing previous increase in water

    [Header("Gradual Weather Water Rise")]
    [SerializeField] private float weatherRiseSpeed = 2f;

    private float currentWeatherAdjustment;
    private float targetWeatherAdjustment;

    [Header("Debug")]
    [SerializeField] private float currentTime;
    public float CurrentTime => currentTime;

    private bool timelinePaused;
    private Coroutine timelineCoroutine;

    // State caching guards to prevent redundant calls and hitching
    private rainStrength? currentActiveRainState = null;
    private bool isWeatherInitialized = false;
    private bool stoppedForGreatFlood;

    // Tracks which weather ambience loop is currently playing so transitions
    // stop exactly one source and start exactly one source.
    // null/empty = no weather audio playing.
    private string currentWeatherSFXName = null;

    private bool IsGreatFloodActive()
    {
        return GameManager.Instance != null && GameManager.Instance.WaterLevel != null && GameManager.Instance.WaterLevel.GreatFloodStarted;
    }

    private void OnEnable()
    {
        if (eventDuration <= 0f)
        {
            Debug.LogWarning("[WaterForecast] Event duration must be greater than zero.", this);
            return;
        }

        if (weatherBacklog.Count == 0)
            BuildInitialBacklog();

        if (weatherBacklog.Count == 0)
        {
            Debug.LogWarning("[WaterForecast] Weather database is empty! Please add weather definitions in the Inspector.", this);
            return;
        }
    }

    private void OnDisable() => StopWeather();

    private void Update()
    {
        if (!IsGreatFloodActive())
        {
            stoppedForGreatFlood = false;
            UpdateWeatherWaterRise();
            return;
        }

        if (!stoppedForGreatFlood)
        {
            StopWeather();
            stoppedForGreatFlood = true;

            Debug.Log("[WaterForecast] Weather stopped because the Great Flood started.");
        }
    }

    private void BuildInitialBacklog()
    {
        weatherBacklog.Clear();

        foreach (WeatherDefinition weather in initialWeatherBacklog)
        {
            weatherBacklog.Add(weather);
        }
    }

    private IEnumerator ProgressTimeline()
    {
        while (true)
        {
            if (IsGreatFloodActive())
            {
                timelineCoroutine = null;
                timelinePaused = true;

                targetWeatherAdjustment = 0f;

                if (GameManager.Instance != null)
                {
                    if (GameManager.Instance.VFXController != null)
                        GameManager.Instance.VFXController.StopRain();
                }

                StopCurrentWeatherSFX();

                currentActiveRainState = null;
                isWeatherInitialized = false;

                yield break;
            }

            currentTime = 0f;

            while (currentTime < eventDuration)
            {
                if (!timelinePaused)
                    currentTime += Time.deltaTime;
                yield return null;
            }

            // Remove the finished current weather at index 0
            if (weatherBacklog.Count > 0)
                weatherBacklog.RemoveAt(0);

            // Add a new random weather event to the end of the queue
            AddRandomWeatherToBacklog();

            if (weatherBacklog.Count == 0)
            {
                timelineCoroutine = null;
                yield break;
            }

            currentTime = 0f;
            PlayCurrentWeather();
        }
    }

    private void AddRandomWeatherToBacklog()
    {
        if (weatherDatabase.Count == 0)
            return;

        float totalWeight = 0f;

        WeatherType? previousType = null;

        if (weatherBacklog.Count > 0)
            previousType = weatherBacklog[weatherBacklog.Count - 1].Type;

        foreach (WeatherDefinition weather in weatherDatabase)
        {
            if (weather.SpawnWeight <= 0f)
                continue;

            if (previousType.HasValue &&
                weather.Type == previousType.Value)
                continue;

            totalWeight += weather.SpawnWeight;
        }

        bool excludePrevious = totalWeight > 0f;

        if (!excludePrevious)
        {
            foreach (WeatherDefinition weather in weatherDatabase)
            {
                if (weather.SpawnWeight > 0f)
                    totalWeight += weather.SpawnWeight;
            }
        }

        if (totalWeight <= 0f)
        {
            WeatherDefinition fallback =
                weatherDatabase[Random.Range(0, weatherDatabase.Count)];

            weatherBacklog.Add(fallback);
            return;
        }

        float randomWeight = Random.Range(0f, totalWeight);

        foreach (WeatherDefinition weather in weatherDatabase)
        {
            if (weather.SpawnWeight <= 0f)
                continue;

            if (excludePrevious &&
                previousType.HasValue &&
                weather.Type == previousType.Value)
                continue;

            randomWeight -= weather.SpawnWeight;

            if (randomWeight <= 0f)
            {
                weatherBacklog.Add(weather);
                return;
            }
        }

        weatherBacklog.Add(weatherDatabase[weatherDatabase.Count - 1]);
    }

    private void PlayCurrentWeather()
    {
        if (weatherBacklog.Count > 0)
            ApplyWeatherEffects(weatherBacklog[0].Type);
    }

    public WeatherDefinition? GetWeatherEvent(int index)
    {
        if (index < 0 || index >= weatherBacklog.Count) return null;
        return weatherBacklog[index];
    }

    public void PauseTimeline() => timelinePaused = true;
    public void ResumeTimeline() { if (timelineCoroutine != null) timelinePaused = false; }

    private void ApplyWeatherEffects(WeatherType weatherType)
    {
        if (GameManager.Instance == null) return;

        bool isSunny = (weatherType == WeatherType.Sunny);
        rainStrength targetStrength = rainStrength.Weak;

        if (!isSunny)
        {
            if (weatherType == WeatherType.Stormy) targetStrength = rainStrength.Decent;
            else if (weatherType == WeatherType.Typhoon) targetStrength = rainStrength.Strong;
        }

        // Optimization check: If the weather state hasn't changed, exit immediately to prevent hitching
        if (isWeatherInitialized)
        {
            if (isSunny && currentActiveRainState == null) return;
            if (!isSunny && currentActiveRainState == targetStrength) return;
        }

        // Handle VFX based on the specific WeatherType
        if (GameManager.Instance.VFXController != null)
        {
            if (isSunny)
                GameManager.Instance.VFXController.StopRain();
            else
                GameManager.Instance.VFXController.PlayRain(targetStrength);
        }

        // Handle Audio: each weather type has its own loop. Only stop the
        // previously-playing one and start the new one, so we never briefly
        // silence every loop or double-play.
        if (GameManager.Instance.SoundManager != null)
        {
            string nextSFX = GetWeatherSFXName(weatherType);

            if (nextSFX != currentWeatherSFXName)
            {
                if (!string.IsNullOrEmpty(currentWeatherSFXName))
                    GameManager.Instance.SoundManager.Stop(currentWeatherSFXName);

                if (!string.IsNullOrEmpty(nextSFX))
                    GameManager.Instance.SoundManager.Play(nextSFX, true);

                currentWeatherSFXName = nextSFX;
            }
        }

        ApplyWaterWeatherAdjustment(targetStrength, isSunny);

        currentActiveRainState = isSunny ? null : targetStrength;
        isWeatherInitialized = true;
    }

    /// <summary>
    /// Maps a weather type to the name of its looping ambience SFX child
    /// GameObject on the SoundManager prefab.
    /// </summary>
    private string GetWeatherSFXName(WeatherType weatherType)
    {
        switch (weatherType)
        {
            case WeatherType.Sunny: return SunnySFXName;
            case WeatherType.Drizzle: return RainSFXName;
            case WeatherType.Stormy: return StormSFXName;
            case WeatherType.Typhoon: return TyphoonSFXName;
            default: return null;
        }
    }

    /// <summary>
    /// Stops whichever weather ambience loop is currently playing and
    /// clears the tracking field. Safe to call even when nothing is playing.
    /// </summary>
    private void StopCurrentWeatherSFX()
    {
        if (string.IsNullOrEmpty(currentWeatherSFXName))
            return;

        if (GameManager.Instance != null && GameManager.Instance.SoundManager != null)
            GameManager.Instance.SoundManager.Stop(currentWeatherSFXName);

        currentWeatherSFXName = null;
    }

    private void AdjustWaterLevel(float amount)
    {
        if (GameManager.Instance == null ||
        GameManager.Instance.WaterLevel == null)
            return;

        GameManager.Instance.WaterLevel.AdjustWaterLevel(amount);
    }

    private void ApplyWaterWeatherAdjustment(rainStrength targetStrength, bool isSunny)
    {
        targetWeatherAdjustment = isSunny
            ? 0f
            : GetWaterAdjustment(targetStrength);
    }

    private float GetWaterAdjustment(rainStrength rainStr)
    {
        switch (rainStr)
        {
            case rainStrength.Weak:
                return drizzlVal;

            case rainStrength.Decent:
                return stormyVal;

            case rainStrength.Strong:
                return typhoonVal;

            case rainStrength.Sunny:
                return sunnyVal;

            default:
                return 0;
        }
    }

    private void UpdateWeatherWaterRise()
    {
        if (GameManager.Instance == null ||
        GameManager.Instance.WaterLevel == null)
            return;

        // Calculate weather contribution after infrastructure reduction.
        float adjustedTarget =
            GameManager.Instance.WaterLevel.GetAdjustedWeatherTarget(
                targetWeatherAdjustment
            );

        if (Mathf.Approximately(currentWeatherAdjustment, adjustedTarget))
            return;

        float previousAdjustment = currentWeatherAdjustment;

        currentWeatherAdjustment = Mathf.MoveTowards(
            currentWeatherAdjustment,
            adjustedTarget,
            weatherRiseSpeed * Time.deltaTime
        );

        float adjustmentDifference =
            currentWeatherAdjustment - previousAdjustment;

        AdjustWaterLevel(adjustmentDifference);
    }

    #region Start and Stop Weather
    public void StartWeather()
    {
        if (IsGreatFloodActive())
        {
            Debug.Log("[WaterForecast] Cannot start weather during the Great Flood.");
            return;
        }

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

        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.VFXController != null)
                GameManager.Instance.VFXController.StopRain();
        }

        StopCurrentWeatherSFX();

        // Gradually remove the existing weather contribution.
        targetWeatherAdjustment = 0f;

        currentActiveRainState = null;
        isWeatherInitialized = false;
    }
    #endregion

    public WeatherType CurrentActiveWeather => IsGreatFloodActive() ? WeatherType.Sunny : weatherBacklog.Count > 0 ? weatherBacklog[0].Type : WeatherType.Sunny;
    public float SunnyVal => sunnyVal;
    public float DrizzleVal => drizzlVal;
    public float StormyVal => stormyVal;
    public float TyphoonVal => typhoonVal;
}