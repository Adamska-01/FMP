using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private float velocity;
    private float damageHead, damageBody, damageLeg; 

    void Start()
    {
        velocity = 50.0f;
        StartCoroutine("DestroySelf");
    }
     
    void Update()
    {
        Vector3 movement = transform.forward * velocity * Time.deltaTime;
        float distance = movement.magnitude;

        if(Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, distance))
        {
            //Deal damage if is a player
            if (hit.collider.gameObject.TryGetComponent<HitboxPlayer>(out var hitbox))
            {
                switch (hitbox.colType)
                {
                    case HitboxPlayer.CollisionType.BODY:
                        hitbox.TakeDamage(damageBody);
                        break;
                    case HitboxPlayer.CollisionType.HEAD:
                        hitbox.TakeDamage(damageHead);
                        break;
                    case HitboxPlayer.CollisionType.LEG:
                        hitbox.TakeDamage(damageLeg);
                        break;
                }
            }

            Debug.Log(hit.collider.gameObject.name);
            Destroy(this.gameObject, Time.deltaTime);
        }
        
        transform.position += movement; 
    }

    private IEnumerator DestroySelf()
    { 
        yield return new WaitForSeconds(3.0f);
        Destroy(gameObject); 
    }

    public void SetDamages(float _head, float _body, float _leg)
    {
        damageHead = _head;
        damageBody = _body;
        damageLeg = _leg;
    }
}
