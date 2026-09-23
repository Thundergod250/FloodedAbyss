using UnityEngine;

public class PlayerDeath : MonoBehaviour
{
    [Header("Respawn Settings")]
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private float resourceLossPercentage = 0.5f; // 50% loss

    private Health playerHealth;
    private PlayerStamina playerStamina;
    private PlayerOxygen playerOxygen;
    private PlayerResources playerResources;
    private CharacterController characterController;

    private void Awake()
    {
        // Cache references attached to the Player
        playerHealth = GetComponent<Health>();
        playerStamina = GetComponent<PlayerStamina>();
        playerOxygen = GetComponent<PlayerOxygen>();
        playerResources = GetComponent<PlayerResources>();
        characterController = GetComponent<CharacterController>();
    }

    private void OnEnable()
    {
        if (playerHealth != null) 
            playerHealth.EvtOnDied.AddListener(HandlePlayerDeath);
    }

    private void OnDisable()
    {
        if (playerHealth != null) 
            playerHealth.EvtOnDied.RemoveListener(HandlePlayerDeath);
    }

    private void HandlePlayerDeath()
    {
        if (respawnPoint != null)
        {
            if (characterController != null) characterController.enabled = false;

            transform.position = respawnPoint.position;
            transform.rotation = respawnPoint.rotation;

            if (characterController != null) characterController.enabled = true;
        }
        
        if (playerHealth != null)
        {
            int halfHealth = Mathf.Max(1, playerHealth.GetMaxHealth() / 2);
            playerHealth.HealHealth(halfHealth);
        }
        
        if (playerStamina != null) 
            playerStamina.RestoreFullStamina();

        if (playerOxygen != null) 
            playerOxygen.RestoreFullOxygen();
        
        if (playerResources != null) 
            playerResources.ApplyResourceDeathPenalty(resourceLossPercentage);
        
        gameObject.SetActive(true);
    }
    
    public void SetRespawnPoint(Transform newRespawnPoint) => respawnPoint = newRespawnPoint;
}