using UnityEngine;
using System.Collections.Generic;

public class ObjectPooling : MonoBehaviour
{
    [Header("Parent for pooled objects")]
    public Transform ParentObject; // assign your "Pooled" GameObject here

    private Dictionary<GameObject, Queue<GameObject>> pools = new();
    private Dictionary<GameObject, GameObject> instanceToPrefab = new();

    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (!pools.ContainsKey(prefab))
            pools[prefab] = new Queue<GameObject>();

        GameObject obj;

        if (pools[prefab].Count > 0)
        {
            obj = pools[prefab].Dequeue();
            obj.SetActive(true);
        }
        else
        {
            obj = Instantiate(prefab);
            instanceToPrefab[obj] = prefab;
        }

        obj.transform.SetPositionAndRotation(position, rotation);
        obj.transform.SetParent(null); // detach from pooled parent when active
        return obj;
    }

    public void Despawn(GameObject obj)
    {
        if (!instanceToPrefab.ContainsKey(obj))
        {
            Debug.LogWarning($"Object {obj.name} not tracked in pool, destroying normally.");
            Destroy(obj);
            return;
        }

        GameObject prefab = instanceToPrefab[obj];
        obj.SetActive(false);

        // Put all despawned objects under one parent
        if (ParentObject != null)
            obj.transform.SetParent(ParentObject);

        pools[prefab].Enqueue(obj);
    }

    public void Despawn(GameObject obj, float delay)
    {
        StartCoroutine(DespawnAfter(obj, delay));
    }

    private System.Collections.IEnumerator DespawnAfter(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        Despawn(obj);
    }
}