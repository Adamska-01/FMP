using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Gun : Item
{
    //Clip info
    public int ammoAvailable;
    public int currentAmmoInMagazine;
    public int maxAmmoInMagazine;
    private bool canShootNextBullet;
    public bool CanShoot { get { return currentAmmoInMagazine > 0.0f && canShootNextBullet; } }

    void Start()
    {
        //Start with full reloaded weapon
        currentAmmoInMagazine = maxAmmoInMagazine;

        canShootNextBullet = true;
    }

    public abstract override void Use();

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

    protected IEnumerator FireRateDelay()
    {
        canShootNextBullet = false;

        yield return new WaitForSeconds(((GunInfo)itemInfo).fireRate);
        
        canShootNextBullet = true;
    }
}
