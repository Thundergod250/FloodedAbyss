using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIOverworld_ResourcePair : MonoBehaviour
{
    public Image resourceImage;
    public TextMeshProUGUI resourceText;
    public ResourceTooltipTrigger resourceTooltipTrigger;

    [Header("Floating Pop Settings")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float floatDistance = 3f;
    [SerializeField] private float duration = 1.1f;

    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
    }

    public void SetupAndFloat(ResourceDataSO resourceData, int amount, Vector3 worldStartPosition)
    {
        transform.position = worldStartPosition;

        if (resourceImage != null && resourceData != null)
            resourceImage.sprite = resourceData.Icon;

        if (resourceText != null)
            resourceText.text = $"+{amount}";

        if (resourceTooltipTrigger != null && resourceData != null)
            resourceTooltipTrigger.SetResource(resourceData);

        StartCoroutine(FloatAndFadeRoutine());
    }

    private IEnumerator FloatAndFadeRoutine()
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + Vector3.up * floatDistance;

        if (canvasGroup != null)
            canvasGroup.alpha = 1f;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            // Smooth ease-out
            float easedT = 1f - Mathf.Pow(1f - t, 2f);

            transform.position = Vector3.Lerp(startPos, endPos, easedT);

            if (canvasGroup != null)
                canvasGroup.alpha = 1f - t;

            yield return null;
        }

        Pool.Destroy(gameObject);
    }
}