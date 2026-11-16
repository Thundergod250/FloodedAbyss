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
    [SerializeField] private GameObject newObjBubble;

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
    [Tooltip("Fuel at or below this value starts the first (moderate) beep tier.")]
    [SerializeField] private float beepTier1Threshold = 100f;
    [Tooltip("Fuel at or below this value switches to the second (faster) beep tier.")]
    [SerializeField] private float beepTier2Threshold = 50f;
    [Tooltip("Fuel at or below this value switches to the third (fastest) beep tier.")]
    [SerializeField] private float beepTier3Threshold = 20f;

    [Tooltip("Seconds between beeps in Tier 1.")]
    [SerializeField] private float beepIntervalTier1 = 1.2f;
    [Tooltip("Seconds between beeps in Tier 2.")]
    [SerializeField] private float beepIntervalTier2 = 0.6f;
    [Tooltip("Seconds between beeps in Tier 3.")]
    [SerializeField] private float beepIntervalTier3 = 0.3f;

    private ObjectiveManager subscribedManager;

    // Cached original colors so we can restore them when the timer is safe
    private Color greatFloodOriginalColor = Color.white;
    private Color pumpOriginalColor = Color.white;
    private int lastCQI = -1;
    private bool hasInitializedCQI = false;

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

        // Reset low-fuel beep state. The beep is a one-shot, so there's
        // nothing playing to Stop — just clear the scheduling state so a
        // re-enable starts fresh.
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

        // --- Great Flood timer (visuals only; audio is handled elsewhere) ---
        if (GameManager.Instance != null && GameManager.Instance.WaterLevel != null && GreatFloodText != null)
        {
            WaterLevel wl = GameManager.Instance.WaterLevel;

            float remainingTime;

            if (!wl.GreatFloodStarted)
            {
                // Phase 1: Before Great Flood
                remainingTime = wl.TimeUntilGreatFlood;
            }
            else if (wl.GreatFloodStarted)
            {
                // Phase 2: During Great Flood
                remainingTime = wl.GreatFloodTimeRemaining;
            }
            else
            {
                // Phase 3: Flood ended
                remainingTime = 0f;
            }

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
        // Beeping is driven by whatever fuel value GetPumpRemainingSeconds()
        // returns, so it works even if pumpTimerText is null (as long as
        // PumpTimeRemaining has been set externally).
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
    /// Shared danger predicate used by the visual flicker on both timers,
    /// so the red/flicker timing stays consistent.
    /// </summary>
    private static bool IsInDanger(float remainingTime, float threshold)
    {
        return remainingTime > 0f && remainingTime <= threshold;
    }

    /// <summary>
    /// Returns the current low-fuel beep tier (0 = silent, 1/2/3 = beeping
    /// with progressively shorter intervals).
    /// </summary>
    private int GetFuelBeepTier(float fuel)
    {
        if (fuel <= 0f) return 3; // empty tank → keep beeping at fastest rate
        if (fuel <= beepTier3Threshold) return 3;
        if (fuel <= beepTier2Threshold) return 2;
        if (fuel <= beepTier1Threshold) return 1;
        return 0;
    }

    /// <summary>
    /// Plays a one-shot low-fuel beep on a tier-dependent cadence.
    /// Fires immediately whenever the tier changes so crossing 100/50/20
    /// is heard right away, then respects the new tier's interval.
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
            return; // silent above the first threshold

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
            // Flicker between 0.2 and 1.0 alpha while red
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
        CheckForNewQuest(manager);

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

        // Rebuild the checklist once.
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
        // Clear previous checklist items.
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
    private void CheckForNewQuest(ObjectiveManager manager)
    {
        int currentCQI = manager.CQI;

        // Initial setup: remember the starting quest.
        if (!hasInitializedCQI)
        {
            lastCQI = currentCQI;
            hasInitializedCQI = true;
            return;
        }

        // Show bubble whenever the quest index increases.
        if (currentCQI > lastCQI)
        {
            if (newObjBubble != null)
                newObjBubble.SetActive(true);
        }

        lastCQI = currentCQI;
    }
}