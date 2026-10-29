using System;
using UnityEngine;
using TMPro;
using System.Collections;
using Object = UnityEngine.Object;

public class UINotification : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI notificationText;
    
    [Header("Warning Settings")]
    [SerializeField] private float lowFuelThreshold = 50f;
    [SerializeField] private float checkInterval = 1f; // Editable check interval in seconds
    [SerializeField] private Color warningColor = Color.red;
    [SerializeField] private float pulseSpeed = 4f;
    [SerializeField] private float minScale = 1f;
    [SerializeField] private float maxScale = 1.2f;
    
    private StructureApexPump targetApexPump;
    private RectTransform rectTransform;
    private bool isLowFuel = false;
    private Coroutine checkRoutine;

    private void Awake()
    {
        if (notificationText != null) 
            rectTransform = notificationText.gameObject.GetComponent<RectTransform>();
        SetNotificationState(false);
    }

    private void OnEnable()
    {
        if (checkRoutine == null)
            checkRoutine = StartCoroutine(CheckFuelRoutine());
    }

    private void OnDisable()
    {
        if (checkRoutine != null)
        {
            StopCoroutine(checkRoutine);
            checkRoutine = null;
        }
    }

    private IEnumerator CheckFuelRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(checkInterval);

        // Give GameManager a brief moment to initialize if needed
        yield return null; 

        while (true)
        {
            // Safely resolve targetApexPump handling Unity's fake nulls
            if (!targetApexPump)
            {
                if (GameManager.Instance != null && GameManager.Instance.ApexPump != null)
                    targetApexPump = GameManager.Instance.ApexPump;
            }

            // Use Unity's overloaded boolean operator to safely check if the component is truly alive
            if (targetApexPump)
            {
                isLowFuel = targetApexPump.CurrentFuel < lowFuelThreshold;

                if (isLowFuel && !notificationText.gameObject.activeSelf)
                    SetNotificationState(true);
                else if (!isLowFuel && notificationText.gameObject.activeSelf) 
                    SetNotificationState(false);
            }

            yield return wait;
        }
    }

    private void Update()
    {
        // Update runs strictly for smooth scaling animation when the warning is active
        if (isLowFuel && notificationText != null && notificationText.gameObject.activeSelf && rectTransform != null)
        {
            float scale = Mathf.Lerp(minScale, maxScale, (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f);
            rectTransform.localScale = new Vector3(scale, scale, 1f);
        }
    }

    private void SetNotificationState(bool active)
    {
        if (notificationText != null)
        {
            notificationText.gameObject.SetActive(active);
            if (active)
            {
                notificationText.color = warningColor;
                notificationText.text = "WARNING: PUMP LOW FUEL";
            }
            else
            {
                if (rectTransform != null)
                    rectTransform.localScale = Vector3.one;
            }
        }
    }
}