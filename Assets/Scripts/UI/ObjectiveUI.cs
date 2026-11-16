using TMPro;
using UnityEngine;

public class ObjectiveUI : MonoBehaviour
{
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
    [SerializeField] private float greatFloodFlickerThreshold = 60f;
    [SerializeField] private float pumpFlickerThreshold = 20f;
    [SerializeField] private float flickerSpeed = 5f;
    [SerializeField] private Color dangerColor = Color.red;

    [Header("Low Fuel Beep Tiers (Fuel Remaining)")]
    [Tooltip("Fuel at or below this value starts the first (very slow) beep tier. Above this = silent.")]
    [SerializeField] private float beepTier1Threshold = 100f;
    [Tooltip("Fuel at or below this value switches to the second (moderate) beep tier.")]
    [SerializeField] private float beepTier2Threshold = 50f;
    [Tooltip("Fuel at or below this value switches to the third (very fast) beep tier. Fuel of 0 is silent (pump empty).")]
    [SerializeField] private float beepTier3Threshold = 9f;

    [Tooltip("Seconds between beeps in Tier 1 (very slow, 100–51 fuel).")]
    [SerializeField] private float beepIntervalTier1 = 8.0f;
    [Tooltip("Seconds between beeps in Tier 2 (moderate, 50–10 fuel).")]
    [SerializeField] private float beepIntervalTier2 = 3.0f;
    [Tooltip("Seconds between beeps in Tier 3 (very fast, 9–1 fuel).")]
    [SerializeField] private float beepIntervalTier3 = 0.1f;

    private ObjectiveManager subscribedManager;

    private Color greatFloodOriginalColor = Color.white;
    private Color pumpOriginalColor = Color.white;

    private int currentFuelBeepTier = 0;
    private float lowFuelBeepTimer = 0f;

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

    public void SetPumpTimeRemaining(float seconds)
    {
        PumpTimeRemaining = seconds;

        if (pumpTimerText != null)
            pumpTimerText.text = Mathf.CeilToInt(Mathf.Max(0f, seconds)).ToString();
    }

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

    private bool IsPumpActive()
    {
        if (GameManager.Instance == null || GameManager.Instance.ApexPump == null)
            return true; // can't detect → assume pump may be running

        return GameManager.Instance.ApexPump.ActiveSwitchCount > 0;
    }

    private static bool IsInDanger(float remainingTime, float threshold)
    {
        return remainingTime > 0f && remainingTime <= threshold;
    }

    private int GetFuelBeepTier(float fuel)
    {
        if (!IsPumpActive()) return 0;

        if (fuel <= 0f) return 0;

        if (fuel <= beepTier3Threshold) return 3;
        if (fuel <= beepTier2Threshold) return 2;
        if (fuel <= beepTier1Threshold) return 1;
        return 0;
    }

    private void UpdateLowFuelBeep(int tier)
    {
        if (tier != currentFuelBeepTier)
        {
            currentFuelBeepTier = tier;
            lowFuelBeepTimer = 0f;
        }

        if (tier <= 0)
            return;

        lowFuelBeepTimer -= Time.deltaTime;
        if (lowFuelBeepTimer > 0f)
            return;

        var sm = GameManager.Instance != null ? GameManager.Instance.SoundManager : null;
        if (sm != null)
            sm.Play(LowFuelSFXName);

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