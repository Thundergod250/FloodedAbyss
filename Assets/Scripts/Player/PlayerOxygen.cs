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
    private float damageTimer;

    public float CurrentOxygen => currentOxygen;
    public float MaxOxygen => maxOxygen;

    private void Awake()
    {
        health = GetComponent<Health>();
    }

    private void Update()
    {
        PlayerMovement movement = GetComponent<PlayerMovement>();

        if (movement == null) return;

        if (!movement.isSwimming)
        {
            RegenerateOxygen();
        }

        if (currentOxygen <= 0f)
        {
            DealOxygenDamage();
        }
    }

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

    public void DrainSwimSprint()
    {
        DrainOxygen(sprintDrain * Time.deltaTime);
    }

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
                int damage = Mathf.CeilToInt(health.MaxHealth * 0.1f);
                health.TakeDamage(damage);
            }

            damageTimer = damageInterval;
        }
    }
}
