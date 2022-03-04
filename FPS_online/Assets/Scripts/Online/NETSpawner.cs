using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NETSpawner : MonoBehaviour
{
    public static NETSpawner instance;
    private void Awake()
    {
        instance = this;
        spawnpoints = GetComponentsInChildren<SpawnPoint>();
    }

    private SpawnPoint[] spawnpoints;
    
    public Transform GetSpawnPoint()
    {
        return spawnpoints[Random.Range(0, spawnpoints.Length)].transform;
    }
}
