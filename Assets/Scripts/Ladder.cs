using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Ladder : MonoBehaviour
{
    [Header("Auto Collider Setup")]
    [SerializeField] private bool autoFitColliderOnStart = true;
    [SerializeField] private float triggerDepth = 0.6f;

    private BoxCollider triggerCollider;

    private void Awake()
    {
        triggerCollider = GetComponent<BoxCollider>();
        triggerCollider.isTrigger = true;

        if (autoFitColliderOnStart)
        {
            FitColliderToChildMeshes();
        }
    }

    [ContextMenu("Fit Collider To Child Meshes")]
    public void FitColliderToChildMeshes()
    {
        Bounds localBounds = CalculateLocalMeshBounds(transform);

        if (triggerCollider == null) triggerCollider = GetComponent<BoxCollider>();
        triggerCollider.isTrigger = true;
        triggerCollider.center = localBounds.center + new Vector3(0f, 0f, triggerDepth * 0.25f);
        triggerCollider.size = new Vector3(localBounds.size.x * 1.1f, localBounds.size.y, localBounds.size.z + triggerDepth);

        BoxCollider solidCollider = GetSolidCollider();
        solidCollider.isTrigger = false;
        solidCollider.center = localBounds.center;
        solidCollider.size = localBounds.size;
    }

    private Bounds CalculateLocalMeshBounds(Transform root)
    {
        MeshFilter[] filters = root.GetComponentsInChildren<MeshFilter>();
        if (filters.Length == 0) return new Bounds(Vector3.zero, Vector3.one);

        Bounds localBounds = new Bounds();
        bool hasBounds = false;

        foreach (MeshFilter filter in filters)
        {
            Mesh mesh = filter.sharedMesh;
            if (mesh == null) continue;

            Bounds meshBounds = mesh.bounds;
            Vector3[] localCorners = new Vector3[8]
            {
                new Vector3(meshBounds.min.x, meshBounds.min.y, meshBounds.min.z),
                new Vector3(meshBounds.min.x, meshBounds.min.y, meshBounds.max.z),
                new Vector3(meshBounds.min.x, meshBounds.max.y, meshBounds.min.z),
                new Vector3(meshBounds.min.x, meshBounds.max.y, meshBounds.max.z),
                new Vector3(meshBounds.max.x, meshBounds.min.y, meshBounds.min.z),
                new Vector3(meshBounds.max.x, meshBounds.min.y, meshBounds.max.z),
                new Vector3(meshBounds.max.x, meshBounds.max.y, meshBounds.min.z),
                new Vector3(meshBounds.max.x, meshBounds.max.y, meshBounds.max.z)
            };

            foreach (Vector3 corner in localCorners)
            {
                Vector3 worldPoint = filter.transform.TransformPoint(corner);
                Vector3 rootLocalPoint = root.InverseTransformPoint(worldPoint);

                if (!hasBounds)
                {
                    localBounds = new Bounds(rootLocalPoint, Vector3.zero);
                    hasBounds = true;
                }
                else
                {
                    localBounds.Encapsulate(rootLocalPoint);
                }
            }
        }

        return hasBounds ? localBounds : new Bounds(Vector3.zero, Vector3.one);
    }

    private BoxCollider GetSolidCollider()
    {
        BoxCollider[] colliders = GetComponents<BoxCollider>();
        foreach (BoxCollider col in colliders)
        {
            if (!col.isTrigger) return col;
        }
        return gameObject.AddComponent<BoxCollider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerMovement pm = other.GetComponent<PlayerMovement>();
        if (pm != null)
        {
            pm.RegisterLadder();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerMovement pm = other.GetComponent<PlayerMovement>();
        if (pm != null)
        {
            pm.UnregisterLadder();
        }
    }
}