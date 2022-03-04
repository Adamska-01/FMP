using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class PlayerManager : MonoBehaviour
{
    private PhotonView pv;
    private GameObject controller; 

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
    }

    private void Start()
    {
        if(pv.IsMine) //if pv is owned by the local player
        {
            CreateController();
        }
    }

    private void CreateController()
    {
        //Instantiate player controller
        Transform spawnpoint = NETSpawner.instance.GetSpawnPoint();
        controller = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Player"), spawnpoint.position, spawnpoint.rotation, 0, new object[] { pv.ViewID });
    }

    public void Die()
    {
        //Death particle 
        PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "DeathEffect"), controller.transform.position + new Vector3(0.0f, 1.2f, 0.0f), Quaternion.identity);
        
        PhotonNetwork.Destroy(controller);
        CreateController();
    }
}
