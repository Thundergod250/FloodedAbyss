using UnityEngine;

public class StructureWaterInfrastructure : Structures
{
    [Header("Water Infrastructure")]
    [SerializeField] private float waterReduc = 10f;

    private bool hasReducedWater = false;

    public override void Activate()
    {
        if (isSubmerged && !ignoreSubmersion) return;
        if (hasReducedWater) return;

        if (GameManager.Instance == null ||
            GameManager.Instance.WaterLevel == null) return;

        GameManager.Instance.WaterLevel.ReduceWaterPressure(waterReduc);

        hasReducedWater = true;

        Debug.Log($"{name} reduced water height by {waterReduc}");
    }
}
