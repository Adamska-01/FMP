using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Recoil : MonoBehaviour
{
    [SerializeField] private PlayerController player;
    public float recoilIntensityCounter;
    public float recoilMaxIntensity;
    public float increaseRate, decreaseRate;
     
     
    void Update()
    {
        if (!player.isFiring && !player.isFiringSingleShot)
        {
            recoilIntensityCounter -= decreaseRate * Time.deltaTime;
            recoilIntensityCounter = Mathf.Clamp(recoilIntensityCounter, 0, recoilMaxIntensity);
        }
    }
}
