using UnityEngine;

public class PlayerOxygen : MonoBehaviour
{
    [Header("Oxygen")]
    [SerializeField] private float maxOxygen = 100f;
    [SerializeField] private float currentOxygen = 100f;
    [SerializeField] private float regenRate = 1f;

    [Header("Swim Sprint")]
    [SerializeField] private float sprintStartCost = 20f;
    [SerializeField] private float sprintDrain = 5f;

    [Header("Oxygen Damage")]
    [SerializeField] private float damageInterval = 1f;

    private Health health;
    private PlayerMovement playerMovement; 
    private float damageTimer;

    private void Start()
    {
        health = GetComponent<Health>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        if (playerMovement == null) 
            return;

        if (!playerMovement.isSwimming) 
            RegenerateOxygen();

        if (currentOxygen <= 0f) 
            DealOxygenDamage();
    }
    
    public float GetCurrentOxygen() => currentOxygen;
    public float GetMaxOxygen() => maxOxygen;

    private void RegenerateOxygen()
    {
        currentOxygen += regenRate * Time.deltaTime;
        currentOxygen = Mathf.Min(currentOxygen, maxOxygen);
    }

    public void DrainOxygen(float amount)
    {
        currentOxygen -= amount;
        currentOxygen = Mathf.Max(currentOxygen, 0f);
    }

    public void DrainSwimSprint() => DrainOxygen(sprintDrain * Time.deltaTime);

    public bool StartSwimSprint()
    {
        if (currentOxygen < sprintStartCost)
            return false;

        DrainOxygen(sprintStartCost);
        return true;
    }

    private void DealOxygenDamage()
    {
        damageTimer -= Time.deltaTime;
        if (damageTimer <= 0f)
        {
            if (health != null)
            {
                int damage = Mathf.CeilToInt(health.GetMaxHealth() * 0.1f);
                health.TakeDamage(damage);
            }
            damageTimer = damageInterval;
        }
    }
    
    public void IncreaseMaxOxygen(float amount, bool fillCurrent = false)
    {
        if (amount <= 0f) return;

        maxOxygen += amount;
        if (fillCurrent)
            currentOxygen += amount;

        currentOxygen = Mathf.Min(currentOxygen, maxOxygen);
    }

    public void DecreaseMaxOxygen(float amount)
    {
        if (amount <= 0f) return;

        maxOxygen = Mathf.Max(1f, maxOxygen - amount);
        currentOxygen = Mathf.Min(currentOxygen, maxOxygen);
    }

    public void SetMaxOxygen(float newMaxOxygen)
    {
        maxOxygen = Mathf.Max(1f, newMaxOxygen);
        currentOxygen = Mathf.Min(currentOxygen, maxOxygen);
    }
}