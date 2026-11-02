using UnityEngine;

public class VFXController : MonoBehaviour
{
    [SerializeField] private VFXRains vfxRains;

    public void PlayRain(rainStrength strength)
    {
        if (vfxRains != null) 
            vfxRains.SetRainState(strength);
    }

    public void StopRain()
    {
        if (vfxRains != null) 
            vfxRains.StopAllRain();
    }
}