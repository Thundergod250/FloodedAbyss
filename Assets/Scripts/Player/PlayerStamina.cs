using UnityEngine;

public class PlayerStamina : MonoBehaviour
{
    [Header("Stamina")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float currentStamina = 100f;
    [SerializeField] private float regenRate = 1f;

    [Header("Cost")]
    [SerializeField] private float runStartCost = 20f;
    [SerializeField] private float runDrain = 5f;
    [SerializeField] private float jumpCost = 20f;
    [SerializeField] private float swimDrain = 5f;

    [Header("Exhaustion")]
    [SerializeField] private float exhaustionDelay = 5f;
    [SerializeField] private float exhaustionSpeedMultiplier = 0.5f;

    private bool exhausted;
    private float exhaustionTimer;

    public float CurrentStamina => currentStamina;
    public float MaxStamina => maxStamina;
    public bool IsExhausted => exhausted;
    public float ExhaustionSpeedMultiplier => exhaustionSpeedMultiplier;

    private void Update()
    {
        if (exhausted)
        {
            exhaustionTimer -= Time.deltaTime;

            if (exhaustionTimer <= 0f)
            {
                currentStamina += regenRate * Time.deltaTime;
                currentStamina = Mathf.Min(currentStamina, maxStamina);

                if (currentStamina >= maxStamina)
                {
                    exhausted = false;
                }
            }

            return;
        }

        currentStamina += regenRate * Time.deltaTime;
        currentStamina = Mathf.Min(currentStamina, maxStamina);
    }

    public bool StartRunning()
    {
        if (exhausted || currentStamina < runStartCost)
            return false;

        currentStamina -= runStartCost;
        return true;
    }

    public void DrainRunning()
    {
        if (exhausted)
            return;

        DrainStamina(runDrain * Time.deltaTime);
    }

    public bool UseJump()
    {
        if (exhausted || currentStamina < jumpCost)
            return false;

        DrainStamina(jumpCost);
        return true;
    }

    public void DrainSwimming()
    {
        if (exhausted)
            return;

        DrainStamina(swimDrain * Time.deltaTime);
    }

    private void DrainStamina(float amount)
    {
        currentStamina -= amount;
        currentStamina = Mathf.Max(currentStamina, 0f);

        if (currentStamina <= 0f)
        {
            currentStamina = 0f;
            exhausted = true;
            exhaustionTimer = exhaustionDelay;
        }
    }
}
