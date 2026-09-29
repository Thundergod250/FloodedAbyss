using System;
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
        playerHealth ??= GetComponent<Health>();
        playerStamina ??= GetComponent<PlayerStamina>();
        playerOxygen ??= GetComponent<PlayerOxygen>();

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

    public void UpgradeMaxHealth() => TryIncrementLevel(ref healthLevel, upgradeData?.healthLevels, ApplyHealthLevel);
    public void UpgradeMaxStamina() => TryIncrementLevel(ref staminaLevel, upgradeData?.staminaLevels, ApplyStaminaLevel);
    public void UpgradeMaxOxygen() => TryIncrementLevel(ref oxygenLevel, upgradeData?.oxygenLevels, ApplyOxygenLevel);
    public void UpgradePickAxeDamage() => TryIncrementLevel(ref pickAxeDamageLevel, upgradeData?.pickAxeDamageLevels, ApplyPickAxeDamageLevel);
    public void UpgradePickAxeAttackSpeed() => TryIncrementLevel(ref pickAxeAttackSpeedLevel, upgradeData?.pickAxeAttackSpeedLevels, ApplyPickAxeAttackSpeedLevel);

    public void UpgradeAllStats()
    {
        UpgradeMaxHealth();
        UpgradeMaxStamina();
        UpgradeMaxOxygen();
        UpgradePickAxeDamage();
        UpgradePickAxeAttackSpeed();
    }

    // --- Getter Methods for UI Debug Panel ---

    public int GetStatLevel(string statKey) => statKey switch
    {
        "Health" => healthLevel,
        "Stamina" => staminaLevel,
        "Oxygen" => oxygenLevel,
        "AxeDamage" => pickAxeDamageLevel,
        "AxeSpeed" => pickAxeAttackSpeedLevel,
        _ => 1
    };

    public string GetStatValueString(string statKey) => statKey switch
    {
        "Health" => FormatStatValue(upgradeData?.healthLevels, healthLevel),
        "Stamina" => FormatStatValue(upgradeData?.staminaLevels, staminaLevel, "0.#"),
        "Oxygen" => FormatStatValue(upgradeData?.oxygenLevels, oxygenLevel, "0.#"),
        "AxeDamage" => FormatStatValue(upgradeData?.pickAxeDamageLevels, pickAxeDamageLevel),
        "AxeSpeed" => FormatStatValue(upgradeData?.pickAxeAttackSpeedLevels, pickAxeAttackSpeedLevel, "0.#"),
        _ => "0"
    };

    // --- Helper Methods ---

    private void TryIncrementLevel<T>(ref int currentLevel, T[] levelsArray, Action applyAction)
    {
        if (levelsArray == null || currentLevel >= levelsArray.Length) return;

        currentLevel++;
        applyAction?.Invoke();
    }

    private string FormatStatValue<T>(T[] levelsArray, int level, string format = null)
    {
        if (levelsArray == null) return "N/A";
        if (level > levelsArray.Length) return "MAX";

        T rawValue = levelsArray[level - 1];
        return !string.IsNullOrEmpty(format) && rawValue is IFormattable formattable
            ? formattable.ToString(format, null)
            : rawValue.ToString();
    }

    private void ApplyStat<T>(T[] levels, int currentLevel, UnityEngine.Object targetComponent, Action<T> applyAction)
    {
        if (targetComponent == null || upgradeData == null || levels == null) return;

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
}