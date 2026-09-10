using UnityEngine;
using System.Collections.Generic;

public class ObjectPooling : MonoBehaviour
{
    private Dictionary<GameObject, Queue<GameObject>> pools = new();
    
    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        // Ensure pool exists
        if (!pools.ContainsKey(prefab))
            pools[prefab] = new Queue<GameObject>();

        GameObject obj;

        // Reuse from pool if available
        if (pools[prefab].Count > 0)
        {
            obj = pools[prefab].Dequeue();
            obj.SetActive(true);
        }
        else
        {
            obj = Instantiate(prefab);
        }

        // Set transform
        obj.transform.SetPositionAndRotation(position, rotation);
        return obj;
    }
    
    public void Despawn(GameObject prefab, GameObject obj)
    {
        obj.SetActive(false);

        // Ensure pool exists
        if (!pools.ContainsKey(prefab))
            pools[prefab] = new Queue<GameObject>();

        pools[prefab].Enqueue(obj);
    }
}