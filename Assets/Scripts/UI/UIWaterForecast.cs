using UnityEngine;
using UnityEngine.UI;

public class UIWaterForecast : MonoBehaviour
{
    [SerializeField] private WaterForecast waterForecast;
    
    [Header("UI Sliders")]
    [SerializeField] private Slider weatherTimerSlider;
    [SerializeField] private Slider waterFill; 

    [Header("Weather UI Images")]
    [SerializeField] private Image currentWeatherImage;
    [SerializeField] private Image nextWeatherImage;
    [SerializeField] private Image followingWeatherImage;
    [SerializeField] private Image followingWeather2Image;

    private void Update()
    {
        if (waterForecast == null) return;
        if (weatherTimerSlider != null)
        {
            weatherTimerSlider.maxValue = waterForecast.EventDuration;
            weatherTimerSlider.value = waterForecast.CurrentTime;
        }
        
        if (waterFill != null && GameManager.Instance != null && GameManager.Instance.WaterLevel != null) 
            waterFill.value = GameManager.Instance.WaterLevel.CurrentWaterHeight;
        
        UpdateWeatherImages();
    }

    private void UpdateWeatherImages()
    {
        SetWeatherImage(currentWeatherImage, waterForecast.GetWeatherEvent(0));
        SetWeatherImage(nextWeatherImage, waterForecast.GetWeatherEvent(1));
        SetWeatherImage(followingWeatherImage, waterForecast.GetWeatherEvent(2));
        SetWeatherImage(followingWeather2Image, waterForecast.GetWeatherEvent(3));
    }

    private void SetWeatherImage(Image targetImage, TimelineEvents weatherEvent)
    {
        if (targetImage == null) return;

        if (weatherEvent == null || weatherEvent.WeatherSprite == null)
        {
            targetImage.sprite = null;
            targetImage.enabled = false;
            return;
        }

        Sprite weatherSprite = weatherEvent.WeatherSprite;
        targetImage.sprite = weatherSprite;
        targetImage.enabled = weatherSprite != null;
    }
}