using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(PlayerStamina))]
[RequireComponent(typeof(PlayerOxygen))]
public class PlayerStats : MonoBehaviour
{
    public UnityEvent EvtOnStatChanged; 
    
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

    // --- Upgrade Methods ---

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

    // --- Getter Methods for UI Debug Panel ---

    public int GetStatLevel(string statKey)
    {
        return statKey switch
        {
            "Health" => healthLevel,
            "Stamina" => staminaLevel,
            "Oxygen" => oxygenLevel,
            "AxeDamage" => pickAxeDamageLevel,
            "AxeSpeed" => pickAxeAttackSpeedLevel,
            _ => 1
        };
    }

    public string GetStatValueString(string statKey)
    {
        if (upgradeData == null) return "N/A";

        return statKey switch
        {
            "Health" => (healthLevel <= upgradeData.healthLevels.Length) ? upgradeData.healthLevels[healthLevel - 1].ToString() : "MAX",
            "Stamina" => (staminaLevel <= upgradeData.staminaLevels.Length) ? upgradeData.staminaLevels[staminaLevel - 1].ToString("0.#") : "MAX",
            "Oxygen" => (oxygenLevel <= upgradeData.oxygenLevels.Length) ? upgradeData.oxygenLevels[oxygenLevel - 1].ToString("0.#") : "MAX",
            "AxeDamage" => (pickAxeDamageLevel <= upgradeData.pickAxeDamageLevels.Length) ? upgradeData.pickAxeDamageLevels[pickAxeDamageLevel - 1].ToString() : "MAX",
            "AxeSpeed" => (pickAxeAttackSpeedLevel <= upgradeData.pickAxeAttackSpeedLevels.Length) ? upgradeData.pickAxeAttackSpeedLevels[pickAxeAttackSpeedLevel - 1].ToString("0.#") : "MAX",
            _ => "0"
        };
    }
    
    // --- Helper Method ---

    private void ApplyStat<T>(T[] levels, int currentLevel, Object targetComponent, System.Action<T> applyAction)
    {
        // Validate target component and data array
        if (targetComponent == null || upgradeData == null || levels == null) return;

        // Guard against index out of bounds (1-based level)
        int index = currentLevel - 1;
        if (index >= 0 && index < levels.Length)
        {
            applyAction(levels[index]);
            EvtOnStatChanged?.Invoke();
        }
    }

    // --- Simplified Apply Methods ---
    private void ApplyHealthLevel() => 
        ApplyStat(upgradeData?.healthLevels, healthLevel, playerHealth, playerHealth.SetMaxHealth);

    private void ApplyStaminaLevel() => 
        ApplyStat(upgradeData?.staminaLevels, staminaLevel, playerStamina, playerStamina.SetMaxStamina);

    private void ApplyOxygenLevel() => 
        ApplyStat(upgradeData?.oxygenLevels, oxygenLevel, playerOxygen, playerOxygen.SetMaxOxygen);

    private void ApplyPickAxeDamageLevel() => 
        ApplyStat(upgradeData?.pickAxeDamageLevels, pickAxeDamageLevel, basePickAxe, basePickAxe.SetDamage);

    private void ApplyPickAxeAttackSpeedLevel() => 
        ApplyStat(upgradeData?.pickAxeAttackSpeedLevels, pickAxeAttackSpeedLevel, basePickAxe, basePickAxe.SetAttackSpeed);
    
    public void UpgradeAllStats()
    {
        UpgradeMaxHealth();
        UpgradeMaxStamina();
        UpgradeMaxOxygen();
        UpgradePickAxeDamage();
        UpgradePickAxeAttackSpeed();
    }
}