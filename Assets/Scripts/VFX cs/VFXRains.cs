using System;
using UnityEngine;

public class VFXRains : MonoBehaviour
{
    [SerializeField] private GameObject strongRainVFX;
    [SerializeField] private GameObject decentRainVFX;
    [SerializeField] private GameObject weakRainVFX;

    private void Start() => TurnOffAllRains();

    public void TurnOffAllRains()
    {
        // Turn everything off first
        if (strongRainVFX != null) strongRainVFX.SetActive(false);
        if (decentRainVFX != null) decentRainVFX.SetActive(false);
        if (weakRainVFX != null) weakRainVFX.SetActive(false);
    }
    
    public void SetRainState(rainStrength strength)
    {
        // Turn everything off first
        TurnOffAllRains();

        // Turn on the appropriate one
        switch (strength)
        {
            case rainStrength.Strong:
                if (strongRainVFX != null) strongRainVFX.SetActive(true);
                break;
            case rainStrength.Decent:
                if (decentRainVFX != null) decentRainVFX.SetActive(true);
                break;
            case rainStrength.Weak:
                if (weakRainVFX != null) weakRainVFX.SetActive(true);
                break;
            case rainStrength.Sunny:
                // All remain inactive
                break;
        }
    }

    public void StopAllRain()
    {
        if (strongRainVFX != null) strongRainVFX.SetActive(false);
        if (decentRainVFX != null) decentRainVFX.SetActive(false);
        if (weakRainVFX != null) weakRainVFX.SetActive(false);
    }
}