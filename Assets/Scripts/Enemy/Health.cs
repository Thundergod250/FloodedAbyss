using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    public enum CharacterCategory
    {
        Ally,
        Enemy
    }

    [SerializeField] private CharacterCategory category;

    [Header("Health Values")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;

    [Header("Events")]
    public UnityEvent EvtOnHit;
    public UnityEvent EvtOnDied;

    private void Start() => currentHealth = maxHealth;

    #region Functions
    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0)
            return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        EvtOnHit?.Invoke();

        if (currentHealth <= 0) 
            Die();
    }

    public void DecreaseHealth(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 1);
    }

    public void IncreaseHealth(int amount)
    {
        maxHealth += amount;
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
    }

    public void HealHealth(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
    }
    
    public void IncreaseMaxHealth(int amount, bool healCurrent = false)
    {
        if (amount <= 0) return;
        maxHealth += amount;
        if (healCurrent) 
            currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
    }
    
    public void DecreaseMaxHealth(int amount)
    {
        if (amount <= 0) return;
        maxHealth = Mathf.Max(1, maxHealth - amount);
        currentHealth = Mathf.Min(currentHealth, maxHealth);
    }
    
    public void SetMaxHealth(int newMaxHealth)
    {        
        maxHealth = Mathf.Max(1, newMaxHealth);
        currentHealth = Mathf.Min(currentHealth, maxHealth);
    }
    #endregion

    private void Die()
    {
        EvtOnDied?.Invoke();   
        gameObject.SetActive(false);
    }
}