using UnityEngine;

public class TutorialArrowController : MonoBehaviour
{
    [Header("Objective Assets")]
    public ObjectivesSO objective1;
    public ObjectivesSO objective2;

    private StructureHarvestResource[] allHarvesters;
    private StructureApexPump[] allPumps;

    private void Start()
    {
        allHarvesters = FindObjectsByType<StructureHarvestResource>(FindObjectsInactive.Exclude);
        allPumps = FindObjectsByType<StructureApexPump>(FindObjectsInactive.Exclude);
    }

    private void Update()
    {
        if (ObjectiveManager.Instance == null || ObjectiveManager.Instance.CurrentQuest == null)
            return;

        ObjectivesSO currentQuest = ObjectiveManager.Instance.CurrentQuest;

        bool isObjective1 = (currentQuest == objective1);
        bool isObjective2 = (currentQuest == objective2);

        foreach (var harvester in allHarvesters)
        {
            if (harvester == null) continue;

            Transform arrow = GetArrowChild(harvester.transform);
            if (arrow == null) continue;

            if (isObjective1)
            {
                arrow.gameObject.SetActive(!harvester.IsWorking);
            }
            else
            {
                arrow.gameObject.SetActive(false);
            }
        }

        foreach (var pump in allPumps)
        {
            if (pump == null) continue;

            Transform arrow = GetArrowChild(pump.transform);
            if (arrow == null) continue;

            if (isObjective2)
            {
                bool isFueled = pump.CurrentFuel > 0 || pump.IsPumpActive;
                arrow.gameObject.SetActive(!isFueled);
            }
            else
            {
                arrow.gameObject.SetActive(false);
            }
        }
    }

    private Transform GetArrowChild(Transform parent)
    {
        foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
        {
            if (child.name.Contains("Arrow"))
                return child;
        }
        return null;
    }
}