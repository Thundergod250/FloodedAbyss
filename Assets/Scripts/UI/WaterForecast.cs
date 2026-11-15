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

    public WeatherType Type => weatherType;
    public Sprite Sprite => weatherSprite;
}

public class WaterForecast : MonoBehaviour
{
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

    private WeatherDefinition? GetSunnyDefinition()
    {
        foreach (var def in weatherDatabase)
        {
            if (def.Type == WeatherType.Sunny)
                return def;
        }
        return null;
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

                    if (GameManager.Instance.SoundManager != null)
                        GameManager.Instance.SoundManager.Stop("SFXRaining");
                }

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
        if (weatherDatabase.Count == 0) return;

        WeatherDefinition chosenWeather;

        // Prevent instant back-to-back duplicate weather types if possible
        if (weatherDatabase.Count > 1 && weatherBacklog.Count > 0)
        {
            WeatherDefinition lastWeather = weatherBacklog[weatherBacklog.Count - 1];
            int safety = 0;
            do
            {
                chosenWeather = weatherDatabase[Random.Range(0, weatherDatabase.Count)];
                safety++;
            }
            while (chosenWeather.Type == lastWeather.Type && safety < 10);
        }
        else
        {
            chosenWeather = weatherDatabase[Random.Range(0, weatherDatabase.Count)];
        }

        weatherBacklog.Add(chosenWeather);
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

        // Handle Audio (Passed true for isLooped so rain ambiance loops seamlessly without restarting instances)
        if (GameManager.Instance.SoundManager != null)
        {
            if (isSunny)
            {
                GameManager.Instance.SoundManager.Stop("SFXRaining");
            }
            else
            {
                GameManager.Instance.SoundManager.Play("SFXRaining", true);
            }
        }

        ApplyWaterWeatherAdjustment(targetStrength, isSunny);

        currentActiveRainState = isSunny ? null : targetStrength;
        isWeatherInitialized = true;
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
        if (isSunny)
        {
            targetWeatherAdjustment = 0f;
            return;
        }

        targetWeatherAdjustment = GetWaterAdjustment(targetStrength);
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

    private int GetTotalActiveSwitches()
    {
        if (GameManager.Instance == null || GameManager.Instance.ApexPump == null)
            return 0;

        return GameManager.Instance.ApexPump.ActiveSwitchCount;
    }

    private WeatherDefinition? GetWeatherDefinition(WeatherType type)
    {
        foreach (var def in weatherDatabase)
        {
            if (def.Type == type)
                return def;
        }

        return null;
    }

    private void UpdateWeatherWaterRise()
    {
        if (Mathf.Approximately(currentWeatherAdjustment, targetWeatherAdjustment))
            return;

        float previousAdjustment = currentWeatherAdjustment;

        currentWeatherAdjustment = Mathf.MoveTowards(
            currentWeatherAdjustment,
            targetWeatherAdjustment,
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

            if (GameManager.Instance.SoundManager != null)
                GameManager.Instance.SoundManager.Stop("SFXRaining");
        }

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