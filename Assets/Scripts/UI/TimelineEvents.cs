using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimelineEvents : MonoBehaviour
{
    [SerializeField] private float heightValue;
    [SerializeField] private float waterIncreaseVal;
    [SerializeField] private float heightIncreaseDuration = 2f;

    private bool triggered;

    private void Start()
    {
        triggered = false;
    }

    public void IncreaseWaterLevel(WaterForecast waterForecast)
    {
        if (triggered)
            return;

        triggered = true;

       // waterForecast.ChangeMaximumWater(waterIncreaseVal);
    }
}
