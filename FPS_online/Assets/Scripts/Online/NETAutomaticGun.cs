using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class NETAutomaticGun : NETGun
{
    public Transform bulletStart;


    public override bool Use()
    {
        Debug.Log("I am instantiating the bullet");
        return Shoot();
    }

    private bool Shoot()
    {
        if (CanShoot)
        {
            AmmoConsumption();

            StartCoroutine(FireRateDelay());

            //Set state (for animation)
            player.isFiring = true;

            //Start ray from center of screen
            Vector2 recoil = player.isAiming ? Vector2.zero : Recoil();
            Ray ray = cam.ViewportPointToRay(new Vector3(0.5f + recoil.x, 0.5f + recoil.y));
            ray.origin = cam.transform.position;
             
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                GetComponent<PhotonView>().RPC("RPC_ShootBullet", RpcTarget.All, bulletStart.position, Quaternion.LookRotation(hit.point - bulletStart.transform.position), GetComponent<PhotonView>().Owner.NickName);
            }
            else
            {
                GetComponent<PhotonView>().RPC("RPC_ShootBullet", RpcTarget.All, bulletStart.position, Quaternion.LookRotation(cam.transform.forward), GetComponent<PhotonView>().Owner.NickName); 
            } 

            return true;
        }

        return false;
    }

    [PunRPC]
    private void RPC_ShootBullet(Vector3 _pos, Quaternion _rot, string _damager)
    {
        NETBullet projectile = Instantiate(bulletPrefab, _pos, _rot).GetComponent<NETBullet>();
        //Assign damages and other stuff
        projectile.SetDamages(((GunInfo)itemInfo).damageHead, ((GunInfo)itemInfo).damageBody, ((GunInfo)itemInfo).damageLeg);
        projectile.pv = GetComponent<PhotonView>();
        projectile.bulletOwner = _damager;

        //Effect
        Instantiate(effectPrefab, _pos, Quaternion.LookRotation(cam.transform.forward));
    }

    private void AmmoConsumption()
    {
        currentAmmoInMagazine -= 1;
    }

    public override bool CanReload()
    {
        return base.CanReload();
    }

    public override void Reload()
    {
        base.Reload();
    }
}
