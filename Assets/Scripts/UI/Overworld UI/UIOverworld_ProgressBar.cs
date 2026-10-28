using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIOverworld_ProgressBar : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private float updateInterval = 0.25f;

    private CanvasGroup canvasGroup;
    private StructureHarvestResource harvestResource;
    private Coroutine updateRoutine;

    private void Awake()
    {
        // Automatically find the StructureHarvestResource on parent objects if not assigned
        if (harvestResource == null) 
            harvestResource = GetComponentInParent<StructureHarvestResource>();

        // Automatically grab CanvasGroup if not assigned
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        // If fillImage isn't assigned, try to find it on children (e.g. Actual Fill)
        if (fillImage == null)
        {
            foreach (var img in GetComponentsInChildren<Image>(true))
            {
                if (img.gameObject != gameObject)
                {
                    fillImage = img;
                    break;
                }
            }
        }

        // Ensure the progress bar starts hidden via CanvasGroup
        SetVisible(false);
    }

    private void OnEnable()
    {
        if (updateRoutine == null) 
            updateRoutine = StartCoroutine(UpdateProgressRoutine());
    }

    private void OnDisable()
    {
        if (updateRoutine != null)
        {
            StopCoroutine(updateRoutine);
            updateRoutine = null;
        }
    }

    private IEnumerator UpdateProgressRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(updateInterval);

        while (true)
        {
            if (harvestResource != null)
            {
                // Check if the resource is currently working (harvesting) or being destroyed
                bool isActive = harvestResource.IsWorking || harvestResource.IsDestroying;

                if (isActive)
                {
                    // Show UI via CanvasGroup
                    if (canvasGroup != null && canvasGroup.alpha == 0) 
                        SetVisible(true);

                    // Update fill amount using ProgressPercentage from StructureHarvestResource
                    if (fillImage != null) 
                        fillImage.fillAmount = harvestResource.ProgressPercentage;
                }
                else
                {
                    // Hide UI via CanvasGroup when idle
                    if (canvasGroup != null && canvasGroup.alpha > 0) 
                        SetVisible(false);
                }
            }

            yield return wait;
        }
    }

    private void SetVisible(bool visible)
    {
        if (canvasGroup == null) return;
        canvasGroup.alpha = visible ? 1f : 0f;
        canvasGroup.interactable = visible;
        canvasGroup.blocksRaycasts = visible;
    }
}