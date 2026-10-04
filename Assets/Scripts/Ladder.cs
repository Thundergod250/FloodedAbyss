using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(BoxCollider))]
public class Ladder : MonoBehaviour
{
    [Header("Ladder Dimensions")]
    [SerializeField] private float height = 6f;
    [SerializeField] private float width = 1f;
    [SerializeField] private int rungCount = 8;
    [SerializeField] private float climbSpeed = 4f;

    [Header("Collision & Visuals")]
    [SerializeField] private bool createSolidBacking = true;
    [SerializeField] private Material ladderMaterial;

    private BoxCollider triggerCollider;

    private void Awake()
    {
        SetupTriggerCollider();
        BuildPrimitiveLadder();

        if (createSolidBacking)
        {
            SetupBackingCollider();
        }
    }

    private void SetupTriggerCollider()
    {
        triggerCollider = GetComponent<BoxCollider>();
        triggerCollider.isTrigger = true;

        // Position trigger slightly forward so the player enters the climb zone before touching the backing
        triggerCollider.center = new Vector3(0f, height / 2f, 0.2f);
        triggerCollider.size = new Vector3(width + 0.2f, height, 0.8f);
    }

    /// <summary>
    /// Adds a solid non-trigger box collider directly behind the rungs so the player cannot clip through.
    /// </summary>
    private void SetupBackingCollider()
    {
        BoxCollider backingCollider = gameObject.AddComponent<BoxCollider>();
        backingCollider.isTrigger = false;
        backingCollider.center = new Vector3(0f, height / 2f, -0.05f);
        backingCollider.size = new Vector3(width, height, 0.1f);
    }

    private void BuildPrimitiveLadder()
    {
        float railThickness = 0.1f;
        float rungThickness = 0.08f;

        // Left Rail
        GameObject leftRail = GameObject.CreatePrimitive(PrimitiveType.Cube);
        leftRail.name = "LeftRail";
        leftRail.transform.SetParent(transform, false);
        leftRail.transform.localPosition = new Vector3(-width / 2f, height / 2f, 0f);
        leftRail.transform.localScale = new Vector3(railThickness, height, railThickness);

        // Right Rail
        GameObject rightRail = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rightRail.name = "RightRail";
        rightRail.transform.SetParent(transform, false);
        rightRail.transform.localPosition = new Vector3(width / 2f, height / 2f, 0f);
        rightRail.transform.localScale = new Vector3(railThickness, height, railThickness);

        if (ladderMaterial != null)
        {
            leftRail.GetComponent<Renderer>().sharedMaterial = ladderMaterial;
            rightRail.GetComponent<Renderer>().sharedMaterial = ladderMaterial;
        }

        Destroy(leftRail.GetComponent<Collider>());
        Destroy(rightRail.GetComponent<Collider>());

        // Rungs
        float spacing = height / (rungCount + 1);
        for (int i = 1; i <= rungCount; i++)
        {
            GameObject rung = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rung.name = $"Rung_{i}";
            rung.transform.SetParent(transform, false);
            rung.transform.localPosition = new Vector3(0f, spacing * i, 0f);
            rung.transform.localScale = new Vector3(width, rungThickness, rungThickness);

            if (ladderMaterial != null)
            {
                rung.GetComponent<Renderer>().sharedMaterial = ladderMaterial;
            }

            Destroy(rung.GetComponent<Collider>());
        }
    }

    private void OnTriggerStay(Collider other)
    {
        PlayerMovement pm = other.GetComponent<PlayerMovement>();
        if (pm == null) return;

        // Set climbing state on player
        pm.isClimbing = true;

        float verticalInput = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
                verticalInput += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
                verticalInput -= 1f;
        }

        if (Mathf.Abs(verticalInput) > 0.1f)
        {
            Vector3 climbVelocity = Vector3.up * (verticalInput * climbSpeed * Time.deltaTime);

            CharacterController cc = other.GetComponent<CharacterController>();
            if (cc != null)
            {
                cc.Move(climbVelocity);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerMovement pm = other.GetComponent<PlayerMovement>();
        if (pm != null)
        {
            pm.isClimbing = false;
        }

        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = true;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(new Vector3(0f, height / 2f, 0f), new Vector3(width, height, 0.2f));
    }
}