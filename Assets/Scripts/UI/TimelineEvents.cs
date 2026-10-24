using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum rainStrength
{
    Weak,
    Decent,
    Strong,
    Sunny
}
public class TimelineEvents : MonoBehaviour
{
    [SerializeField] private rainStrength rainStrength;

    [Header("Weather Icon")]
    [SerializeField] private Sprite weatherSprite;

    public Sprite WeatherSprite => weatherSprite;

    // CHANGED: Removed the one-time triggered guard.
    public void TimelinePlayRain(WaterForecast waterForecast)
    {
        if (waterForecast == null)
            return;

        switch (rainStrength)
        {
            case rainStrength.Weak:
                waterForecast.PlayWeakRain();
                break;

            case rainStrength.Decent:
                waterForecast.PlayDecentRain();
                break;

            case rainStrength.Strong:
                waterForecast.PlayStrongRain();
                break;

            case rainStrength.Sunny:
                waterForecast.StopRain();
                break;

            default:
                Debug.LogWarning(
                    "[TimelineEvents] Unknown rain strength.", this);
                break;
        }
    }
}