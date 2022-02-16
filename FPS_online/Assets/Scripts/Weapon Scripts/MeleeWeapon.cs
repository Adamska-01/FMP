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

    public override bool CanReload()
    {
        //No need to reload
        return true;
    }
     
    public override void Reload()
    { }

    private void Hit()
    {
        //TODO knife hit
    }
}
