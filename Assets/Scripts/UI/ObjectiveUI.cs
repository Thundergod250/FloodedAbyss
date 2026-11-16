using TMPro;
using UnityEngine;

public class ObjectiveUI : MonoBehaviour
{
    // ── SFX names (must match the SoundManager entry / child GameObject) ──
    private const string LowFuelSFXName = "SFX_LowFuel";

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI objectiveNameText;
    [SerializeField] private TextMeshProUGUI GreatFloodText;
    [SerializeField] private TextMeshProUGUI objectiveDescriptionText;

    [Header("Pump Timer")]
    [SerializeField] private TextMeshProUGUI pumpTimerText;

    [Header("Checklist References")]
    [SerializeField] private Transform checklistParent;
    [SerializeField] private UIObjectiveChecklist checklistItemPrefab;

    [Header("Timer Danger / Flicker")]
    [SerializeField] private float greatFloodFlickerThreshold = 60f; // 1 minute
    [SerializeField] private float pumpFlickerThreshold = 20f;       // 20 seconds
    [SerializeField] private float flickerSpeed = 5f;                // higher = faster flicker
    [SerializeField] private Color dangerColor = Color.red;

    [Header("Low Fuel Beep Tiers (Fuel Remaining)")]
    [Tooltip("Fuel at or below this value starts the first (very slow) beep tier. Above this = silent.")]
    [SerializeField] private float beepTier1Threshold = 100f;
    [Tooltip("Fuel at or below this value switches to the second (moderate) beep tier.")]
    [SerializeField] private float beepTier2Threshold = 50f;
    [Tooltip("Fuel at or below this value switches to the third (very fast) beep tier. Fuel of 0 is silent (pump empty).")]
    [SerializeField] private float beepTier3Threshold = 9f;

    [Tooltip("Seconds between beeps in Tier 1 (very slow, 100–51 fuel).")]
    [SerializeField] private float beepIntervalTier1 = 2.0f;
    [Tooltip("Seconds between beeps in Tier 2 (moderate, 50–10 fuel).")]
    [SerializeField] private float beepIntervalTier2 = 0.8f;
    [Tooltip("Seconds between beeps in Tier 3 (very fast, 9–1 fuel).")]
    [SerializeField] private float beepIntervalTier3 = 0.25f;

    private ObjectiveManager subscribedManager;

    // Cached original colors so we can restore them when the timer is safe
    private Color greatFloodOriginalColor = Color.white;
    private Color pumpOriginalColor = Color.white;

    // Low-fuel beep state: current tier (0 = silent) and a countdown timer
    // that determines when the next beep fires.
    private int currentFuelBeepTier = 0;
    private float lowFuelBeepTimer = 0f;

    // Optional externally-set pump time. If left at 0, the script falls back
    // to reading the value straight off pumpTimerText.
    public float PumpTimeRemaining { get; set; } = 0f;

    private void Awake()
    {
        if (GreatFloodText != null)
            greatFloodOriginalColor = GreatFloodText.color;

        if (pumpTimerText != null)
            pumpOriginalColor = pumpTimerText.color;
    }

    private void OnEnable()
    {
        TrySubscribe();
        RefreshUI();
    }

    private void OnDisable()
    {
        Unsubscribe();

        // Reset low-fuel beep state. Any in-flight one-shot finishes
        // naturally; no new beeps are scheduled.
        currentFuelBeepTier = 0;
        lowFuelBeepTimer = 0f;
    }

