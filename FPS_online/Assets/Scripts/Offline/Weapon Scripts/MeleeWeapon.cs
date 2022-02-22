using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeWeapon : Gun
{
    [SerializeField] Camera cam; 


    public override bool Use()
    {
        return Hit();
    }

    public override bool CanReload()
    {
        //No need to reload
        return false;
    }
     
    public override void Reload()
    { }

    private bool Hit()
    {
        if(canShootNextBullet)
        {
            StartCoroutine(FireRateDelay()); 

            //Start ray from center of screen
            Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f));
            ray.origin = cam.transform.position;
             
            if (Physics.Raycast(ray, out RaycastHit hit, 0.9f))
            {
                Debug.DrawLine(cam.transform.position, hit.point);
                if (hit.collider.gameObject.TryGetComponent<HitboxPlayer>(out var hitbox))
                {
                    switch (hitbox.colType)
                    {
                        case HitboxPlayer.CollisionType.BODY:
                            hitbox.TakeDamage(((GunInfo)itemInfo).damageBody);
                            break;
                        case HitboxPlayer.CollisionType.HEAD:
                            hitbox.TakeDamage(((GunInfo)itemInfo).damageHead);
                            break;
                        case HitboxPlayer.CollisionType.LEG:
                            hitbox.TakeDamage(((GunInfo)itemInfo).damageLeg);
                            break;
                    }
                }
            }

            return true;
        }

        return false;
    }
}
