using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PanelStructureCard : MonoBehaviour
{
    private const string ErrorSFXName = "SFXError";

    [Header("Card References")]
    [SerializeField] private Image structureImage;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI infoText;
    [SerializeField] private TextMeshProUGUI notEnoughWorkersText; // Red warning text
    [SerializeField] private TextMeshProUGUI ownedText;            // Owned count text
    [SerializeField] private float warningDisplayDuration = 2f;    // Editable duration
    [SerializeField] private Transform costContainer; // Container holding the ResourcePair prefabs for costs
    [SerializeField] private Button cardButton;

    private StructureDataSO boundData;
    private System.Action<StructureDataSO> onCardClickedCallback;
    private Coroutine warningRoutine;

    private void OnEnable()
    {
        if (notEnoughWorkersText != null)
            notEnoughWorkersText.gameObject.SetActive(false);
    }

    public void Setup(StructureDataSO data, GameObject resourcePairPrefabTemplate, System.Action<StructureDataSO> onClickCallback)
    {
        boundData = data;
        onCardClickedCallback = onClickCallback;

        if (structureImage != null && data.StructureIcon != null)
            structureImage.sprite = data.StructureIcon;

        if (titleText != null)
            titleText.text = data.StructureTitle;

        if (infoText != null)
            infoText.text = data.StructureInfo;

        if (notEnoughWorkersText != null)
            notEnoughWorkersText.gameObject.SetActive(false);

        // Fetch owned count globally from BuildingManager using the ScriptableObject reference
        if (ownedText != null && data != null)
        {
            int ownedCount = 0;
            if (GameManager.Instance != null && GameManager.Instance.BuildingManager != null)
            {
                ownedCount = GameManager.Instance.BuildingManager.GetOwnedCount(data);
            }
            ownedText.text = $"Owned: {ownedCount}";
        }

        if (cardButton != null)
        {
            cardButton.onClick.RemoveAllListeners();
            cardButton.onClick.AddListener(() => onCardClickedCallback?.Invoke(boundData));
        }

        BuildCostDisplay(resourcePairPrefabTemplate);
    }

    public void ShowWarningMessage(string message)
    {
        if (notEnoughWorkersText != null)
        {
            // ── Error SFX ─────────────────────────────────────────────
            // Fires on every warning, whether it's "not enough workers"
            // or "not enough resources". Single entry point, single call.
            var sm = GameManager.Instance != null ? GameManager.Instance.SoundManager : null;
            if (sm != null)
                sm.Play(ErrorSFXName);
            // ──────────────────────────────────────────────────────────

            if (warningRoutine != null) StopCoroutine(warningRoutine);
            warningRoutine = StartCoroutine(FlashWarningRoutine(message));
        }
    }

    private IEnumerator FlashWarningRoutine(string message)
    {
        if (titleText != null) titleText.gameObject.SetActive(false);
        if (infoText != null) infoText.gameObject.SetActive(false);

        notEnoughWorkersText.gameObject.SetActive(true);
        notEnoughWorkersText.text = message;

        yield return new WaitForSeconds(warningDisplayDuration);

        notEnoughWorkersText.gameObject.SetActive(false);
        if (titleText != null) titleText.gameObject.SetActive(true);
        if (infoText != null) infoText.gameObject.SetActive(true);
    }

    private void BuildCostDisplay(GameObject resourcePairPrefabTemplate)
    {
        if (costContainer == null || resourcePairPrefabTemplate == null || boundData == null) return;

        foreach (Transform child in costContainer)
        {
            if (child.gameObject != resourcePairPrefabTemplate)
                Destroy(child.gameObject);
        }

        foreach (var cost in boundData.Costs)
        {
            if (cost.resourceDataSo == null) continue;

            GameObject costGO = Instantiate(resourcePairPrefabTemplate, costContainer);
            costGO.SetActive(true);

            PanelResourcePair pairPanel = costGO.GetComponent<PanelResourcePair>();
            if (pairPanel != null)
                pairPanel.Setup(cost.resourceDataSo, cost.amount);
        }
    }
}