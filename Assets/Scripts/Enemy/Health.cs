using System;
using UnityEngine;
using UnityEngine.UI;

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

    public event Action OnDied;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    #region Functions 
    public void TakeDamage(int damage) 
    {
        if (currentHealth <= 0)
            return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        if (currentHealth <= 0)
        {
            Die();
        }
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
    #endregion

    private void Die()
    {
        OnDied?.Invoke();

        gameObject.SetActive(false);
    }
}

