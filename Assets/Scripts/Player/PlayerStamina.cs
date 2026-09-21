using UnityEngine;

public class PlayerStamina : MonoBehaviour
{
    [Header("Stamina")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float currentStamina = 100f;
    [SerializeField] private float regenRate = 1f;

    [Header("Cost")]
    [SerializeField] private float runDrain = 5f;
    [SerializeField] private float jumpCost = 20f;
    [SerializeField] private float swimDrain = 5f;

    private void Update()
    {
        currentStamina += regenRate * Time.deltaTime;
        currentStamina = Mathf.Min(currentStamina, maxStamina);
    }
    
    public float GetCurrentStamina() => currentStamina;
    public float GetMaxStamina() => maxStamina;

    public void DrainRunning() => DrainStamina(runDrain * Time.deltaTime);

    public bool UseJump()
    {
        if (currentStamina < jumpCost)
            return false;
        DrainStamina(jumpCost);
        return true;
    }

    public void DrainSwimming()
    {
        DrainStamina(swimDrain * Time.deltaTime);
    }

    private void DrainStamina(float amount)
    {
        currentStamina -= amount;
        currentStamina = Mathf.Max(currentStamina, 0f);
    }
    
    public void IncreaseMaxStamina(float amount, bool fillCurrent = false)
    {
        if (amount <= 0f) return;
        maxStamina += amount;
        if (fillCurrent) 
            currentStamina += amount;
        currentStamina = Mathf.Min(currentStamina, maxStamina);
    }
    
    public void DecreaseMaxStamina(float amount)
    {
        if (amount <= 0f) return;

        maxStamina = Mathf.Max(1f, maxStamina - amount);
        currentStamina = Mathf.Min(currentStamina, maxStamina);
    }
    
    public void SetMaxStamina(float newMaxStamina)
    {
        maxStamina = Mathf.Max(1f, newMaxStamina);
        currentStamina = Mathf.Min(currentStamina, maxStamina);
    }
}