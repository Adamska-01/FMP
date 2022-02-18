using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleShotGun : Gun
{
    [SerializeField] Camera cam; 
    public Transform bulletStart;
    public GameObject bulletPrefab; 
    public GameObject effectPrefab; 

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

            GameObject bullet = null;
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Debug.DrawLine(cam.transform.position, hit.point);
                bullet = Instantiate(bulletPrefab, bulletStart.position, Quaternion.LookRotation(hit.point - bulletStart.transform.position));
            }
            else
            {
                Debug.DrawLine(cam.transform.position, hit.point);
                bullet = Instantiate(bulletPrefab, bulletStart.position, Quaternion.LookRotation(cam.transform.forward));
            }

            Instantiate(effectPrefab, bulletStart.position, Quaternion.LookRotation(cam.transform.forward));

            //Assign damages
            bullet.GetComponent<Bullet>().SetDamages(((GunInfo)itemInfo).damageHead, ((GunInfo)itemInfo).damageBody, ((GunInfo)itemInfo).damageLeg);

            return true;
        }

        return false;
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
