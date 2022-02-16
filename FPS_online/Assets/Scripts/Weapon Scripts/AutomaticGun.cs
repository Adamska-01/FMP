using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutomaticGun : Gun
{
    [SerializeField] Camera cam;
    public float bulletVelocity = 200.0f;
    public Transform bulletStart;
    public GameObject bulletPrefab;

    //Ammo info
    public int ammoAvailable;
    private int currentAmmoInMagazine;
    public int maxAmmoInMagazine;
    public bool CanShoot { get { return currentAmmoInMagazine > 0.0f; } }

    private void Start()
    { 
        currentAmmoInMagazine = maxAmmoInMagazine;
    }


    public override void Use()
    {
        Shoot();
    }

    private void Shoot()
    {
        if(CanShoot)
        {
            AmmoConsumption();

            //Start ray from center of screen
            Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f));
            ray.origin = cam.transform.position;

            GameObject bullet = null;
            if(Physics.Raycast(ray, out RaycastHit hit))
            {
                Debug.DrawLine(cam.transform.position, hit.point);
                bullet = Instantiate(bulletPrefab, bulletStart.position, Quaternion.LookRotation(hit.point - bulletStart.transform.position)); 
            }
            else
            {
                Debug.DrawLine(cam.transform.position, hit.point);
                bullet = Instantiate(bulletPrefab, bulletStart.position, Quaternion.LookRotation(cam.transform.forward));
            }

            //Assign damages
            bullet.GetComponent<Bullet>().SetDamages(((GunInfo)itemInfo).damageHead, ((GunInfo)itemInfo).damageBody, ((GunInfo)itemInfo).damageLeg);
        }
    }

    private void AmmoConsumption()
    {
        currentAmmoInMagazine -= 1;
    }

    public override bool CanReload()
    {
        if (currentAmmoInMagazine < maxAmmoInMagazine && ammoAvailable > 0)
            return true;

        return false;
    }

    public override void Reload()
    {
        int availableSpaceInMagazine = maxAmmoInMagazine - currentAmmoInMagazine;
        int bulletsToAdd = Mathf.Min(availableSpaceInMagazine, ammoAvailable);

        ammoAvailable -= bulletsToAdd;
        currentAmmoInMagazine += bulletsToAdd;
    }
}