    private void Update()
    {
        if (subscribedManager == null && ObjectiveManager.Instance != null)
        {
            TrySubscribe();
            RefreshUI();
        }

        // --- Great Flood timer (visuals only) ---
        if (GameManager.Instance != null && GameManager.Instance.WaterLevel != null && GreatFloodText != null)
        {
            WaterLevel wl = GameManager.Instance.WaterLevel;

            float remainingTime;

            if (!wl.GreatFloodStarted)
                remainingTime = wl.TimeUntilGreatFlood;
            else if (wl.GreatFloodStarted)
                remainingTime = wl.GreatFloodTimeRemaining;
            else
                remainingTime = 0f;

            int totalSeconds = Mathf.CeilToInt(Mathf.Max(0f, remainingTime));
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;

            GreatFloodText.text = $"{minutes}:{seconds:D2}";

            ApplyTimerVisuals(
                GreatFloodText,
                remainingTime,
                greatFloodOriginalColor,
                greatFloodFlickerThreshold
            );
        }

        // --- Pump timer + low-fuel beeping ---
        float pumpRemaining = GetPumpRemainingSeconds();

        if (pumpTimerText != null)
        {
            ApplyTimerVisuals(
                pumpTimerText,
                pumpRemaining,
                pumpOriginalColor,
                pumpFlickerThreshold
            );
        }

        UpdateLowFuelBeep(GetFuelBeepTier(pumpRemaining));
    }

    /// <summary>
    /// Optional setter if another script wants to drive the pump timer directly.
    /// Passing a value here takes precedence over reading the text.
    /// </summary>
    public void SetPumpTimeRemaining(float seconds)
    {
        PumpTimeRemaining = seconds;

        if (pumpTimerText != null)
            pumpTimerText.text = Mathf.CeilToInt(Mathf.Max(0f, seconds)).ToString();
    }

    /// <summary>
    /// Returns the pump time / fuel value. Prefers an externally-set
    /// <see cref="PumpTimeRemaining"/>; otherwise parses whatever is currently
    /// shown in <see cref="pumpTimerText"/> ("20", "0:20", "00:20").
    /// </summary>
    private float GetPumpRemainingSeconds()
    {
        if (PumpTimeRemaining > 0f)
            return PumpTimeRemaining;

        if (pumpTimerText == null)
            return 0f;

        string s = pumpTimerText.text?.Trim();
        if (string.IsNullOrEmpty(s))
            return 0f;

        if (s.Contains(":"))
        {
            string[] parts = s.Split(':');
            if (parts.Length == 2 &&
                int.TryParse(parts[0], out int m) &&
                int.TryParse(parts[1], out int sec))
                return m * 60 + sec;
        }

        if (float.TryParse(s, out float v))
            return v;

        return 0f;
    }

    /// <summary>
    /// True while the pump is running (at least one active switch).
    /// Falls back to <c>true</c> if ApexPump isn't wired up, so the beep
    /// system still works in scenes that don't use the pump.
    /// </summary>
    private bool IsPumpActive()
    {
        if (GameManager.Instance == null || GameManager.Instance.ApexPump == null)
            return true; // can't detect → assume pump may be running

        return GameManager.Instance.ApexPump.ActiveSwitchCount > 0;
    }

    /// <summary>
    /// Shared danger predicate used by the visual flicker on both timers.
    /// </summary>
    private static bool IsInDanger(float remainingTime, float threshold)
    {
        return remainingTime > 0f && remainingTime <= threshold;
    }

    /// <summary>
    /// Returns the current low-fuel beep tier:
    ///   0 = silent (pump off, fuel 0, or fuel > 100)
    ///   1 = very slow (fuel 51–100)
    ///   2 = moderate  (fuel 10–50)
    ///   3 = very fast (fuel 1–9)
    /// </summary>
    private int GetFuelBeepTier(float fuel)
    {
        // Pump off → no beeping at all, regardless of fuel value.
        if (!IsPumpActive()) return 0;

        // Empty tank → silent (pump is off / out of fuel).
        if (fuel <= 0f) return 0;

        if (fuel <= beepTier3Threshold) return 3; // ≤ 9
        if (fuel <= beepTier2Threshold) return 2; // ≤ 50
        if (fuel <= beepTier1Threshold) return 1; // ≤ 100
        return 0;                                  // > 100
    }

