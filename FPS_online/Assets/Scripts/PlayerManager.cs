using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class PlayerManager : MonoBehaviour
{
    PhotonView pv;
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

    void CreateController()
    {
        //Instantiate player controller
        PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Player_Controller"), Vector3.zero, Quaternion.identity);
    }

}
