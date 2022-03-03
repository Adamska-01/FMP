using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NETSingleShotGun : NETGun
{ 
    public Transform bulletStart;

    public override bool Use()
    {
        return Shoot();
    }

    private bool Shoot()
    {
        if (CanShoot)
        {
            AmmoConsumption();

            StartCoroutine(FireRateDelay());

            //Set state (for animation)
            player.isFiringSingleShot = true;

            //Start ray from center of screen
            Vector2 recoil = player.isAiming ? Vector2.zero : Recoil();
            Ray ray = cam.ViewportPointToRay(new Vector3(0.5f + recoil.x, 0.5f + recoil.y));
            ray.origin = cam.transform.position;
             
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                GetComponent<PhotonView>().RPC("RPC_ShootBullet", RpcTarget.All, bulletStart.position, Quaternion.LookRotation(hit.point - bulletStart.transform.position));
            }
            else
            {
                GetComponent<PhotonView>().RPC("RPC_ShootBullet", RpcTarget.All, bulletStart.position, Quaternion.LookRotation(cam.transform.forward));
            } 

            return true;
        }

        return false;
    }

    [PunRPC]
    private void RPC_ShootBullet(Vector3 _pos, Quaternion _rot)
    {
        GameObject projectile = Instantiate(bulletPrefab, _pos, _rot);
        //Assign damages
        projectile.GetComponent<NETBullet>().SetDamages(((GunInfo)itemInfo).damageHead, ((GunInfo)itemInfo).damageBody, ((GunInfo)itemInfo).damageLeg);
        projectile.GetComponent<NETBullet>().pv = GetComponent<PhotonView>();

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