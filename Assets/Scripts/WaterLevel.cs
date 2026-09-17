using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class WaterLevel : MonoBehaviour
{
    public Transform waterLevelTransform;

    public float floatDepth = 2;
    public float floatForce = 10;
    public float waterGravity = -10f;
    public float minimumFloatDepth = 0;

    private GameObject playerRefTest;
    private PlayerMovement playerMovement;


    private void Start()
    {
        playerRefTest = GameManager.Instance.playerController.gameObject;
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
