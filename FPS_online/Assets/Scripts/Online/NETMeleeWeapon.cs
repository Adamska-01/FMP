using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NETMeleeWeapon : NETGun
{
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
        if (canShootNextBullet)
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
                            hitbox.TakeDamage(((GunInfo)itemInfo).damageBody, transform.root.GetComponent<PhotonView>().Owner.NickName);
                            break;
                        case HitboxPlayer.CollisionType.HEAD:
                            hitbox.TakeDamage(((GunInfo)itemInfo).damageHead, transform.root.GetComponent<PhotonView>().Owner.NickName);
                            break;
                        case HitboxPlayer.CollisionType.LEG:
                            hitbox.TakeDamage(((GunInfo)itemInfo).damageLeg, transform.root.GetComponent<PhotonView>().Owner.NickName);
                            break;
                    }
                }

                ImpactsAndHoles impactsAndHoles = FindObjectOfType<ImpactsAndHoles>();
                switch (hit.collider.tag)
                {
                    case "Concrete":
                        Instantiate(impactsAndHoles.GetBulletsAndImpacts()[ImpactsAndHoles.ImpactType.CONCRETE].impact, hit.point, Quaternion.LookRotation(hit.normal, Vector3.up));
                        Instantiate(impactsAndHoles.GetBulletsAndImpacts()[ImpactsAndHoles.ImpactType.CONCRETE].hole, hit.point + (hit.normal * 0.001f), Quaternion.LookRotation(hit.normal, Vector3.up));
                        break;
                    case "Dirt":
                        Instantiate(impactsAndHoles.GetBulletsAndImpacts()[ImpactsAndHoles.ImpactType.DIRT].impact, hit.point, Quaternion.LookRotation(hit.normal, Vector3.up));
                        Instantiate(impactsAndHoles.GetBulletsAndImpacts()[ImpactsAndHoles.ImpactType.DIRT].hole, hit.point + (hit.normal * 0.001f), Quaternion.LookRotation(hit.normal, Vector3.up));
                        break;
                    case "Metal":
                        Instantiate(impactsAndHoles.GetBulletsAndImpacts()[ImpactsAndHoles.ImpactType.METAL].impact, hit.point, Quaternion.LookRotation(hit.normal, Vector3.up));
                        Instantiate(impactsAndHoles.GetBulletsAndImpacts()[ImpactsAndHoles.ImpactType.METAL].hole, hit.point + (hit.normal * 0.001f), Quaternion.LookRotation(hit.normal, Vector3.up));
                        break;
                    case "Sand":
                        Instantiate(impactsAndHoles.GetBulletsAndImpacts()[ImpactsAndHoles.ImpactType.SAND].impact, hit.point, Quaternion.LookRotation(hit.normal, Vector3.up));
                        Instantiate(impactsAndHoles.GetBulletsAndImpacts()[ImpactsAndHoles.ImpactType.SAND].hole, hit.point + (hit.normal * 0.001f), Quaternion.LookRotation(hit.normal, Vector3.up));
                        break;
                    case "Wood":
                        Instantiate(impactsAndHoles.GetBulletsAndImpacts()[ImpactsAndHoles.ImpactType.WOOD].impact, hit.point, Quaternion.LookRotation(hit.normal, Vector3.up));
                        Instantiate(impactsAndHoles.GetBulletsAndImpacts()[ImpactsAndHoles.ImpactType.WOOD].hole, hit.point + (hit.normal * 0.001f), Quaternion.LookRotation(hit.normal, Vector3.up));
                        break;
                    case "Body":
                        Instantiate(impactsAndHoles.GetBulletsAndImpacts()[ImpactsAndHoles.ImpactType.BODY].impact, hit.point, Quaternion.LookRotation(hit.normal, Vector3.up));
                        break;
                    default:
                        Instantiate(impactsAndHoles.GetBulletsAndImpacts()[ImpactsAndHoles.ImpactType.CONCRETE].impact, hit.point, Quaternion.LookRotation(hit.normal, Vector3.up));
                        break;
                }
            }

            return true;
        }

        return false;
    }
}
