using UnityEngine;

public class StructureGenerator : ItemStructure
{
    [Header("Generator Settings")]
    [SerializeField] private float secondsPerResource = 1f; // Seconds per 1 unit of fuel burned
    [SerializeField] private int energyPerResource = 1;     // Energy added to player per unit burned
    [SerializeField] private bool addDirectlyToPlayer = true; // Automatically deposit to PlayerResources

    private float timer = 0f;
    private int generatedEnergySession = 0; // Tracks local generator output history

    public int GeneratedEnergySession => generatedEnergySession;

    private void Update()
    {
        // Active burn loop: runs whenever targetResource (Wood/Fuel) is available
        if (targetResource.currentAmount > 0)
        {
            timer += Time.deltaTime;

            if (timer >= secondsPerResource)
            {
                timer -= secondsPerResource;
                ConsumeFuelAndProduceEnergy();
            }
        }
        else
        {
            timer = 0f; // Idle when out of fuel
        }
    }

    private void ConsumeFuelAndProduceEnergy()
    {
        targetResource.currentAmount--;
        generatedEnergySession += energyPerResource;

        // Pass false to suppress popups for per-second generation
        if (addDirectlyToPlayer && playerResources != null)
        {
            playerResources.AddResource(ResourceType.Energy, energyPerResource, showNotification: false);
        }

        RefreshUIIfOpen();
    }

    private void RefreshUIIfOpen()
    {
        if (GameManager.Instance != null && GameManager.Instance.uiController != null)
        {
            UIStructures modal = GameManager.Instance.uiController.GetModal<UIStructures>(UIController.UIState.Structure);
            if (modal != null && modal.CurrentStructure == this)
            {
                modal.UpdateUI();
            }
        }
    }
}