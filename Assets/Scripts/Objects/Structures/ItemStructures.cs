using UnityEngine;

public class ItemStructure : Item
{
    [Header("Structure Settings")]
    [SerializeField] private StructureDataSO structureData;

    protected PlayerResources playerResources;

    protected virtual void Start()
    {
        if (GameManager.Instance != null && GameManager.Instance.playerController != null)
        {
            playerResources = GameManager.Instance.playerController.PlayerResources;
        }
    }

    /// <summary>
    /// Triggered when the player interacts with this placed structure.
    /// Opens the Structure UI Modal and passes necessary structure references/data.
    /// </summary>
    public override void Activate()
    {
        // 1. Change UI State to Structure
        GameManager.Instance.uiController.OpenModal(UIController.UIState.Structure);

        // 2. Retrieve the UIStructure modal script instance
        UIStructures structureModal = GameManager.Instance.uiController.GetModal<UIStructures>(UIController.UIState.Structure);

        if (structureModal != null)
        {
            // 3. Populate or bind structure data to the UI
            //structureModal.SetupStructure(this, structureData);
        }
        else
        {
            Debug.LogError($"[ItemStructure] UIStructure modal instance not found on UIState.Structure!");
        }
    }

    public StructureDataSO StructureData => structureData;
}