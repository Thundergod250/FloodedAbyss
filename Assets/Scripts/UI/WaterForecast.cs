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

    [Header("Active Weather Queue (Backlog)")]
    [SerializeField] private List<WeatherDefinition> weatherBacklog = new();
    [SerializeField] private int initialBacklogCount = 4;

    [Header("Active Weather Water Adjustment")]
    [SerializeField] private float sunnyVal;
    [SerializeField] private float drizzlVal;
    [SerializeField] private float stormyVal;
    [SerializeField] private float typhoonVal;
    private float storedWaterAdjustment; // storing previous increase in water

    [Header("Weather Strength Scaling")]
    [SerializeField] private float strengthIncreasePerSwitch = 0.05f;

    private int lastActiveSwitchCount = -1;
    [Header("Debug")]
    [SerializeField] private float currentTime;
    public float CurrentTime => currentTime;

    private bool timelinePaused;
    private Coroutine timelineCoroutine;

    // State caching guards to prevent redundant calls and hitching
    private rainStrength? currentActiveRainState = null;
    private bool isWeatherInitialized = false;

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

        //StartWeather();
    }

    private void OnDisable() => StopWeather();

    private void BuildInitialBacklog()
    {
        weatherBacklog.Clear();
        if (weatherDatabase.Count == 0) return;

        // 1. Always force the very first weather event to be Sunny
        WeatherDefinition? sunnyDef = GetSunnyDefinition();
        if (sunnyDef.HasValue)
            weatherBacklog.Add(sunnyDef.Value);
        else
        {
            Debug.LogWarning("[WaterForecast] 'Sunny' weather type not found in database! Using database entry 0 as fallback.");
            weatherBacklog.Add(weatherDatabase[0]);
        }

        // 2. Fill the remaining slots of the initial count with randomized weather
        for (int i = 1; i < initialBacklogCount; i++) 
            AddRandomWeatherToBacklog();
    }
    private void Update()
    {
        int activeSwitches = GetTotalActiveSwitches();

        if (activeSwitches != lastActiveSwitchCount)
        {
            lastActiveSwitchCount = activeSwitches;

            if (isWeatherInitialized)
            {
                bool isSunny = currentActiveRainState == null;

                ApplyWaterWeatherAdjustment(currentActiveRainState ?? rainStrength.Weak,isSunny);
            }
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
        if (amount >= 0) // positive
        {
            GameManager.Instance.WaterLevel.IncWaterPressure(amount);
        }
        else if (amount < 0)
        {
            GameManager.Instance.WaterLevel.ReduceWaterPressure(Mathf.Abs(amount));
        }
    }

    private void ApplyWaterWeatherAdjustment(rainStrength targetStrength, bool isSunny)
    {
        AdjustWaterLevel(-storedWaterAdjustment);
        storedWaterAdjustment = 0f;

        if (!isSunny)
        {
            float baseAdjustment = GetWaterAdjustment(targetStrength);
            float multiplier = GetWeatherMultiplier();

            float newAdjustment = baseAdjustment * multiplier;

            AdjustWaterLevel(newAdjustment);
            storedWaterAdjustment = newAdjustment;
        }
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

    public float GetWeatherMultiplier()
    {
        return 1f + (GetTotalActiveSwitches() * strengthIncreasePerSwitch);
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
        
        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.VFXController != null)
                GameManager.Instance.VFXController.StopRain();
            if (GameManager.Instance.SoundManager != null)
                GameManager.Instance.SoundManager.Stop("SFXRaining");
        }
    }
    #endregion

    public WeatherType CurrentActiveWeather => weatherBacklog.Count > 0 ? weatherBacklog[0].Type : WeatherType.Sunny;
    public float SunnyVal => sunnyVal;
    public float DrizzleVal => drizzlVal;
    public float StormyVal => stormyVal;
    public float TyphoonVal => typhoonVal;


}