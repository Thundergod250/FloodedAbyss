using UnityEngine;

public class MineBuildable : ItemBuildable
{
    [SerializeField] private MineManager mineManager;

    public override void OnBuildStructureSelected(StructureDataSO selectedStructure)
    {
        if (playerResources == null)
        {
            Debug.LogError("[ItemBuildable] PlayerResources instance not found!");
            return;
        }

        if (playerResources.TrySpendResources(selectedStructure.CostRequirements))
        {
            Debug.Log($"Successfully built {selectedStructure.StructureName}!");

            GameManager.Instance.uiController.CloseAllModals();

            structurePicked = selectedStructure;

            Debug.Log(structurePicked.CostRequirements[0].amount);

            int oreQuality = structurePicked.CostRequirements[0].amount;

            mineManager.EnterMine(oreQuality);
        }
        else
        {
            Debug.LogWarning(
                $"Cannot build {selectedStructure.StructureName}: Insufficient resources!"
            );
        }
    }
}