    /// <summary>
    /// Plays a one-shot low-fuel beep on a tier-dependent cadence.
    /// Fires immediately whenever the tier changes so crossing 100/50/9
    /// is heard right away, then respects the new tier's interval.
    /// When the tier drops to 0 (pump off, empty, or full), no further
    /// beeps are scheduled — any currently-playing beep finishes naturally.
    /// </summary>
    private void UpdateLowFuelBeep(int tier)
    {
        // On a tier change, fire a beep this frame and reset the timer.
        if (tier != currentFuelBeepTier)
        {
            currentFuelBeepTier = tier;
            lowFuelBeepTimer = 0f;
        }

        if (tier <= 0)
            return; // silent

        lowFuelBeepTimer -= Time.deltaTime;
        if (lowFuelBeepTimer > 0f)
            return;

        var sm = GameManager.Instance != null ? GameManager.Instance.SoundManager : null;
        if (sm != null)
            sm.Play(LowFuelSFXName); // one-shot

        lowFuelBeepTimer = GetBeepInterval(tier);
    }

    private float GetBeepInterval(int tier)
    {
        switch (tier)
        {
            case 1: return beepIntervalTier1;
            case 2: return beepIntervalTier2;
            case 3: return beepIntervalTier3;
            default: return float.MaxValue;
        }
    }

    /// <summary>
    /// Turns the text red and flickers it while 0 &lt; remainingTime &lt;= threshold.
    /// Otherwise restores the original color at full opacity.
    /// </summary>
    private void ApplyTimerVisuals(TextMeshProUGUI text, float remainingTime, Color originalColor, float threshold)
    {
        if (text == null) return;

        bool isInDanger = IsInDanger(remainingTime, threshold);

        if (isInDanger)
        {
            float pingPong = Mathf.PingPong(Time.time * flickerSpeed, 1f);
            float alpha = Mathf.Lerp(0.2f, 1f, pingPong);

            Color c = dangerColor;
            c.a = alpha;
            text.color = c;
        }
        else
        {
            Color c = originalColor;
            c.a = 1f;
            text.color = c;
        }
    }

    private void TrySubscribe()
    {
        ObjectiveManager manager = ObjectiveManager.Instance;

        if (manager == null || subscribedManager == manager)
            return;

        Unsubscribe();

        subscribedManager = manager;
        subscribedManager.OnObjectiveUpdated += RefreshUI;
    }

    private void Unsubscribe()
    {
        if (subscribedManager == null)
            return;

        subscribedManager.OnObjectiveUpdated -= RefreshUI;
        subscribedManager = null;
    }

    private void RefreshUI()
    {
        ObjectiveManager manager = ObjectiveManager.Instance;

        if (manager == null)
        {
            if (objectiveNameText != null)
                objectiveNameText.text = "No Objectives";

            if (objectiveDescriptionText != null)
                objectiveDescriptionText.text = "Objective manager not found.";

            ClearChecklist();
            return;
        }

        ObjectivesSO quest = manager.CurrentQuest;

        if (manager.AllObjectiveCompleted || quest == null)
        {
            if (objectiveDescriptionText != null)
                objectiveDescriptionText.text = "You've completed every quest!";

            ClearChecklist();
            return;
        }

        if (objectiveNameText != null)
            objectiveNameText.text = quest.objectiveName;

        if (objectiveDescriptionText != null)
            objectiveDescriptionText.text = "";

        RefreshChecklist(manager);
    }

    private void ClearChecklist()
    {
        if (checklistParent == null)
            return;

        foreach (Transform child in checklistParent)
        {
            Destroy(child.gameObject);
        }
    }

    private void RefreshChecklist(ObjectiveManager manager)
    {
        foreach (Transform child in checklistParent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < manager.ObjectiveCount; i++)
        {
            UIObjectiveChecklist item =
                Instantiate(checklistItemPrefab, checklistParent);

            item.Setup(
                manager.GetChecklistItemText(i),
                manager.IsChecklistItemCompleted(i)
            );
        }
    }
}