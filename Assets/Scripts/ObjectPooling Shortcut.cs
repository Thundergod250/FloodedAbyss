using UnityEngine;

public static class Pool
{
    // Standard GameObject Overloads
    public static GameObject Instantiate(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        return GameManager.Instance.objectPooling.Spawn(prefab, position, rotation);
    }

    public static GameObject Instantiate(GameObject prefab, Transform parent)
    {
        return GameManager.Instance.objectPooling.Spawn(prefab, parent);
    }

    public static GameObject Instantiate(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent)
    {
        return GameManager.Instance.objectPooling.Spawn(prefab, position, rotation, parent);
    }

    // Generic Component Overloads (e.g. UINotificationPanelPrefab)
    public static T Instantiate<T>(T componentPrefab, Transform parent) where T : Component
    {
        GameObject spawnedObj = GameManager.Instance.objectPooling.Spawn(componentPrefab.gameObject, parent);
        return spawnedObj.GetComponent<T>();
    }

    public static T Instantiate<T>(T componentPrefab, Vector3 position, Quaternion rotation, Transform parent = null) where T : Component
    {
        GameObject spawnedObj = GameManager.Instance.objectPooling.Spawn(componentPrefab.gameObject, position, rotation, parent);
        return spawnedObj.GetComponent<T>();
    }

    // Destroy / Despawn Overloads
    public static void Destroy(GameObject obj)
    {
        GameManager.Instance.objectPooling.Despawn(obj);
    }

    public static void Destroy(Component component)
    {
        GameManager.Instance.objectPooling.Despawn(component.gameObject);
    }

    public static void Destroy(GameObject obj, float time)
    {
        GameManager.Instance.objectPooling.Despawn(obj, time);
    }

    public static void Destroy(Component component, float time)
    {
        GameManager.Instance.objectPooling.Despawn(component.gameObject, time);
    }
}