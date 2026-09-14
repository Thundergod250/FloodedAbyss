using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class WaterLevel : MonoBehaviour
{
    public Transform waterLevelTransform;

    [SerializeField] private GameObject playerRefTest;
    [SerializeField] private float upwardForce = 10f;

    private PlayerMovement playerMovement;


    private void Awake()
    {
        playerMovement = playerRefTest.GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        float depth = waterLevelTransform.position.y - playerRefTest.transform.position.y;

        if (depth > 0f)
        {
            playerMovement.isSwimming = true;
            playerMovement.SetWaterSurface(waterLevelTransform);
        }
        else
        {
            playerMovement.isSwimming = false;
        }
    }
}
