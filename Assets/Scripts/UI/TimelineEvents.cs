using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimelineEvents : MonoBehaviour
{
    [SerializeField] private float heightValue;
    [SerializeField] private float heightIncreaseDuration = 2f;

    private bool triggered;

    private void Start()
    {
        triggered = false;
    }

    public void IncreaseWaterLevel(Transform ocean)
    {
        if (triggered)
            return;

        StartCoroutine(IncreaseWaterLevelOverTime(ocean));
    }

    private IEnumerator IncreaseWaterLevelOverTime(Transform ocean)
    {
        triggered = true;

        float elapsed = 0f;
        float startHeight = ocean.position.y;
        float targetHeight = startHeight + heightValue;

        while (elapsed < heightIncreaseDuration)
        {
            elapsed += Time.deltaTime;

            float progress = elapsed / heightIncreaseDuration;

            Vector3 position = ocean.position;
            position.y = Mathf.Lerp(startHeight, targetHeight,progress);

            ocean.position = position;

            yield return null;
        }

        ocean.position = new Vector3(
            ocean.position.x,
            targetHeight,
            ocean.position.z
        );
    }
}
