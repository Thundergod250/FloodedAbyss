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

    public float CurrentStamina => currentStamina;
    public float MaxStamina => maxStamina;

    private void Update()
    {
        currentStamina += regenRate * Time.deltaTime;
        currentStamina = Mathf.Min(currentStamina, maxStamina);
    }

    public void DrainRunning()
    {
        DrainStamina(runDrain * Time.deltaTime);
    }

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
}