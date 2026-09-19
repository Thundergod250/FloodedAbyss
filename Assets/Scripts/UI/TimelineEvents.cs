using UnityEngine;

public class TimelineEvents : MonoBehaviour
{
    [SerializeField] private float heightValue;

    private bool triggered;
    private void Start()
    {
        triggered = false;
    }

    public void IncreaseWaterLevel(Transform ocean)
    {
        if(!triggered)
        {
            ocean.position += Vector3.up * heightValue;
            triggered = true;
        }
    }
}
