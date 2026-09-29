using UnityEngine;

[CreateAssetMenu(fileName = "StatUpgradeData", menuName = "Stats/Stat Upgrade Data")]
public class StatUpgradeData : ScriptableObject
{
    [Header("Max Health Levels")]
    public int[] healthLevels = new int[] { 100, 125, 150, 200, 250 };

    [Header("Max Stamina Levels")]
    public float[] staminaLevels = new float[] { 100f, 120f, 150f, 180f, 220f };

    [Header("Max Oxygen Levels")]
    public float[] oxygenLevels = new float[] { 100f, 125f, 150f, 175f, 200f };

    [Header("PickAxe Damage Levels")]
    public int[] pickAxeDamageLevels = new int[] { 10, 15, 22, 30, 40 };

    [Header("PickAxe Attack Speed Levels")]
    public float[] pickAxeAttackSpeedLevels = new float[] { 1.0f, 1.15f, 1.3f, 1.5f, 1.8f };
}