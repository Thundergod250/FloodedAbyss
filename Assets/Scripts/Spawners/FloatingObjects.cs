using System.Collections.Generic;
using UnityEngine;

public class FloatingObjects : MonoBehaviour
{
    [Header("Floating Settings")]
    [SerializeField] private float floatingObjSinkVal = 5f;

    private Dictionary<Transform, Vector3> originalPositions = new Dictionary<Transform, Vector3>();
    private FloatingItem floatingItem;

    private void Start()
    {
        floatingItem = GetComponent<FloatingItem>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("OceanCollision")) return;

        if(!floatingItem.isSinking)
        {
            floatingItem.isSinking = true;
            this.gameObject.transform.position += new Vector3(0f, -floatingObjSinkVal, 0f);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("OceanCollision")) return;

        if (floatingItem.isSinking)
        {
            floatingItem.isSinking = false;
            this.gameObject.transform.position += new Vector3(0f, floatingObjSinkVal, 0f);
        }
    }
}
