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
        //Shuffle spawn points 
        SpawnPoint[] spawnP = new SpawnPoint[spawnpoints.Length];
        spawnpoints.CopyTo(spawnP, 0);
        Shuffle(spawnP);

        //Find a suitable spawn point (far from enemies in sight)
        for (int i = 0; i < spawnP.Length; i++)
        {
            Collider[] targetsInView = Physics.OverlapSphere(spawnP[i].transform.position, 25.0f);
            bool canSpawnHere = true;
            for (int j = 0; j < targetsInView.Length; j++)
            {
                if(targetsInView[j].transform.root.GetComponent<NETPlayerController>() != null)
                {
                    Transform target = targetsInView[j].gameObject.transform;
                    Vector3 dirToTarget = (target.position - spawnP[i].transform.position).normalized;
                    if(Vector3.Angle(spawnP[i].transform.forward, dirToTarget) < (60.0f/2.0f)) //60.0f == view angle (default FOV of the camera)
                    {
                        float dstToTarget = Vector3.Distance(spawnP[i].transform.position, target.position);
                        if(Physics.Raycast(spawnP[i].transform.position, dirToTarget, out RaycastHit hit, dstToTarget))
                        {
                            if (hit.collider.transform.root.GetComponent<NETPlayerController>() != null)
                            {
                                canSpawnHere = false;
                                break;
                            }
                        }
                    }
                }
            } 

            if(canSpawnHere && !spawnP[i].IsOccupied)
            {
                Debug.Log("I was spawned with the algorithm");
                return spawnP[i].transform;
            }
        }

        Debug.Log("Random spawn");
        int tries = 0, maximumTries = 10;
        while(tries <= 10)
        {
            SpawnPoint sp = spawnpoints[Random.Range(0, spawnpoints.Length)];
            if (!sp.IsOccupied)
                return sp.transform;

            tries++;
        }

        //Return random if no suitable spawn point is found 
        return spawnpoints[Random.Range(0, spawnpoints.Length)].transform;
    }

    private void Shuffle(object[] arr)
    {
        System.Random rand = new System.Random();
        for (int i = arr.Length - 1; i >= 1; i--)
        {
            int j = rand.Next(i + 1);
            object tmp = arr[j];
            arr[j] = arr[i];
            arr[i] = tmp;
        }
    }
}
