using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooling : MonoBehaviour
{
    [Header("Parent for pooled objects")]
    public Transform ParentObject; // assign your "Pooled" GameObject here

    private readonly Dictionary<GameObject, Queue<GameObject>> pools = new();
    private readonly Dictionary<GameObject, GameObject> instanceToPrefab = new();
    private readonly Dictionary<GameObject, Vector3> prefabDefaultScales = new();
    private readonly Dictionary<GameObject, Quaternion> prefabDefaultRotations = new();

    public GameObject Spawn(GameObject prefab, Transform parent = null) =>
        Spawn(prefab, Vector3.zero, Quaternion.identity, parent);

    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        if (prefab == null)
        {
            Debug.LogWarning("[ObjectPooling] Attempted to spawn a null prefab.");
            return null;
        }

        if (!pools.ContainsKey(prefab))
        {
            pools[prefab] = new Queue<GameObject>();
            prefabDefaultScales[prefab] = prefab.transform.localScale;
            prefabDefaultRotations[prefab] = prefab.transform.localRotation;
        }

        GameObject obj = null;

        while (pools[prefab].Count > 0 && obj == null)
        {
            obj = pools[prefab].Dequeue();
        }

        if (obj == null)
        {
            obj = Instantiate(prefab);
            instanceToPrefab[obj] = prefab;
        }

        // Set parent before setting position/rotation/scale so transform space remains correct
        obj.transform.SetParent(parent);

        Quaternion targetRotation = rotation == Quaternion.identity
            ? (prefabDefaultRotations.TryGetValue(prefab, out Quaternion cachedRot) ? cachedRot : prefab.transform.localRotation)
            : rotation;

        obj.transform.SetPositionAndRotation(position, targetRotation);

        // Preserve the prefab's authored localScale (e.g., 10, 10, 10 on world-space UI)
        Vector3 targetScale = prefabDefaultScales.TryGetValue(prefab, out Vector3 cachedScale)
            ? cachedScale
            : prefab.transform.localScale;
        obj.transform.localScale = targetScale;

        obj.SetActive(true);
        return obj;
    }

    public void Despawn(GameObject obj)
    {
        if (obj == null) return;

        if (!instanceToPrefab.TryGetValue(obj, out GameObject prefab))
        {
            Debug.LogWarning($"[ObjectPooling] Object {obj.name} not tracked in pool, destroying normally.");
            Destroy(obj);
            return;
        }

        obj.SetActive(false);

        // Put all despawned objects under pooled parent
        if (ParentObject != null)
            obj.transform.SetParent(ParentObject);

        if (!pools.ContainsKey(prefab))
            pools[prefab] = new Queue<GameObject>();

        pools[prefab].Enqueue(obj);
    }

    public void Despawn(GameObject obj, float delay)
    {
        if (obj == null) return;
        StartCoroutine(DespawnAfter(obj, delay));
    }

    private IEnumerator DespawnAfter(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        Despawn(obj);
    }
}