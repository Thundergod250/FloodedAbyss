using UnityEngine;
using System.Collections.Generic;

public class ObjectPooling : MonoBehaviour
{
    private Dictionary<GameObject, Queue<GameObject>> pools = new();
    private Dictionary<GameObject, GameObject> instanceToPrefab = new(); // track which prefab spawned each instance

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
            instanceToPrefab[obj] = prefab; // record prefab ownership
        }

        obj.transform.SetPositionAndRotation(position, rotation);
        return obj;
    }

    // Immediate despawn (like Destroy(obj))
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
        pools[prefab].Enqueue(obj);
    }

    // Delayed despawn (like Destroy(obj, time))
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