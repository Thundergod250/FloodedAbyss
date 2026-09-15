using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using Unity.Mathematics;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class FloatingObject : MonoBehaviour
{
    [Header("Water Settings")]
    public WaterSurface targetSurface;
    public bool includeDeformers = true;
    public float verticalOffset = 0.0f;

    [Header("Follow Settings")]
    public bool followWaterCurrent = false;
    public float currentSpeedMultiplier = 1f;

    [Header("Collision Settings")]
    public bool useRigidbodyForCollision = false;
    public LayerMask obstacleLayers;

    private bool disableCurrentFlow = false;
    private WaterSearchParameters searchParameters = new WaterSearchParameters();
    private WaterSearchResult searchResult = new WaterSearchResult();
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (rb == null && useRigidbodyForCollision)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = !useRigidbodyForCollision;
        }
    }

    void Update()
    {
        if (targetSurface == null) return;

        searchParameters.startPositionWS = (float3)searchResult.candidateLocationWS;
        searchParameters.targetPositionWS = (float3)transform.position;
        searchParameters.error = 0.01f;
        searchParameters.maxIterations = 8;
        searchParameters.includeDeformation = includeDeformers;
        searchParameters.excludeSimulation = false;

        if (targetSurface.ProjectPointOnWaterSurface(searchParameters, out searchResult))
        {
            Vector3 projectedPosition = (Vector3)searchResult.projectedPositionWS;
            Vector3 newPosition = projectedPosition + Vector3.up * verticalOffset;

            if (followWaterCurrent && !disableCurrentFlow)
            {
                Vector3 currentDirection = (Vector3)searchResult.currentDirectionWS;
                newPosition += currentDirection * currentSpeedMultiplier * Time.deltaTime;
            }

            transform.position = newPosition;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!useRigidbodyForCollision) return;

        if (((1 << collision.gameObject.layer) & obstacleLayers) != 0)
        {
            disableCurrentFlow = true;
            followWaterCurrent = false;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (!useRigidbodyForCollision) return;

        if (((1 << collision.gameObject.layer) & obstacleLayers) != 0)
        {
            disableCurrentFlow = false;
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(FloatingObject))]
public class SimpleFloatingObjectEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        FloatingObject obj = (FloatingObject)target;

        if (obj.useRigidbodyForCollision)
        {
            Collider col = obj.GetComponent<Collider>();

            if (col == null)
            {
                EditorGUILayout.HelpBox("This needs collider for rb.", MessageType.Warning);
            }
        }
    }
}
#endif