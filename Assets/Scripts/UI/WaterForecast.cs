using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class WaterForecast : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image currentTimeline;
    [SerializeField] private Image timeline;
    [SerializeField] private TextMeshProUGUI waterLevelText;

    [Header("Timer")]
    [SerializeField] private float timer;

    [Header("Breakpoints")]
    [SerializeField] private List<Image> eventpoints = new List<Image>();

    private WaterLevel level;

    private void Start()
    {
        level = GameManager.Instance.waterLevel;

        StartCoroutine(ProgressTimeline());
    }

    private void Update()
    {
        waterLevelText.text = level.waterLevelTransform.transform.position.y.ToString() + "m";
    }

    private IEnumerator ProgressTimeline()
    {
        float elapsed = 0f;

        while (elapsed < timer)
        {
            elapsed += Time.deltaTime;

            float progress = elapsed / timer;

            RectTransform timelineRect = timeline.rectTransform;
            RectTransform currentRect = currentTimeline.rectTransform;

            float startY = 0f;
            float endY = timelineRect.rect.height;

            Vector2 position = currentRect.anchoredPosition;
            position.y = Mathf.Lerp(startY, endY, progress);

            currentRect.anchoredPosition = position;

            CheckBreakpoints();

            yield return null;
        }
    }

    private void CheckBreakpoints()
    {
        foreach (Image eventpoint in eventpoints)
        {
            if (Mathf.Abs(currentTimeline.rectTransform.anchoredPosition.y - eventpoint.rectTransform.anchoredPosition.y) < 1f) // if distance is below 1
            {
                Debug.Log("Current timeline reached breakpoint: " + eventpoint.name);

                eventpoint.GetComponent<TimelineEvents>().IncreaseWaterLevel(level.waterLevelTransform);
            }
        }
    }

    public void IncDecWaterLevel(float height)
    {
        level.waterLevelTransform.transform.position = new Vector3(level.waterLevelTransform.transform.position.x, height, level.waterLevelTransform.transform.position.z);
    }
}