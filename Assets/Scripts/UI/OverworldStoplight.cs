using UnityEngine;
using UnityEngine.UI;

public class OverworldStoplight : MonoBehaviour
{
    [Header("Stoplight References")]
    [SerializeField] private Image greenLight; 
    [SerializeField] private Image redLight;
    private StructureHarvestResource parentStructure;

    private void Awake()
    {
        if (parentStructure == null)
            parentStructure = GetComponentInParent<StructureHarvestResource>();
    }

    private void OnEnable()
    {
        if (parentStructure != null)
        {
            // Automatically add listeners via code
            parentStructure.EvtOnBuildingWorking.AddListener(SetWorking);
            parentStructure.EvtOnBuildingNotWorking.AddListener(SetIdle);

            // Set correct initial state right away
            SetWorkingState(parentStructure.IsWorking);
        }
    }

    private void OnDisable()
    {
        if (parentStructure != null)
        {
            parentStructure.EvtOnBuildingWorking.RemoveListener(SetWorking);
            parentStructure.EvtOnBuildingNotWorking.RemoveListener(SetIdle);
        }
    }

    public void SetWorking() => SetWorkingState(true);

    public void SetIdle() => SetWorkingState(false);

    private void SetWorkingState(bool isWorking)
    {
        if (greenLight != null)
            greenLight.gameObject.SetActive(isWorking);

        if (redLight != null)
            redLight.gameObject.SetActive(!isWorking);
    }
}