using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeWeapon : Gun
{
    [SerializeField] Camera cam;   
     
      
    public override void Use()
    {
        Hit();
    }

    private void Hit()
    {
        //TODO knife hit
    }
}
