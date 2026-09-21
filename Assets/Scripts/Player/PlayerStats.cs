using UnityEngine;

[RequireComponent(typeof(Health))]
public class PlayerStats : MonoBehaviour
{
    [Header("Component References")]
    private Health playerHealth;

    private void Start()
    {
        if (playerHealth == null) 
            playerHealth = GetComponent<Health>();
    }
   
    public void UpgradeMaxHealth(int amount, bool healCurrent = true)
    {
        if (playerHealth != null)
            playerHealth.IncreaseMaxHealth(amount, healCurrent);
        else
            Debug.LogWarning("PlayerStats: Health component reference is missing!");
    }
}