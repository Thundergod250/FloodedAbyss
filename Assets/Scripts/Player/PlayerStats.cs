using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(PlayerStamina))]
[RequireComponent(typeof(PlayerOxygen))]
public class PlayerStats : MonoBehaviour
{
    [Header("Upgrade Settings")]
    [SerializeField] private StatUpgradeData upgradeData;

    [Header("Equipment")]
    public BasePickAxe basePickAxe;

    [Header("Stat Levels")]
    public int healthLevel = 1;
    public int staminaLevel = 1;
    public int oxygenLevel = 1;
    public int pickAxeDamageLevel = 1;
    public int pickAxeAttackSpeedLevel = 1;

    private Health playerHealth;
    private PlayerStamina playerStamina;
    private PlayerOxygen playerOxygen;

    private void Start()
    {
        if (playerHealth == null) 
            playerHealth = GetComponent<Health>();

        if (playerStamina == null)
            playerStamina = GetComponent<PlayerStamina>();

        if (playerOxygen == null)
            playerOxygen = GetComponent<PlayerOxygen>();

        ApplyAllStatLevels();
    }

    public void ApplyAllStatLevels()
    {
        if (upgradeData == null) return;

        ApplyHealthLevel();
        ApplyStaminaLevel();
        ApplyOxygenLevel();
        ApplyPickAxeDamageLevel();
        ApplyPickAxeAttackSpeedLevel();
    }

    public void UpgradeMaxHealth()
    {
        if (upgradeData == null || healthLevel >= upgradeData.healthLevels.Length) return;

        healthLevel++;
        ApplyHealthLevel();
    }

    public void UpgradeMaxStamina()
    {
        if (upgradeData == null || staminaLevel >= upgradeData.staminaLevels.Length) return;

        staminaLevel++;
        ApplyStaminaLevel();
    }

    public void UpgradeMaxOxygen()
    {
        if (upgradeData == null || oxygenLevel >= upgradeData.oxygenLevels.Length) return;

        oxygenLevel++;
        ApplyOxygenLevel();
    }

    public void UpgradePickAxeDamage()
    {
        if (upgradeData == null || pickAxeDamageLevel >= upgradeData.pickAxeDamageLevels.Length) return;

        pickAxeDamageLevel++;
        ApplyPickAxeDamageLevel();
    }

    public void UpgradePickAxeAttackSpeed()
    {
        if (upgradeData == null || pickAxeAttackSpeedLevel >= upgradeData.pickAxeAttackSpeedLevels.Length) return;

        pickAxeAttackSpeedLevel++;
        ApplyPickAxeAttackSpeedLevel();
    }

    private void ApplyHealthLevel()
    {
        if (playerHealth != null && upgradeData != null && healthLevel <= upgradeData.healthLevels.Length)
        {
            int targetValue = upgradeData.healthLevels[healthLevel - 1];
            playerHealth.SetMaxHealth(targetValue);
        }
    }

    private void ApplyStaminaLevel()
    {
        if (playerStamina != null && upgradeData != null && staminaLevel <= upgradeData.staminaLevels.Length)
        {
            float targetValue = upgradeData.staminaLevels[staminaLevel - 1];
            playerStamina.SetMaxStamina(targetValue);
        }
    }

    private void ApplyOxygenLevel()
    {
        if (playerOxygen != null && upgradeData != null && oxygenLevel <= upgradeData.oxygenLevels.Length)
        {
            float targetValue = upgradeData.oxygenLevels[oxygenLevel - 1];
            playerOxygen.SetMaxOxygen(targetValue);
        }
    }

    private void ApplyPickAxeDamageLevel()
    {
        if (basePickAxe != null && upgradeData != null && pickAxeDamageLevel <= upgradeData.pickAxeDamageLevels.Length)
        {
            int targetValue = upgradeData.pickAxeDamageLevels[pickAxeDamageLevel - 1];
            basePickAxe.SetDamage(targetValue);
        }
    }

    private void ApplyPickAxeAttackSpeedLevel()
    {
        if (basePickAxe != null && upgradeData != null && pickAxeAttackSpeedLevel <= upgradeData.pickAxeAttackSpeedLevels.Length)
        {
            float targetValue = upgradeData.pickAxeAttackSpeedLevels[pickAxeAttackSpeedLevel - 1];
            basePickAxe.SetAttackSpeed(targetValue);
        }
    }
}