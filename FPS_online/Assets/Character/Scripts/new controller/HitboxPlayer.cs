using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitboxPlayer : MonoBehaviour
{
    public enum CollisionType
    {
        HEAD,
        BODY,
        LEG
    }
    public CollisionType colType;
    public PlayerStats damageable;

    public void TakeDamage(float _dmg)
    {
        damageable.TakeDamage(_dmg);
    }
}
