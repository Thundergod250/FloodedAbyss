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

    [Header("Debug")]
    [SerializeField] private float currentTime;
    public float CurrentTime => currentTime;

    private bool timelinePaused;
    private Coroutine timelineCoroutine;

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

        StartWeather();
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
            chosenWeather = weatherDatabase[Random.Range(0, weatherDatabase.Count)];

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

        // Handle VFX based on the specific WeatherType
        if (GameManager.Instance.VFXController != null)
        {
            if (weatherType == WeatherType.Sunny)
                GameManager.Instance.VFXController.StopRain();
            else
            {
                rainStrength mappedStrength = rainStrength.Weak;
                
                if (weatherType == WeatherType.Stormy) mappedStrength = rainStrength.Decent;
                else if (weatherType == WeatherType.Typhoon) mappedStrength = rainStrength.Strong;

                GameManager.Instance.VFXController.PlayRain(mappedStrength);
            }
        }

        // Handle Audio
        if (GameManager.Instance.SoundManager != null)
        {
            if (weatherType == WeatherType.Sunny)
                GameManager.Instance.SoundManager.Stop("SFXRaining");
            else
                GameManager.Instance.SoundManager.Play("SFXRaining");
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
        
        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.VFXController != null)
                GameManager.Instance.VFXController.StopRain();
            if (GameManager.Instance.SoundManager != null)
                GameManager.Instance.SoundManager.Stop("SFXRaining");
        }
    }
    #endregion
}