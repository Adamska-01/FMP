using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance;
    private PhotonView pv;
    private GameObject controller;
    private GameObject deathCamera;

    private void Awake()
    {
        instance = this;
        pv = GetComponent<PhotonView>();
        deathCamera = GameObject.Find("DeathCamera");
    }

    private void Start()
    {
        if (pv.IsMine) //if pv is owned by the local player
        {
            CreateController();
            deathCamera.SetActive(false);
        }
    }

    public void CreateController()
    {
        //Instantiate player controller
        Transform spawnpoint = NETSpawner.instance.GetSpawnPoint();
        controller = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Player"), spawnpoint.position, spawnpoint.rotation, 0, new object[] { pv.ViewID });
    }

    public void Die(string _damager)
    {
        NETUIController.instance.deathText.text = "You were killed by <color=red>" + _damager + "</color>";

        //Update stats
        MatchManager.instance.UpdateStatsSend(PhotonNetwork.LocalPlayer.ActorNumber , 1, 1);

        if (controller != null)
        {
            StartCoroutine(DieCo());
        }
    }

    public IEnumerator DieCo()
    {
        //Death effect 
        PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "DeathEffect"), controller.transform.position + new Vector3(0.0f, 1.2f, 0.0f), Quaternion.identity);

        //Destroy player 
        PhotonNetwork.Destroy(controller);
        controller = null;

        deathCamera.SetActive(true);

        //Open death panel
        NETUIController.instance.OpenPanel(NETUIController.PanelType.DEATH);

        yield return new WaitForSeconds(5.0f);

        //Open HUD and close death
        NETUIController.instance.OpenPanel(NETUIController.PanelType.HUD);

        deathCamera.SetActive(false);

        //Respawn
        if(MatchManager.instance.state == MatchManager.GameStates.Playing && controller == null)
            CreateController();
    }
}
