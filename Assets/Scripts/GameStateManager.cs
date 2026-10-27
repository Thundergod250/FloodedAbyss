using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameStateManager : MonoBehaviour
{
    [Header("ROcky")]
    [SerializeField] private List<GameObject> rocksToDestroy = new List<GameObject>();
    [SerializeField] private int rockRequiredToDestroy = 5;

    [Header("Cathedral")]
    [SerializeField] private Transform cathedral;
    [SerializeField] private int cathedralHeightIncrease;

    private int lastRockCount;

    private void Update()
    {
        int destroyedRocks = 0;

        foreach (GameObject rock in rocksToDestroy)
        {
            if (rock == null || !rock.activeSelf)
            {
                destroyedRocks++;
            }
        }

        if (destroyedRocks >= lastRockCount + rockRequiredToDestroy)
        {
            cathedral.position += Vector3.up * cathedralHeightIncrease;
            lastRockCount = destroyedRocks;
        }
    }
}
