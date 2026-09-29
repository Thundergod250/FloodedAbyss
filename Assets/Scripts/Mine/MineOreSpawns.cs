using NUnit.Framework;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MineOreSpawns : MonoBehaviour
{
    [Header("Ore Spawn Locs")]
    public List<Transform> oreSpawnlocations = new List<Transform>();
    public Transform PlayerSpawnPoint;
}
