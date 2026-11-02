using UnityEngine;

public class VFXRains : MonoBehaviour
{
    [Header("Particle Systems")]
    [SerializeField] private ParticleSystem strongRainVFX;
    [SerializeField] private ParticleSystem decentRainVFX;
    [SerializeField] private ParticleSystem weakRainVFX;

    private void Start()
    {
        StopAllRain();
    }

    public void SetRainState(rainStrength strength)
    {
        // Gracefully stop whatever is currently playing before switching
        StopAllRain();

        // Play the newly requested rain type
        switch (strength)
        {
            case rainStrength.Strong:
                if (strongRainVFX != null) strongRainVFX.Play();
                break;
            case rainStrength.Decent:
                if (decentRainVFX != null) decentRainVFX.Play();
                break;
            case rainStrength.Weak:
                if (weakRainVFX != null) weakRainVFX.Play();
                break;
            case rainStrength.Sunny:
                // All remain stopped/fading out
                break;
        }
    }

    public void StopAllRain()
    {
        // StopEmitting allows current particles to fall/fade naturally instead of popping out
        if (strongRainVFX != null) strongRainVFX.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        if (decentRainVFX != null) decentRainVFX.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        if (weakRainVFX != null) weakRainVFX.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }
}