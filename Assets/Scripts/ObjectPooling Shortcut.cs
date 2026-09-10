using UnityEngine;

public static class Pool
{
    public static GameObject Instantiate(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        return GameManager.Instance.objectPooling.Spawn(prefab, position, rotation);
    }

    public static void Destroy(GameObject obj)
    {
        GameManager.Instance.objectPooling.Despawn(obj);
    }

    public static void Destroy(GameObject obj, float time)
    {
        GameManager.Instance.objectPooling.Despawn(obj, time);
    }
}
