using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyStats : MonoBehaviour, IDamageable
{
    //max values
    public float MAX_HEALTH_VALUE = 100.0f;
    public float MAX_ARMOUR_VALUE = 50.0f;

    //health
    private float HealthValue;
    //Stamina
    private float ArmourValue;

    private bool isDead;
     
    void Start()
    {
        HealthValue = MAX_HEALTH_VALUE;
        ArmourValue = MAX_ARMOUR_VALUE;
    }

    //Changed by Mattie to FixedUpdate
    void FixedUpdate()
    {
        updateUI();
    }


    private void updateUI() //For now there is no UI
    {
        HealthValue = Mathf.Clamp(HealthValue, 0, MAX_HEALTH_VALUE);
        ArmourValue = Mathf.Clamp(ArmourValue, 0, MAX_ARMOUR_VALUE);
    }

    public bool CanHeal()
    {
        return false; 
    }

    public void Heal(float value)
    { }

    public void TakeDamage(float damage, string _damager)
    {
        if (!isDead)
        {
            Debug.Log("Took Damage: " + damage);
            if (ArmourValue > 0.0f)
            {
                ArmourValue -= damage;
                if (ArmourValue < 0.0f)
                    HealthValue += ArmourValue;
            }
            else
                HealthValue -= damage;

            if (HealthValue <= 0)
            {
                Destroy(gameObject);
                Debug.Log("Dead");
            }

            //TODO: Update UI
        }
    } 
}
