using System;
using System.Collections;
using UnityEngine;

public class TideManager : MonoBehaviour
{
    public static TideManager Instance { get; private set; }

    [Header("Ocean Reference")]
    [SerializeField] private Transform oceanTransform;

    [Header("Tide Settings")]
    [Tooltip("Default water height (low tide). If <= 0, captures the ocean's starting Y.")]
    [SerializeField] private float lowTideHeight = 0f;

    [Tooltip("Height added during high tide. Editable per spec (default 50).")]
    [SerializeField] private float highTideOffset = 50f;

    [Tooltip("Fixed time (seconds) the tide stays in one state before flipping.")]
    [SerializeField] private float tideInterval = 60f;

    [Tooltip("0 = instant snap. >0 = smooth transition (spec says instant, default 0).")]
    [SerializeField] private float tideTransitionDuration = 0f;

    [Tooltip("If true, the game starts in high tide instead of low tide.")]
    [SerializeField] private bool startAtHighTide = false;

    [Header("Debug Status")]
    [SerializeField] private bool isHighTide = false;
    [SerializeField] private float currentWaterHeight = 0f;

    private Coroutine tideCycleRoutine;
    private Coroutine transitionRoutine;

    public event Action<bool> OnTideChanged;
    public event Action OnHighTideStarted;
    public event Action OnLowTideStarted;

    public bool IsHighTide => isHighTide;
    public float CurrentWaterHeight => currentWaterHeight;
    public float LowTideHeight => lowTideHeight;
    public float HighTideHeight => lowTideHeight + highTideOffset;
    public float CurrentTideOffset => isHighTide ? highTideOffset : 0f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (oceanTransform == null) oceanTransform = transform;

        // Capture default low-tide height from the ocean's starting position
        if (lowTideHeight <= 0f)
            lowTideHeight = oceanTransform.position.y;

        isHighTide = startAtHighTide;
        currentWaterHeight = isHighTide ? lowTideHeight + highTideOffset : lowTideHeight;
    }

    private void Start()
    {
        ApplyHeightImmediate(currentWaterHeight);
        tideCycleRoutine = StartCoroutine(TideCycleRoutine());
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private IEnumerator TideCycleRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(tideInterval);

            if (isHighTide) GoLowTide();
            else GoHighTide();
        }
    }

    public void GoHighTide()
    {
        if (isHighTide) return;
        SetTideState(true);
    }

    public void GoLowTide()
    {
        if (!isHighTide) return;
        SetTideState(false);
    }

    private void SetTideState(bool high)
    {
        isHighTide = high;
        float target = high ? lowTideHeight + highTideOffset : lowTideHeight;

        if (transitionRoutine != null) StopCoroutine(transitionRoutine);
        transitionRoutine = StartCoroutine(TransitionToHeightRoutine(target));

        OnTideChanged?.Invoke(high);
        if (high) OnHighTideStarted?.Invoke();
        else OnLowTideStarted?.Invoke();
    }

    private IEnumerator TransitionToHeightRoutine(float targetHeight)
    {
        // Instant snap (per spec)
        if (tideTransitionDuration <= 0f)
        {
            ApplyHeightImmediate(targetHeight);
            transitionRoutine = null;
            yield break;
        }

        float start = currentWaterHeight;
        float elapsed = 0f;

        while (elapsed < tideTransitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / tideTransitionDuration);
            currentWaterHeight = Mathf.Lerp(start, targetHeight, t);
            ApplyOceanHeight(currentWaterHeight);
            yield return null;
        }

        ApplyHeightImmediate(targetHeight);
        transitionRoutine = null;
    }

    private void ApplyHeightImmediate(float height)
    {
        currentWaterHeight = height;
        ApplyOceanHeight(height);
    }

    private void ApplyOceanHeight(float height)
    {
        if (oceanTransform == null) return;
        Vector3 pos = oceanTransform.position;
        pos.y = height;
        oceanTransform.position = pos;
    }
}