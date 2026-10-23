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

    private bool triggered;

    private void Start()
    {
        triggered = false;
    }

    public void TimelinePlayRain(WaterForecast waterForecast)
    {
        if (triggered)
            return;

        triggered = true;

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
                Debug.LogWarning("[TimelineEvents] Unknown rain strength.", this);
                break;
        }
    }
}
