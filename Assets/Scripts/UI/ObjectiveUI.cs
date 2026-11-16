using TMPro;
using UnityEngine;

public class ObjectiveUI : MonoBehaviour
{
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

    private ObjectiveManager subscribedManager;

    // Cached original colors so we can restore them when the timer is safe
    private Color greatFloodOriginalColor = Color.white;
    private Color pumpOriginalColor = Color.white;
    private int lastCQI = -1;
    private bool hasInitializedCQI = false;

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
    }

    private void Update()
    {
        if (subscribedManager == null && ObjectiveManager.Instance != null)
        {
            TrySubscribe();
            RefreshUI();
        }

        // --- Great Flood timer ---
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

        // --- Pump timer ---
        // The pump timer intentionally stops at 9, so once it enters the 20s
        // range it will keep flickering red indefinitely.
        if (pumpTimerText != null)
        {
            float pumpRemaining = GetPumpRemainingSeconds();
            ApplyTimerVisuals(
                pumpTimerText,
                pumpRemaining,
                pumpOriginalColor,
                pumpFlickerThreshold
            );
        }
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
    /// Returns the pump time in seconds. Prefers an externally-set
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
    /// Turns the text red and flickers it while 0 &lt; remainingTime &lt;= threshold.
    /// Otherwise restores the original color at full opacity.
    /// </summary>
    private void ApplyTimerVisuals(TextMeshProUGUI text, float remainingTime, Color originalColor, float threshold)
    {
        if (text == null) return;

        bool isInDanger = remainingTime > 0f && remainingTime <= threshold;

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