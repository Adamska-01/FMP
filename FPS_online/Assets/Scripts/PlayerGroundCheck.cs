using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGroundCheck : MonoBehaviour
{
    PlayerController playerController;

    private void Awake()
    {
        playerController = GetComponentInParent<PlayerController>();
    }
     

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == playerController.gameObject) //Safe check
            return; 

        playerController.SetGrounded(true);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject == playerController.gameObject) //Safe check
            return; 

        playerController.SetGrounded(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == playerController.gameObject) //Safe check
            return;

        playerController.SetGrounded(false);
    }
}
