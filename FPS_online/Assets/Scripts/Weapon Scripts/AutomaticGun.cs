using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutomaticGun : Gun
{
    [SerializeField] Camera cam;
    public float bulletVelocity = 200.0f;

    public override void Use()
    {
        Shoot();
    }

    private void Shoot()
    {
        //Start ray from center of screen
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        ray.origin = cam.transform.position;

        if(Physics.Raycast(ray, out RaycastHit hit))
        {
            if(hit.collider.gameObject.TryGetComponent<HitboxPlayer>(out var hitbox))
            {
                switch(hitbox.colType)
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
    }
}
