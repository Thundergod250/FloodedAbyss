using System.Collections.Generic;
using UnityEngine;

public class ObjectPooling : MonoBehaviour
{
    [Header("Parent for pooled objects")]
    public Transform ParentObject; // assign your "Pooled" GameObject here

    private Dictionary<GameObject, Queue<GameObject>> pools = new();
    private Dictionary<GameObject, GameObject> instanceToPrefab = new();

    public GameObject Spawn(GameObject prefab, Transform parent = null) => 
        Spawn(prefab, Vector3.zero, Quaternion.identity, parent);

    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        if (!pools.ContainsKey(prefab))
            pools[prefab] = new Queue<GameObject>();

        GameObject obj;

        if (pools[prefab].Count > 0)
            obj = pools[prefab].Dequeue();
        else
        {
            obj = Instantiate(prefab);
            instanceToPrefab[obj] = prefab;
        }

        // Set parent before setting position/rotation so transform space remains correct
        obj.transform.SetParent(parent);
        obj.transform.SetPositionAndRotation(position, rotation);
        
        // Ensure standard local scale is preserved for UI objects
        obj.transform.localScale = Vector3.one;
        
        obj.SetActive(true);
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

        // Put all despawned objects under pooled parent
        if (ParentObject != null)
            obj.transform.SetParent(ParentObject);

        pools[prefab].Enqueue(obj);
    }

    public void Despawn(GameObject obj, float delay) => 
        StartCoroutine(DespawnAfter(obj, delay));

    private System.Collections.IEnumerator DespawnAfter(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        Despawn(obj);
    }
}